using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.GameHost;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Replies to <c>pSvr::ChannelInfo</c> (1713) with the live channel-load
    /// snapshot for the client's current map.  Pulls channel state from
    /// <see cref="MapRegistry"/> (canonical per-channel population source).
    ///
    /// Dungeon clients never see the channel UI — early-return.
    /// </summary>
    public class ChannelsPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.Channels;

        private readonly MapRegistry _registry;
        private readonly ILogger _logger;

        public ChannelsPacketProcessor(MapRegistry registry, ILogger logger)
        {
            _registry = registry;
            _logger = logger;
        }

        public Task Process(GameClient client, byte[] packetData)
        {
            if (client.DungeonMap)
                return Task.CompletedTask;

            var mapId = client.Tamer.Location.MapId;
            var channels = _registry.GetChannelsOf(MapTypeEnum.Default, mapId);

            var loads = new Dictionary<byte, byte>();
            foreach (var c in channels)
                loads[c.Channel] = (byte)Math.Min(byte.MaxValue, c.Clients.Count);

            _logger.Debug("Sending channel list for map {MapId}: {Count} channels",
                mapId, loads.Count);

            client.Send(new AvailableChannelsPacket(loads));
            return Task.CompletedTask;
        }
    }
}
