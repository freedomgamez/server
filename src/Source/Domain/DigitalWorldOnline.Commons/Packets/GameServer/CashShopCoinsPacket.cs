using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pCashShop::Balance (3404) reply. Wire layout from <c>GS2C_RECV_CASHSHOP_BALANCE</c>
    /// + <c>cCliGameShop.cpp:RecvCashShopBalance</c>: <c>n4 m_nResult, n4 m_nBonusCash,
    /// n4 m_nHaveCash</c>. <c>m_nResult == 0</c> (SUCCESS) is the only value that lets
    /// the cash shop window stay open — anything else triggers
    /// <c>BILLING_RESULT_CASHITEM_NOT_PURCHASE</c> and force-closes
    /// (<c>NewCashshopContents.cpp:1669-1696</c>).
    /// <para>
    /// Pushed unsolicited at login from <c>ComplementarInformationPacketProcessor</c>
    /// AND emitted by <c>CashShopBalanceRequestPacketProcessor</c> in response to the
    /// client's on-open / post-purchase refresh.
    /// </para>
    /// </summary>
    public class CashShopCoinsPacket : PacketWriter
    {
        private const int PacketNumber = 3404;

        public CashShopCoinsPacket(int premium, int silk)
        {
            Type(PacketNumber);
            WriteInt(0);                // m_nResult — SUCCESS
            WriteInt(silk);             // m_nBonusCash
            WriteInt(premium);          // m_nHaveCash
        }
    }
}