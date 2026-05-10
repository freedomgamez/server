using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pCashShop::BuyRequest (3401) reply. Wire layout from
    /// <c>cCliGameShop.cpp:RecvBuyCashItemRequest_Steam</c>:
    /// <c>int nError · int nCash</c>.
    /// <para>
    /// The Steam pre-purchase handshake is irrelevant for this non-Steam build, but the
    /// v487 client's <c>SendCashShopBuyRequest_Steam</c> still fires under
    /// <c>VERSION_USA</c>. Server replies with <c>(0, total_cash)</c> so the client
    /// proceeds straight to the regular MultiBuy purchase flow.
    /// </para>
    /// </summary>
    public class CashShopBuyRequestSteamResponsePacket : PacketWriter
    {
        private const int PacketNumber = 3401;

        public CashShopBuyRequestSteamResponsePacket(int error, int totalCash)
        {
            Type(PacketNumber);
            WriteInt(error);
            WriteInt(totalCash);
        }
    }
}
