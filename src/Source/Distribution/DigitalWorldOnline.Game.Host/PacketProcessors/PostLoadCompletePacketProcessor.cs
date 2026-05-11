using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.CharacterServer;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pSvr::Change</c> (1703) — sent by the client's
    /// <c>LoadingContents::_DataLoadComplete</c> → <c>cCliGame::SendChangeServer</c>
    /// after a <see cref="MapSwapPacket"/>-triggered loading screen finishes.
    ///
    /// What the client expects in reply: a server→client <c>pSvr::Change</c>
    /// (also 1703).  The client's <c>RecvChangeServer</c> sets
    /// <c>net::cmd = Cmd::ConnectGameServer</c>; the next idle tick calls
    /// <c>net::start(type::game, ip, port)</c> which <c>DoDisconnect</c>s the
    /// current socket and opens a fresh one to the address it received in the
    /// MapSwap.  The new socket then goes through the normal AccessCode
    /// handshake (handled by <c>InitialInformationPacketProcessor</c>), which
    /// reloads the character from DB.  Because
    /// <see cref="SwitchChannelPacketProcessor"/> already persisted
    /// <c>Character.Channel = target</c> before sending MapSwap,
    /// <c>MapServerBaseOperation.PickChannelFor</c> drops the player onto the
    /// new channel.
    ///
    /// Without this reply the client stays stuck on the loading screen
    /// forever, because it's waiting for the cue to reconnect.
    /// </summary>
    public class PostLoadCompletePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.PostLoadComplete;

        private readonly ILogger _logger;

        public PostLoadCompletePacketProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task Process(GameClient client, byte[] packetData)
        {
            _logger.Information("PostLoadComplete from tamer {TamerId} — sending Change ack so client reconnects",
                client.TamerId);

            // Bounce pSvr::Change back; client will close this socket, open a
            // fresh one, and re-run the AccessCode handshake on it.
            client.Send(new ConnectGameServerPacket().Serialize());
            return Task.CompletedTask;
        }
    }
}
