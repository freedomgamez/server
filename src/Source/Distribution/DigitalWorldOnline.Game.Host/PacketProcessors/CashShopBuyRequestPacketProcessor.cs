using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pCashShop::BuyRequest</c> (3401), the Steam pre-purchase handshake.
    /// Fired only by <c>SendCashShopBuyRequest_Steam</c> under <c>VERSION_USA</c>;
    /// the regular-build purchase flow uses MultiBuy directly.
    /// <para>
    /// This non-Steam server stubs the handshake by replying with
    /// <c>(error=0, totalCash=Premium+Silk)</c>, which the client interprets as
    /// "Steam OK, proceed" and falls into the normal MultiBuy purchase path.
    /// Wire format from <c>RecvBuyCashItemRequest_Steam</c>:
    /// <c>int nError, int nCash</c>.
    /// </para>
    /// </summary>
    public class CashShopBuyRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.CashShopBuyRequest;

        private readonly ILogger _logger;

        public CashShopBuyRequestPacketProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task Process(GameClient client, byte[] packetData)
        {
            int totalCash = client.Premium + client.Silk;
            client.Send(new CashShopBuyRequestSteamResponsePacket(error: 0, totalCash: totalCash));
            _logger.Verbose(
                "CashShop Steam pre-purchase stub for tamer {TamerId}: ack with totalCash={TotalCash}.",
                client.TamerId, totalCash);
            return Task.CompletedTask;
        }
    }
}
