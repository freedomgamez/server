using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pCashShop::Balance</c> (3404). Empty payload — the client just
    /// asks "what's my current balance?" Fired by <c>NewCashshopContents._OpenCashshopWindow</c>
    /// on cash shop window open and after each successful purchase to refresh the
    /// balance display.
    /// <para>
    /// Replies with <see cref="CashShopCoinsPacket"/>(Premium, Silk). The same packet
    /// type is also pushed unsolicited at login from
    /// <c>ComplementarInformationPacketProcessor</c>; this processor handles the
    /// client-initiated path.
    /// </para>
    /// </summary>
    public class CashShopBalanceRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.CashShopBalanceRequest;

        private readonly ILogger _logger;

        public CashShopBalanceRequestPacketProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task Process(GameClient client, byte[] packetData)
        {
            client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
            _logger.Verbose(
                "CashShop balance refresh for tamer {TamerId}: Premium={Premium} Silk={Silk}.",
                client.TamerId, client.Premium, client.Silk);
            return Task.CompletedTask;
        }
    }
}
