using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pCashShop::BuyHistory (3412) reply. Wire layout from
    /// <c>GS2C_RECV_CASHITEM_BUYHISTORY</c> + <c>cCliGameShop.cpp:RecvCashshopBuyhistory</c>:
    /// <c>n1 nResult · n2 nCount · nCount × n4 nProductItemID</c>.
    /// <para>
    /// Used by the cash shop catalog UI to dim/disable already-purchased products.
    /// Server projects <c>ItemId</c> from <c>AccountBuyHistory</c>'s <see
    /// cref="Models.Base.ItemListModel"/> rows — the list stores full item metadata
    /// for parity with other warehouse types but the wire only needs product IDs.
    /// </para>
    /// </summary>
    public class CashShopBuyHistoryPacket : PacketWriter
    {
        private const int PacketNumber = 3412;
        private const byte ResultSuccess = 0;

        public CashShopBuyHistoryPacket(IReadOnlyList<int> productIds)
        {
            Type(PacketNumber);
            WriteByte(ResultSuccess);

            // Cap at u2 max — the wire format can't represent more, and the v487
            // CashShop UI doesn't need more than ~255 entries anyway.
            int count = Math.Min(productIds.Count, ushort.MaxValue);
            WriteShort((short)count);
            for (int i = 0; i < count; i++)
                WriteInt(productIds[i]);
        }
    }
}
