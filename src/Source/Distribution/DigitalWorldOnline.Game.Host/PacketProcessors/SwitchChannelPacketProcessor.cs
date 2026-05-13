using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.MapServer;
using DigitalWorldOnline.Game.Services;
using DigitalWorldOnline.GameHost;
using MediatR;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pGame::ChangeChannel</c> (1050) — client requests a switch to
    /// the given channel idx on its current map.  Phase E Step 4.
    ///
    /// Payload (matches <c>cCliGame::SendChangeChannel</c>):
    /// <code>u4 targetChannelIdx</code>
    ///
    /// Flow:
    /// 1. Validate target channel exists + is initialised + has headroom.
    /// 2. Reject path — send an <see cref="AvailableChannelsPacket"/> with the
    ///    target slot's load forced to <c>0xFF</c>.  The client's
    ///    <c>ChannelContents::SetChannelStateInfo</c> consumes that and fires
    ///    <c>CHANGE_CHANNEL_FAIL</c> through the usual portal-failure path.
    /// 3. Accept path — remove from the old channel, update
    ///    <c>character.Channel</c>, persist, and send a <see cref="MapSwapPacket"/>
    ///    keyed to the same MapId.  Client treats it as a portal-success and
    ///    re-enters the map; <c>MapServer.AddClient</c> then picks the channel
    ///    matching the persisted <c>character.Channel</c> (Phase E Step 6).
    /// </summary>
    public class SwitchChannelPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ChangeChannel;

        private const string GameServerPublic = "GameServer:PublicAddress";
        private const string GameServerPort = "GameServer:Port";

        /// <summary>Soft cap per channel; arrivals above this trigger reject.</summary>
        private const int ChannelMaxPopulation = 250;

        private readonly MapRegistry _registry;
        private readonly MapServer _mapServer;
        private readonly ISender _sender;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly OwnerStorageFlushService _ownerStorageFlushService;

        public SwitchChannelPacketProcessor(
            MapRegistry registry,
            MapServer mapServer,
            ISender sender,
            IConfiguration configuration,
            ILogger logger,
            OwnerStorageFlushService ownerStorageFlushService)
        {
            _registry = registry;
            _mapServer = mapServer;
            _sender = sender;
            _configuration = configuration;
            _logger = logger;
            _ownerStorageFlushService = ownerStorageFlushService;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            uint targetChannelIdxRaw = (uint)packet.ReadInt();
            if (targetChannelIdxRaw > byte.MaxValue)
            {
                _logger.Warning("ChangeChannel: tamer {TamerId} requested channel {Idx} (out of byte range) — rejected",
                    client.TamerId, targetChannelIdxRaw);
                SendChannelListWithReject(client, byte.MaxValue);
                return;
            }
            byte targetChannelIdx = (byte)targetChannelIdxRaw;

            // Dungeon clients aren't channel-aware.
            if (client.DungeonMap)
            {
                _logger.Warning("ChangeChannel: tamer {TamerId} is in a dungeon — ignored", client.TamerId);
                return;
            }

            // Same-channel no-op.
            if (client.Tamer.Channel == targetChannelIdx)
            {
                _logger.Debug("ChangeChannel: tamer {TamerId} already on channel {Idx}", client.TamerId, targetChannelIdx);
                return;
            }

            var mapId = client.Tamer.Location.MapId;
            var target = _registry.GetByMapAndChannel(MapTypeEnum.Default, mapId, targetChannelIdx);

            if (target == null || !target.Initialized)
            {
                _logger.Warning("ChangeChannel: tamer {TamerId} requested missing channel {Idx} on map {MapId} — rejected",
                    client.TamerId, targetChannelIdx, mapId);
                SendChannelListWithReject(client, targetChannelIdx);
                return;
            }

            if (target.Clients.Count >= ChannelMaxPopulation)
            {
                _logger.Information("ChangeChannel: tamer {TamerId} -> channel {Idx} rejected (full, {Pop}/{Cap})",
                    client.TamerId, targetChannelIdx, target.Clients.Count, ChannelMaxPopulation);
                SendChannelListWithReject(client, targetChannelIdx);
                return;
            }

            // Phase E Step 7 protection: refuse a switch while one is in-flight.
            // (Loading == another portal/channel-swap is mid-stream.)
            if (client.Tamer.State == CharacterStateEnum.Loading)
            {
                _logger.Information("ChangeChannel: tamer {TamerId} already loading — rejected",
                    client.TamerId);
                SendChannelListWithReject(client, targetChannelIdx);
                return;
            }

            // Accept — relocate.
            client.Tamer.SetCurrentChannel(targetChannelIdx);
            await _sender.Send(new UpdateCharacterChannelCommand(client.TamerId, targetChannelIdx));

            client.Tamer.UpdateState(CharacterStateEnum.Loading);
            await _sender.Send(new UpdateCharacterStateCommand(client.TamerId, CharacterStateEnum.Loading));

            await _ownerStorageFlushService.FlushForTransitionAsync(client);
            _mapServer.RemoveClient(client);

            // MapSwap to the same map at the current position — the client treats
            // it as a portal-success, fades to load, and on Ready our existing
            // pipeline calls MapServer.AddClient again.  AddClient now prefers
            // the channel matching client.Tamer.Channel (Phase E Step 6).
            client.Send(new MapSwapPacket(
                _configuration[GameServerPublic],
                _configuration[GameServerPort],
                client.Tamer.Location.MapId,
                client.Tamer.Location.X,
                client.Tamer.Location.Y)
                .Serialize());

            _logger.Information("ChangeChannel: tamer {TamerId} accepted switch to channel {Idx} on map {MapId}",
                client.TamerId, targetChannelIdx, mapId);
        }

        /// <summary>
        /// Send the current channel list with one slot forced to <c>0xFF</c> so
        /// the client interprets it as "channel unavailable" and fires
        /// <c>CHANGE_CHANNEL_FAIL</c>.  Other channel loads are accurate so the
        /// UI bar stays useful.
        /// </summary>
        private void SendChannelListWithReject(GameClient client, byte rejectedIdx)
        {
            var mapId = client.Tamer.Location.MapId;
            var loads = new Dictionary<byte, byte>();
            foreach (var c in _registry.GetChannelsOf(MapTypeEnum.Default, mapId))
                loads[c.Channel] = (byte)Math.Min(byte.MaxValue, c.Clients.Count);

            // Force the rejected slot to the "unavailable" sentinel.  0xFF
            // bypasses the GetChannelLoad bucketing on the writer side because
            // it's already above the highest load bin — client sees it as -1
            // (unused / failed).
            loads[rejectedIdx] = byte.MaxValue;

            client.Send(new AvailableChannelsPacket(loads));
        }

    }
}
