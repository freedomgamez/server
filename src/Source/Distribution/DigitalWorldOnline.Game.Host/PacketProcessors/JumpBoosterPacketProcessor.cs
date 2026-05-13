using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.MapServer;
using DigitalWorldOnline.Game.Services;
using DigitalWorldOnline.GameHost;
using MediatR;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class JumpBoosterPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.JumpBooster;

        private readonly MapServer _mapServer;
        private readonly DMBaseBinLoader _dmBase;
        private readonly IConfiguration _configuration;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly OwnerStorageFlushService _ownerStorageFlushService;

        private const string GamerServerPublic = "GameServer:PublicAddress";
        private const string GameServerPort = "GameServer:Port";

        public JumpBoosterPacketProcessor(
            MapServer mapServer,
            DMBaseBinLoader dmBase,
            IConfiguration configuration,
            ISender sender,
            ILogger logger,
            OwnerStorageFlushService ownerStorageFlushService)
        {
            _configuration = configuration;
            _mapServer = mapServer;
            _dmBase = dmBase;
            _sender = sender;
            _logger = logger;
            _ownerStorageFlushService = ownerStorageFlushService;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var vipEnabled = Convert.ToBoolean(packet.ReadByte());
            var slot = packet.ReadShort();
            var mapId = packet.ReadShort();

            if(mapId == 1 )
            {
                mapId = 3;
            }

            var waypoints = await _sender.Send(new MapRegionListAssetsByMapIdQuery(mapId));
            if (waypoints == null || !waypoints.Regions.Any())
            {
                client.Send(new SystemMessagePacket($"Map information not found for map Id {mapId}."));
                _logger.Warning($"Map information not found for map Id {mapId} on character {client.TamerId} jump booster.");
                return;
            }

            if (!vipEnabled)
            {
                var bombItem = client.Tamer.Inventory.FindItemBySlot(slot);
                if (bombItem == null || bombItem.ItemId == 0)
                {
                    client.Send(new SystemMessagePacket($"Unable to jump to {mapId}."));
                    _logger.Warning($"Invalid bomb item at slot {slot} for character {client.TamerId} jump booster.");
                    return;
                }

                // Validate the (item -> destination) pair against DMBase.bin section 4.
                // The client UI already filters destinations via IsUseJumpBuster (see
                // JumpBuster.cpp:114/170) — the server mirrors that allowlist so a desynced
                // or tampered client can't teleport to a map this bomb shouldn't reach.
                if (!_dmBase.Data.JumpBusterDestinations.TryGetValue(bombItem.ItemId, out var allowedMaps)
                    || !allowedMaps.Contains(mapId))
                {
                    client.Send(new SystemMessagePacket($"Bomb {bombItem.ItemId} cannot teleport to map {mapId}."));
                    _logger.Warning(
                        "Rejecting jump-booster from tamer {TamerId}: item {ItemId} not allowed to map {MapId} per DMBase.bin allowlist.",
                        client.TamerId, bombItem.ItemId, mapId);
                    return;
                }

                if (!client.Tamer.Inventory.RemoveOrReduceItem(bombItem, 1, slot))
                {
                    client.Send(new SystemMessagePacket($"Unable to jump to {mapId}."));
                    _logger.Warning($"Failed to consume bomb item at slot {slot} for character {client.TamerId} jump booster.");
                    return;
                }

                await _sender.Send(new UpdateItemCommand(bombItem));

                _logger.Verbose($"Character {client.TamerId} jumped to map {mapId} with bomb {bombItem.ItemId}.");
            }
            else
                _logger.Verbose($"Character {client.TamerId} jumped to map {mapId} with VIP");
            

            await _ownerStorageFlushService.FlushForTransitionAsync(client);
            _mapServer.RemoveClient(client);

            var destination = waypoints.Regions.First();

            client.Tamer.NewLocation(mapId, destination.X, destination.Y);
            await _sender.Send(new UpdateCharacterLocationCommand(client.Tamer.Location));

            client.Tamer.Partner.NewLocation(mapId, destination.X, destination.Y);
            await _sender.Send(new UpdateDigimonLocationCommand(client.Tamer.Partner.Location));

            client.Tamer.UpdateState(CharacterStateEnum.Loading);
            await _sender.Send(new UpdateCharacterStateCommand(client.TamerId, CharacterStateEnum.Loading));

            client.SetGameQuit(false);

            client.Send(new MapSwapPacket(
                _configuration[GamerServerPublic],
                _configuration[GameServerPort],
                client.Tamer.Location.MapId,
                client.Tamer.Location.X,
                client.Tamer.Location.Y)
                .Serialize());
        }

    }
}
