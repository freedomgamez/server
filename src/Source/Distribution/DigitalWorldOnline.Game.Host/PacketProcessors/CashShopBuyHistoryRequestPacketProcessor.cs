using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pCashShop::BuyHistory</c> (3412). Empty payload — the client wants
    /// the list of products the player has previously purchased so the catalog UI can
    /// dim already-bought items.
    /// <para>
    /// Server projects the <c>ItemId</c> column from each row in
    /// <c>client.Tamer.AccountBuyHistory</c> (an account-level <c>Shared_ItemList</c>
    /// of type <c>BuyHistory</c>, sized 255). The list stores full item metadata for
    /// schema parity with other warehouse types, but the wire only needs the product
    /// IDs themselves.
    /// </para>
    /// </summary>
    public class CashShopBuyHistoryRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.CashShopBuyHistoryRequest;

        private readonly ILogger _logger;

        public CashShopBuyHistoryRequestPacketProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task Process(GameClient client, byte[] packetData)
        {
            var history = client.Tamer?.AccountBuyHistory;
            var productIds = history?.Items
                .Where(i => i.ItemId > 0)
                .Select(i => i.ItemId)
                .ToList()
                ?? new List<int>();

            client.Send(new CashShopBuyHistoryPacket(productIds));
            _logger.Verbose(
                "CashShop buy-history sent for tamer {TamerId}: {Count} entries.",
                client.TamerId, productIds.Count);
            return Task.CompletedTask;
        }
    }
}
