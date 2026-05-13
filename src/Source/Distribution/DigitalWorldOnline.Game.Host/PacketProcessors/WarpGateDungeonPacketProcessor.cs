using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.MapServer;
using DigitalWorldOnline.Game.Managers;
using DigitalWorldOnline.Game.Services;
using DigitalWorldOnline.GameHost;
using MediatR;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class WarpGateDungeonPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.WarpGateDungeon;

        private const string GamerServerPublic = "GameServer:PublicAddress";
        private const string GameServerPort = "GameServer:Port";

        private readonly PartyManager _partyManager;
        private readonly IConfiguration _configuration;
        private readonly AssetsLoader _assets;
        private readonly MapServer _mapServer;
        private readonly DungeonsServer _dungeonServer;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly OwnerStorageFlushService _ownerStorageFlushService;

        public WarpGateDungeonPacketProcessor(
            PartyManager partyManager,
            IConfiguration configuration,
            AssetsLoader assets,
            MapServer mapServer,
            ISender sender,
            ILogger logger,
            DungeonsServer dungeonServer,
            OwnerStorageFlushService ownerStorageFlushService)
        {
            _partyManager = partyManager;
            _configuration = configuration;
            _assets = assets;
            _mapServer = mapServer;
            _sender = sender;
            _logger = logger;
            _dungeonServer = dungeonServer;
            _ownerStorageFlushService = ownerStorageFlushService;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var portalId = packet.ReadInt();

            var portal = _assets.Portal.FirstOrDefault(x => x.Id == portalId);

            if (portal == null)
            {
                client.Send(new SystemMessagePacket($"Portal {portalId} not found."));
                _logger.Error($"Portal id {portalId} not found.");


                var mapId = client.Tamer.Location.MapId;

                var waypoints = await _sender.Send(new MapRegionListAssetsByMapIdQuery(mapId));
                if (waypoints == null || !waypoints.Regions.Any())
                {
                    client.Send(new SystemMessagePacket($"Map information not found for {mapId}"));
                    _logger.Error($"Map information not found for {mapId}");
                    return;
                }

                await _ownerStorageFlushService.FlushForTransitionAsync(client);
                if (client.DungeonMap)
                {
                    _dungeonServer.RemoveClient(client);
                }
                else
                {
                    _mapServer.RemoveClient(client);
                }

                var destination = waypoints.Regions.First();

                client.Tamer.NewLocation(mapId, destination.X, destination.Y);
                await _sender.Send(new UpdateCharacterLocationCommand(client.Tamer.Location));

                client.Tamer.Partner.NewLocation(mapId, destination.X, destination.Y);
                await _sender.Send(new UpdateDigimonLocationCommand(client.Tamer.Partner.Location));

                client.Tamer.UpdateState(CharacterStateEnum.Loading);
                await _sender.Send(new UpdateCharacterStateCommand(client.TamerId, CharacterStateEnum.Loading));

                client.Send(
                    new MapSwapPacket(
                        _configuration[GamerServerPublic],
                        _configuration[GameServerPort],
                        mapId,
                        destination.X,
                        destination.Y
                    )
                );
            }
            else
            {
                if (!await TryConsumePortalRequirementAsync(client, portal))
                {
                    return;
                }

                await _ownerStorageFlushService.FlushForTransitionAsync(client);
                if (client.DungeonMap)
                    _dungeonServer.RemoveClient(client);
                else
                    _mapServer.RemoveClient(client);

                client.Tamer.NewLocation(portal.DestinationMapId, portal.DestinationX, portal.DestinationY);
                await _sender.Send(new UpdateCharacterLocationCommand(client.Tamer.Location));

                client.Tamer.Partner.NewLocation(portal.DestinationMapId, portal.DestinationX, portal.DestinationY);
                await _sender.Send(new UpdateDigimonLocationCommand(client.Tamer.Partner.Location));

                client.Tamer.UpdateState(CharacterStateEnum.Loading);
                await _sender.Send(new UpdateCharacterStateCommand(client.TamerId, CharacterStateEnum.Loading));

                client.SetGameQuit(false);

                client.Send(
                    new MapSwapPacket(
                        _configuration[GamerServerPublic],
                        _configuration[GameServerPort],
                        client.Tamer.Location.MapId,
                        client.Tamer.Location.X,
                        client.Tamer.Location.Y
                    )
                );

                var party = _partyManager.FindParty(client.TamerId);
                if (party != null)
                {
                    party.UpdateMember(party[client.TamerId]);

                    _mapServer.BroadcastForTargetTamers(party.GetMembersIdList(),
                        new PartyMemberWarpGatePacket(party[client.TamerId]).Serialize());

                    _dungeonServer.BroadcastForTargetTamers(party.GetMembersIdList(),
                        new PartyMemberWarpGatePacket(party[client.TamerId]).Serialize());
                }
            }

            //client.Send(new SendHandler(client.Tamer.GeneralHandler));

        }

        private async Task<bool> TryConsumePortalRequirementAsync(GameClient client, Commons.Models.Asset.PortalAssetModel portal)
        {
            var npcAsset = _assets.Npcs.FirstOrDefault(x =>
                x.NpcId == portal.NpcId &&
                x.MapId == client.Tamer.Location.MapId);

            if (npcAsset == null)
                return true;

            if (portal.PortalIndex < 0 || portal.PortalIndex >= npcAsset.Portals.Count)
                return true;

            var npcPortal = npcAsset.Portals[portal.PortalIndex];
            var requirements = (npcPortal.PortalsAsset ?? new List<DigitalWorldOnline.Commons.DTOs.Assets.NpcPortalsAmountAssetModel>())
                .Where(x => x?.npcPortalsAsset != null)
                .SelectMany(x => x.npcPortalsAsset)
                .Where(x => x != null && x.Type != NpcResourceTypeEnum.None)
                .ToList();

            if (!requirements.Any())
                return true;

            foreach (var requirement in requirements)
            {
                if (requirement.Type == NpcResourceTypeEnum.Item)
                {
                    var requiredItemId = requirement.ItemId;
                    var requiredAmount = requirement.ResourceAmount > 0 ? requirement.ResourceAmount : 1;
                    var available = client.Tamer.Inventory.FindItemsById(requiredItemId).Sum(x => x.Amount);
                    if (available < requiredAmount)
                    {
                        client.Send(new SystemMessagePacket("Insufficient required item amount."));
                        _logger.Warning(
                            "Dungeon warp denied: tamer {TamerId} missing NPC-portal item {ItemId} x{Amount} for portal {PortalId}.",
                            client.TamerId, requiredItemId, requiredAmount, portal.Id);
                        return false;
                    }
                }
                else if (requirement.Type == NpcResourceTypeEnum.Money)
                {
                    var requiredBits = requirement.ItemId > 0 ? requirement.ItemId : requirement.ResourceAmount;
                    if (requiredBits > 0 && client.Tamer.Inventory.Bits < requiredBits)
                    {
                        client.Send(new SystemMessagePacket("Insufficient bits amount."));
                        _logger.Warning(
                            "Dungeon warp denied: tamer {TamerId} missing NPC-portal bits {Bits} for portal {PortalId}.",
                            client.TamerId, requiredBits, portal.Id);
                        return false;
                    }
                }
            }

            var itemsChanged = false;
            var bitsChanged = false;
            foreach (var requirement in requirements)
            {
                if (requirement.Type == NpcResourceTypeEnum.Item)
                {
                    var requiredAmount = requirement.ResourceAmount > 0 ? requirement.ResourceAmount : 1;
                    if (client.Tamer.Inventory.RemoveOrReduceItemsByItemId(requirement.ItemId, requiredAmount))
                    {
                        itemsChanged = true;
                    }
                }
                else if (requirement.Type == NpcResourceTypeEnum.Money)
                {
                    var requiredBits = requirement.ItemId > 0 ? requirement.ItemId : requirement.ResourceAmount;
                    if (requiredBits > 0 && client.Tamer.Inventory.RemoveBits(requiredBits))
                    {
                        bitsChanged = true;
                    }
                }
            }

            if (itemsChanged)
                await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));

            if (bitsChanged)
                await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));

            return true;
        }

    }
}
