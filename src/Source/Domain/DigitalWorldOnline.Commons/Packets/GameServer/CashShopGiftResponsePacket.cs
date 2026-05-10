using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pCashShop::Gift (3403) reply. Wire layout from <c>GS2C_RECV_CASHSHOP_GIFT</c>
    /// + <c>cCliGameShop.cpp:RecvGiftCashItem</c>:
    /// <c>u2 nResult · n4 nRealCash · n4 nBonusCash · wstring szPeerTamerName ·
    /// n4 nGiftItemProductIDX</c>.
    /// <para>
    /// The client treats the result as the same nsCashShopResult set as MultiBuy
    /// (0=success, billing-error code on fail). The peer name is echoed back so the
    /// gift confirmation toast can include it. <c>wstring</c> on the wire uses the
    /// packet protocol's <c>[u1 byteLen][bytes][NUL]</c> framing — the v487 client's
    /// <c>cPacket::pop&lt;std::wstring&gt;</c> reads it as multibyte then converts
    /// via <c>nBase::m2w</c>; server's existing <c>WriteString</c> matches that frame.
    /// </para>
    /// </summary>
    public class CashShopGiftResponsePacket : PacketWriter
    {
        private const int PacketNumber = 3403;

        public const ushort ResultSuccess = 0;
        public const ushort ResultPeerNotFound = 2;     // BILLING_FAIL_NOT_EXIST_TAMER
        public const ushort ResultUnknownProduct = 4;   // BILLING_FAIL_GIFT_CASHITEM
        public const ushort ResultPriceMismatch = 4;
        public const ushort ResultInsufficientCash = 4;
        public const ushort ResultInternalError = 99;

        public CashShopGiftResponsePacket(
            ushort result,
            int realCashAfter,
            int bonusCashAfter,
            string peerTamerName,
            int giftProductId)
        {
            Type(PacketNumber);
            WriteShort((short)result);
            WriteInt(realCashAfter);
            WriteInt(bonusCashAfter);
            WriteString(peerTamerName ?? string.Empty);
            WriteInt(giftProductId);
        }
    }
}
