using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Utils;
using System.Text;

namespace DigitalWorldOnline.Commons.Models.Base
{
    public partial class ItemModel : ICloneable
    {
        /// <summary>
        /// Sets the current power.
        /// </summary>
        /// <param name="power">New power value</param>
        public void SetPower(byte power) => Power = power;

        /// <summary>
        /// Sets the left reroll amount.
        /// </summary>
        /// <param name="reroll">New reroll amount</param>
        public void SetReroll(byte reroll) => RerollLeft = reroll;

        public void SetFamilyType(byte familyType) => FamilyType = familyType;

        public ItemModel(int itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            if (AccessoryStatus == null || AccessoryStatus.Count != 8)
            {
                AccessoryStatus = new List<ItemAccessoryStatusModel>()
                {
                    new ItemAccessoryStatusModel(0),
                    new ItemAccessoryStatusModel(1),
                    new ItemAccessoryStatusModel(2),
                    new ItemAccessoryStatusModel(3),
                    new ItemAccessoryStatusModel(4),
                    new ItemAccessoryStatusModel(5),
                    new ItemAccessoryStatusModel(6),
                    new ItemAccessoryStatusModel(7)
                };
            }

            if (SocketStatus == null || SocketStatus.Count != 3)
            {
                SocketStatus = new List<ItemSocketStatusModel>()
                {
                    new ItemSocketStatusModel(0),
                    new ItemSocketStatusModel(1),
                    new ItemSocketStatusModel(2)

                };
            }
        }

        public uint RemainingMinutes()
        {
            if (!ItemInfo.TemporaryItem)
                return 0;

            if ((EndDate - DateTime.Now).TotalMinutes <= 0)
            {
                return 0xFFFFFFFF;
            }

            var time = (EndDate - DateTime.Now).TotalMinutes > 0 ? (int)(EndDate - DateTime.Now).TotalMinutes : 0;

           

            return (uint)time;
        }

        /// <summary>
        /// Flags the current item if it has been expired.
        /// </summary>
        public bool Expired
        {
            get
            {
                return ItemInfo != null && ItemInfo.UseTimeType > 0 && RemainingMinutes() == 0xFFFFFFFF && FirstExpired;
            }
        }


        /// <summary>
        /// Flag for acessory status.
        /// </summary>
        public bool HasAccessoryStatus => AccessoryStatus.Any(x => x.Value > 0);

        /// <summary>
        /// Returns the flag with the information about item duration.
        /// </summary>
        public bool IsTemporary => ItemInfo?.UseTimeType > 0;

        /// <summary>
        /// Sets the current remaining time of the target item to the base value, if possible.
        /// </summary>
        public bool SetDefaultRemainingTime()
        {
            if (ItemInfo != null && IsTemporary)
            {
                Duration = ItemInfo.UsageTimeMinutes;
                EndDate = DateTime.Now.AddMinutes(ItemInfo.UsageTimeMinutes);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the current item id.
        /// </summary>
        /// <param name="itemId">The new item id</param>
        public void SetItemId(int itemId = 0) => ItemId = itemId;

        /// <summary>
        /// Updates the remaining time.
        /// </summary>
        /// <param name="remainingTime">The new remaining time</param>
        public void SetRemainingTime(uint remainingTime = 0)
        {
            if (remainingTime == 4294967280)
                remainingTime = 0;

            Duration = (int)remainingTime;
            EndDate = DateTime.Now.AddMinutes(remainingTime);
        }

        /// <summary>
        /// Updates the current amount.
        /// </summary>
        /// <param name="amount">The new amount</param>
        public void SetAmount(int amount = 0) => Amount = amount;

        public void SetSlot(int slot) => Slot = slot;

        public void SetTradeSlot(int slot) => TradeSlot = slot;

        /// <summary>
        /// Reduces the current amount.
        /// </summary>
        /// <param name="amount">The amount to be reduced</param>
        public void ReduceAmount(int amount) => Amount -= amount;

        public bool CanIncrease(int amount)
        {
            if (amount <= 0 || ItemInfo == null || ItemId == 0)
                return false;

            return Amount + amount <= ItemInfo.Overlap;
        }

        public int GetMaxIncreaseCapacity()
        {
            if (ItemInfo == null || ItemId == 0)
                return 0;

            var capacity = ItemInfo.Overlap - Amount;
            return capacity > 0 ? capacity : 0;
        }

        /// <summary>
        /// Updates the sell price at tamer shop.
        /// </summary>
        /// <param name="sellPrice">The new sell price</param>
        public void SetSellPrice(int sellprice) => TamerShopSellPrice = sellprice;

        /// <summary>
        /// Updates the extra information about the item.
        /// </summary>
        /// <param name="info">The extra information</param>
        public void SetItemInfo(ItemAssetModel? info) => ItemInfo = info;

        public void SetFirstExpired(bool firstExpired) => FirstExpired = firstExpired;  

        /// <summary>
        /// Increases the current amount.
        /// </summary>
        /// <param name="amount">The amount to be increased</param>
        public void IncreaseAmount(int amount) => Amount += amount;

        /// <summary>
        /// Serializes the current item into byte array.
        /// </summary>
        /// <returns>The serialization byte array.</returns>
        public byte[] ToArray(bool simplified = false)
        {
            byte[] buffer = Array.Empty<byte>();
            using (MemoryStream m = new())
            {
                // ItemId > 0 && Amount > 0 — half-state rows (ItemId set, Amount cleared)
                // serialize as empty slot per the else branch. The client's cIcon::RenderCount
                // (Icon.cpp:235) asserts when nCount == 0; emitting them as empty avoids the
                // popup. Real items with zero stock should be removed from the list at the
                // service layer (CheckEmptyItems), not represented as a half-state row.
                if (ItemId > 0 && Amount > 0)
                {
                    // v487 client recv (cCliGameReceive.cpp:8505 RecvInvenResult,
                    // and the warehouse + sharestash branches at the same site)
                    // memcpys each entry into `cItemData`, where ItemId + Amount
                    // share ONE u4 as a packed bitfield (`m_nType : 17 | m_nCount : 15`).
                    // Same situation the GiftToArray fix handled. Writing two
                    // separate u4s leaves m_nCount = 0 → the client's icon renderer
                    // falls back to "1" / 0-count assert. Pack here.
                    uint mNAll = ((uint)ItemId & 0x1FFFFu) | (((uint)Amount & 0x7FFFu) << 17);
                    m.Write(BitConverter.GetBytes(mNAll), 0, 4); // cItemData::m_nAll

                    if (simplified)
                    {
                        m.Write(new byte[64]); // remaining bytes in sizeof(cItemData)=68
                    }
                    else
                    {
                        // cItemData layout (pack(4)):
                        // u1 m_nRate, u1 m_nLevel, u1 m_nLimited, pad1,
                        // u2[3] m_nSockItemType, u1[3] m_nSockAppRate, pad1,
                        // u2[8] m_nAccOption, u2[8] m_nAccValues,
                        // u4 m_nEndTime, u4 m_nRemainTradeLimitTime, u8 ExtraBytes
                        m.WriteByte(Power);
                        m.WriteByte(RerollLeft);
                        m.WriteByte((byte)(ItemInfo?.BoundType ?? 0));
                        m.WriteByte(0); // alignment padding before u2 array

                        foreach (var socketStatus in SocketStatus.OrderBy(x => x.Slot))
                        {
                            m.Write(BitConverter.GetBytes(socketStatus.AttributeId), 0, 2);
                        }


                        foreach (var socketStatus in SocketStatus.OrderBy(x => x.Slot))
                        {
                            m.Write(BitConverter.GetBytes(socketStatus.Value), 0, 1);
                        }

                        m.WriteByte(0); // alignment padding before accessory u2 array

                        foreach (var accessoryStatus in AccessoryStatus.OrderBy(x => x.Slot))
                        {
                            m.Write(BitConverter.GetBytes(accessoryStatus.Type.GetHashCode()), 0, 2);
                        }

                        foreach (var accessoryStatus in AccessoryStatus.OrderBy(x => x.Slot))
                        {
                            m.Write(BitConverter.GetBytes(accessoryStatus.Value), 0, 2);
                        }

                        m.Write(BitConverter.GetBytes((ushort)0), 0, 2); // alignment padding before u4

                        if (RemainingMinutes() == 0xFFFFFFFF)
                        {
                            m.Write(BitConverter.GetBytes(0xFFFFFFFF), 0, 4);
                        }
                        else
                        {
                            var ts = UtilitiesFunctions.RemainingTimeMinutes((int)RemainingMinutes());
                            m.Write(BitConverter.GetBytes(ts), 0, 4);
                        }

                        m.Write(BitConverter.GetBytes(0), 0, 4); // m_nRemainTradeLimitTime

                        // ExtraBytes (u8 in COMPAT_487)
                        m.Write(BitConverter.GetBytes(0), 0, 4);
                        m.Write(BitConverter.GetBytes(0), 0, 4);
                    }
                }
                else
                {
                    for (int i = 0; i < GeneralSizeEnum.ItemSizeInBytes.GetHashCode(); i++)
                        m.WriteByte(0);
                }

                buffer = m.ToArray();
            }

            return buffer;
        }
        /// <summary>
        /// Serializes the current item into byte array.
        /// </summary>
        /// <returns>The serialization byte array.</returns>
        public byte[] GiftToArray(bool simplified = false)
        {
            // ItemId <= 0 OR Amount <= 0 — half-state row, skip per the cIcon::RenderCount
            // (Icon.cpp:235 nCount != 0) assert. Belt-and-braces with the upstream filter
            // in ItemListModelBehavior.GiftToArray (also filters by Amount > 0).
            if (ItemId <= 0 || Amount <= 0)
            {
                return Array.Empty<byte>();
            }

            // v487 client packs ItemId + Count into ONE u4 bitfield (cItemData::m_nAll
            // — type:17, count:15, common_vs2019/cItemData.h). Writing them as two
            // separate u4s left m_nCount=0, which is why DailyEvent/Attendance gifts
            // displayed "1" (the icon renderer's 0/1 fallback). Everything *past* the
            // first 8 bytes is unchanged from the legacy layout — the in-game gift
            // expiry, accessory and socket fields were already at their previous
            // offsets and shouldn't be reshuffled in this fix.
            uint mNAll = ((uint)ItemId & 0x1FFFFu) | (((uint)Amount & 0x7FFFu) << 17);

            using (MemoryStream m = new())
            {
                m.Write(BitConverter.GetBytes(mNAll), 0, 4);    // cItemData::m_nAll

                if (simplified)
                {
                    m.Write(new byte[64]); // remaining bytes in sizeof(cItemData)=68
                }
                else
                {
                    m.WriteByte(Power);
                    m.WriteByte(RerollLeft);
                    m.WriteByte((byte)(ItemInfo?.BoundType ?? 0));
                    m.WriteByte(0); // alignment padding before u2 array

                    m.Write(BitConverter.GetBytes((ushort)0), 0, 2);
                    m.Write(BitConverter.GetBytes((ushort)0), 0, 2);
                    m.Write(BitConverter.GetBytes((ushort)0), 0, 2);

                    m.WriteByte(0);
                    m.WriteByte(0);
                    m.WriteByte(0);
                    m.WriteByte(0); // alignment padding before accessory u2 array

                    var orderedAccessoryStatus = AccessoryStatus.OrderBy(x => x.Slot);

                    foreach (var accessoryStatus in orderedAccessoryStatus)
                    {
                        m.Write(BitConverter.GetBytes(accessoryStatus.Type.GetHashCode()), 0, 2);
                    }

                    foreach (var accessoryStatus in orderedAccessoryStatus)
                    {
                        m.Write(BitConverter.GetBytes(accessoryStatus.Value), 0, 2);
                    }

                    m.Write(BitConverter.GetBytes((ushort)0), 0, 2); // alignment padding before u4

                    // Gift box expiration: derive directly from EndDate so non-temporary
                    // items (most gifts — XP boosts and time-limited items are the
                    // exception) still expose a "claim by" countdown. RemainingMinutes()
                    // gates by ItemInfo.TemporaryItem and would return 0 for everything
                    // else, making the gift UI show "expires now" on every item.
                    uint giftRemainingMin = 0;
                    if (EndDate > DateTime.UtcNow)
                    {
                        giftRemainingMin = (uint)Math.Min(uint.MaxValue, (EndDate - DateTime.UtcNow).TotalMinutes);
                    }
                    else if (EndDate != default)
                    {
                        // EndDate set but in the past → expired sentinel that the client
                        // recognizes (matches the legacy 0xFFFFFFFF-1 behavior).
                        giftRemainingMin = 0xFFFFFFFF;
                    }
                    m.Write(BitConverter.GetBytes(giftRemainingMin), 0, 4);
                    m.Write(BitConverter.GetBytes(0), 0, 4); // m_nRemainTradeLimitTime
                    m.Write(BitConverter.GetBytes(0), 0, 4); // ExtraBytes low
                    m.Write(BitConverter.GetBytes(0), 0, 4); // ExtraBytes high
                }

                return m.ToArray();
            }
        }

        public string ToString()
        {
            var sb = new StringBuilder();

            if (ItemId > 0)
            {
                sb.AppendLine($"Amount {Amount.ToString()}");
                sb.AppendLine($"Power {Power.ToString()}");
                sb.AppendLine($"RerollLeft {RerollLeft.ToString()}");
                sb.AppendLine($"BoundType {ItemInfo?.BoundType.ToString()}");

                foreach (var accessoryStatus in AccessoryStatus.OrderBy(x => x.Slot))
                {
                    sb.AppendLine($"AccessoryStatus{accessoryStatus.Slot}");
                    sb.AppendLine($"Type {accessoryStatus.Type.ToString()}");
                    sb.AppendLine($"Value {accessoryStatus.Value.ToString()}");
                }

                //sb.AppendLine($"RemainingTime {RemainingTime.ToString()}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the current amount of the target status type.
        /// </summary>
        /// <param name="type">Target status type</param>
        public byte StatusAmount(AccessoryStatusTypeEnum type) => (byte)AccessoryStatus.Count(x => x.Type == type);

        /// <summary>
        /// Clon's an item properties, but keeps the same identifier.
        /// </summary>
        /// <param name="id">The identifier that will be kept.</param>
        /// <returns>The cloned item with the source identifier.</returns>
        public object Clone(Guid id)
        {
            var clonedObject = (ItemModel)Clone();
            clonedObject.Id = id;

            return clonedObject;
        }

        /// <summary>
        /// Clon's an item properties.
        /// </summary>
        /// <returns>The cloned item.</returns>
        public object Clone()
        {
            return (ItemModel)MemberwiseClone();
        }
    }
}
