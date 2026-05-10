using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pCashShop::MultiBuy (3413) reply. Wire layout from
    /// <c>GS2C_RECV_CASHSHOP_MULTIBUY</c> + <c>cCliGameShop.cpp:RecvBuyCashItem</c>:
    /// <c>u2 nResult · n4 nRealCash · n4 nBonusCash · n1 nSuccessItemCnt ·
    /// nSuccessItemCnt × n4 nSuccessCashItemNo · n1 nFailedItemCnt ·
    /// nFailedItemCnt × n4 nFailedCashItemNo</c>.
    /// <para>
    /// <c>nResult == 0</c> = success; anything else is a billing-error code (the client
    /// passes it to <c>cPrintMsg::PrintMsg</c>). On error the rest of the packet body is
    /// still parsed by the client so always emit valid trailing fields — the success/fail
    /// item lists carry the partial-success picture (e.g. catalog mismatch on one of N
    /// items in a bundle still grants the others).
    /// </para>
    /// </summary>
    public class CashShopMultiBuyResponsePacket : PacketWriter
    {
        private const int PacketNumber = 3413;

        public const ushort ResultSuccess = 0;
        public const ushort ResultInsufficientCash = 1;
        public const ushort ResultUnknownProduct = 2;
        public const ushort ResultPriceMismatch = 3;
        public const ushort ResultInactiveProduct = 4;
        public const ushort ResultWarehouseFull = 5;
        public const ushort ResultInternalError = 99;

        public CashShopMultiBuyResponsePacket(
            ushort result,
            int realCashAfter,
            int bonusCashAfter,
            IReadOnlyList<int> successProductIds,
            IReadOnlyList<int> failedProductIds)
        {
            Type(PacketNumber);
            WriteShort((short)result);
            WriteInt(realCashAfter);
            WriteInt(bonusCashAfter);

            byte sCnt = (byte)Math.Min(successProductIds.Count, byte.MaxValue);
            WriteByte(sCnt);
            for (int i = 0; i < sCnt; i++)
                WriteInt(successProductIds[i]);

            byte fCnt = (byte)Math.Min(failedProductIds.Count, byte.MaxValue);
            WriteByte(fCnt);
            for (int i = 0; i < fCnt; i++)
                WriteInt(failedProductIds[i]);
        }
    }
}
