using System.Collections.Concurrent;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Create;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Chat;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ShoutMessagePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ShoutMessage;

        private readonly MapServer _mapServer;
        private readonly DMBaseBinLoader _dmBase;
        private readonly ILogger _logger;
        private readonly ISender _sender;

        // Tamer-id → UTC of last successful shout. Per-tamer cooldown enforcement; the
        // cooldown duration itself is per-map and comes from DMBase.bin section 3
        // (CsBaseMapInfo.ShoutSec, in milliseconds — bin's `s_nShoutSec` is excel-imported
        // as `*1000` in BaseMng.cpp:612). Singleton registration → fields persist across
        // requests; ConcurrentDictionary handles parallel clients on different threads.
        private readonly ConcurrentDictionary<long, DateTime> _lastShoutAt = new();

        public ShoutMessagePacketProcessor(
            MapServer mapServer,
            DMBaseBinLoader dmBase,
            ILogger logger,
            ISender sender)
        {
            _mapServer = mapServer;
            _dmBase = dmBase;
            _logger = logger;
            _sender = sender;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            var message = packet.ReadString();

            if (client.Tamer.Level < 20)
            {
                client.Send(new SystemMessagePacket($"Tamer level 20 required for shout chat."));
                _logger.Verbose($"Character {client.TamerId} sent shout to map {client.Tamer.Location.MapId} but has insufficient tamer level.");
                return;
            }

            // Per-map shout cooldown (from DMBase.bin section 3). v487 default is 15000 ms
            // server-wide. If the map isn't in MapInfo, skip the check (rather than fail
            // hard) — better to drop the cooldown than to block shouting on every map that
            // isn't bin-listed.
            short mapId = client.Tamer.Location.MapId;
            uint cooldownMs = _dmBase.Data.MapInfo.TryGetValue(mapId, out var info) ? info.ShoutSec : 0u;
            if (cooldownMs > 0)
            {
                var now = DateTime.UtcNow;
                if (_lastShoutAt.TryGetValue(client.TamerId, out var lastShout))
                {
                    var elapsedMs = (now - lastShout).TotalMilliseconds;
                    if (elapsedMs < cooldownMs)
                    {
                        int remainSec = (int)Math.Ceiling((cooldownMs - elapsedMs) / 1000.0);
                        client.Send(new SystemMessagePacket($"Shout chat is on cooldown ({remainSec}s remaining)."));
                        _logger.Verbose("Character {TamerId} shout rate-limited on map {MapId}: {Elapsed}ms < {Cooldown}ms.",
                            client.TamerId, mapId, (int)elapsedMs, cooldownMs);
                        return;
                    }
                }
                _lastShoutAt[client.TamerId] = now;
            }

            _mapServer.BroadcastForMap(mapId, new ChatMessagePacket(message, ChatTypeEnum.Shout, client.Tamer.Name).Serialize());
            _logger.Verbose($"Character {client.TamerId} sent shout to map {mapId} with message {message}.");

            await _sender.Send(new CreateChatMessageCommand(ChatMessageModel.Create(client.TamerId, message)));
        }
    }
}