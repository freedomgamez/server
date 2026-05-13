using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Base;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    internal static class AccessoryParitySnapshot
    {
        private static readonly AccessoryStatusTypeEnum[] TrackedTypes =
        {
            AccessoryStatusTypeEnum.CT,
            AccessoryStatusTypeEnum.EV,
            AccessoryStatusTypeEnum.CD,
            AccessoryStatusTypeEnum.ATT
        };

        public static void LogItemSnapshot(ILogger logger, string source, long tamerId, int slot, ItemModel item)
        {
            if (item == null || item.AccessoryStatus == null)
                return;

            var summary = BuildSummary(item);
            if (summary.Length == 0)
                return;

            logger.Information(
                "AccessoryParity {Source}: tamer={TamerId} slot={Slot} item={ItemId} power={Power} {Summary}",
                source,
                tamerId,
                slot,
                item.ItemId,
                item.Power,
                summary.ToString());
        }

        public static void LogEquippedSnapshot(ILogger logger, string source, long tamerId, IEnumerable<ItemModel> equippedItems)
        {
            if (equippedItems == null)
                return;

            var snapshotItems = equippedItems
                .Where(item => item != null && item.HasAccessoryStatus)
                .ToList();

            if (!snapshotItems.Any())
                return;

            var itemParts = new List<string>();
            foreach (var item in snapshotItems)
            {
                var summary = BuildSummary(item);
                if (summary.Length > 0)
                {
                    itemParts.Add($"slot={item.Slot},item={item.ItemId},p={item.Power} [{summary}]");
                }
            }

            if (!itemParts.Any())
                return;

            logger.Information(
                "AccessoryParity {Source}: tamer={TamerId} equipped={EquippedCount} {Items}",
                source,
                tamerId,
                itemParts.Count,
                string.Join(" | ", itemParts));
        }

        private static StringBuilder BuildSummary(ItemModel item)
        {
            var summary = new StringBuilder();
            var powerMultiplier = (decimal)item.Power / 100m;

            foreach (var type in TrackedTypes)
            {
                var rawTotal = item.AccessoryStatus
                    .Where(status => status.Type == type)
                    .Sum(status => (int)status.Value);

                if (rawTotal <= 0)
                    continue;

                var scaled = (int)Math.Round(rawTotal * powerMultiplier, MidpointRounding.AwayFromZero);

                int normalized;
                decimal displayPercent;
                switch (type)
                {
                    case AccessoryStatusTypeEnum.CT:
                    case AccessoryStatusTypeEnum.EV:
                        normalized = scaled * 100;
                        displayPercent = normalized / 100m;
                        break;
                    case AccessoryStatusTypeEnum.CD:
                        normalized = scaled;
                        displayPercent = normalized / 100m;
                        break;
                    case AccessoryStatusTypeEnum.ATT:
                        normalized = scaled;
                        displayPercent = normalized;
                        break;
                    default:
                        continue;
                }

                if (summary.Length > 0)
                    summary.Append("; ");

                summary.Append(type)
                    .Append(" raw=").Append(rawTotal)
                    .Append(" scaled=").Append(scaled)
                    .Append(" normalized=").Append(normalized)
                    .Append(" display=").Append(displayPercent.ToString("0.00")).Append('%');
            }

            return summary;
        }
    }
}
