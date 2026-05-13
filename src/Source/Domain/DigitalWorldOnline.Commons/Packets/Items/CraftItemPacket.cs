using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.Items
{
    public class CraftItemPacket : PacketWriter
    {
        private const int PacketNumber = 3982;

        public CraftItemPacket(
            int resultCode,
            ItemCraftAssetModel craftRecipe,
            int totalMakedCount,
            int tradeLimitTime,
            long moneyResult,
            int successCount,
            int itemCount,
            IReadOnlyList<(int ItemId, int Amount)> deletedItems,
            int usedProtectItemId)
        {
            Type(PacketNumber);
            WriteInt(resultCode);
            WriteInt(craftRecipe.ItemId);
            WriteInt(itemCount);
            WriteInt(tradeLimitTime);
            WriteInt64(moneyResult);
            WriteInt(totalMakedCount);
            WriteInt(successCount);
            WriteInt(deletedItems.Count);

            foreach (var material in deletedItems)
            {
                WriteInt(material.ItemId);
                WriteInt(material.Amount);
            }

            WriteInt(usedProtectItemId);
        }
    }
}
