using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.Items;
using System.Text;

namespace DigitalWorldOnline.Commons.Models.Base
{
    public partial class ItemListModel
    {
        /// <summary>
        /// Returns the current itens in inventory.
        /// </summary>
        // Filter by Amount > 0 too so half-empty rows (ItemId set, Amount cleared by a
        // partial grant or manual DB row) don't appear in the wire count. The v487 client
        // renders one icon per "Count" entry and asserts at cIcon::RenderCount (Icon.cpp:235)
        // when nCount == 0 — popping a CsAssert dialog mid-render. Belt-and-braces with the
        // serialization-side filter in GiftToArray / ToArray.
        public byte Count => (byte)Items.Count(x => x.ItemId != 0 && x.Amount > 0);

        /// <summary>
        /// Return the current free slots amount.
        /// </summary>
        public byte TotalEmptySlots => (byte)Items.Count(x => x.ItemId == 0);

        public int RetrieveEnabled => Count > 0 || Bits > 0 ? 100 : 0;

        /// <summary>
        /// Sort itens by ItemId.
        /// </summary>
        public void Sort()
        {
            var existingItens = Items
                .Where(x => x.ItemId > 0)
                .OrderByDescending(x => x.ItemInfo.Type)
                .ThenByDescending(x => x.ItemId)
                .ThenByDescending(x => x.Amount)
                .ToList();

            var emptyItens = Items
                .Where(x => x.ItemId == 0)
                .ToList();

            existingItens.AddRange(emptyItens);

            var slot = 0;

            foreach (var existingItem in existingItens)
            {
                existingItem.Slot = slot;
                slot++;
            }

            Items = existingItens;
        }

      
        /// <summary>
        /// Increase the current inventory size/slots.
        /// </summary>
        /// <param name="amount">Slots to add.</param>
        public byte AddSlots(byte amount = 1)
        {
            for (byte i = 0; i < amount; i++)
            {
                var newItemSlot = new ItemModel(Items.Max(x => x.Slot))
                {
                    ItemListId = Id
                };

                Items.Add(newItemSlot);

                Size++;
            }

            return Size;
        }

        /// <summary>
        /// Increase the current inventory size.
        /// </summary>
        public ItemModel AddSlot()
        {
            var newItemSlot = new ItemModel(Items.Max(x => x.Slot))
            {
                ItemListId = Id
                //ItemList = this
            };

            Items.Add(newItemSlot);

            Size++;

            return newItemSlot;
        }

        public bool RemoveOrReduceItemsBySection(int itemSection, int totalAmount)
        {
            var backup = BackupOperation();

            var targetAmount = totalAmount;
            var targetItems = FindItemsBySection(itemSection);
            targetItems = targetItems.OrderBy(x => x.Slot).ToList();
            foreach (var targetItem in targetItems)
            {
                if (targetItem.Amount >= targetAmount)
                {
                    targetItem.ReduceAmount(targetAmount);
                    targetAmount = 0;
                }
                else
                {
                    targetAmount -= targetItem.Amount;
                    targetItem.SetAmount();
                }

                if (targetAmount == 0)
                    break;
            }

            if (targetAmount > 0)
            {
                RevertOperation(backup);
                return false;
            }

            return true;
        }

        public bool RemoveOrReduceItemsByItemId(int itemId, int totalAmount)
        {
            var backup = BackupOperation();

            var targetAmount = totalAmount;
            var targetItems = FindItemsById(itemId);
            targetItems = targetItems.OrderBy(x => x.Slot).ToList();
            foreach (var targetItem in targetItems)
            {
                if (targetItem.Amount >= targetAmount)
                {
                    targetItem.ReduceAmount(targetAmount);
                    targetAmount = 0;
                }
                else
                {
                    targetAmount -= targetItem.Amount;
                    targetItem.SetAmount();
                }

                if (targetAmount == 0)
                    break;
            }

            if (targetAmount > 0)
            {
                RevertOperation(backup);
                return false;
            }

            return true;
        }

        public List<ItemModel> FindItemsBySection(int itemSection)
        {
            return Items
                .Where(x => x.Amount > 0 && x.ItemInfo?.Section == itemSection)
                .ToList();
        }

        public ItemModel? FindItemBySection(int itemSection)
        {
            return Items.FirstOrDefault(x => x.Amount > 0 && x.ItemInfo?.Section == itemSection);
        }

        public ItemModel? FindItemById(int itemId, bool allowEmpty = false)
        {
            if (allowEmpty)
                return Items.FirstOrDefault(x => itemId == x.ItemId);
            else
                return Items.FirstOrDefault(x => x.Amount > 0 && itemId == x.ItemId);
        }

        public int FindAvailableSlot(ItemModel targetItem)
        {
            var overlap = targetItem.ItemInfo?.Overlap > 0 ? targetItem.ItemInfo.Overlap : (short)1;
            var slot = Items.FindIndex(x =>
                x.ItemId == targetItem.ItemId &&
                x.Amount + targetItem.Amount <= overlap);

            if (slot < 0)
                slot = GetEmptySlot;

            return slot;
        }

        public List<ItemModel> FindItemsById(int itemId, bool allowEmpty = false)
        {
            if (allowEmpty)
                return Items.Where(x => itemId == x.ItemId).ToList();
            else
                return Items.Where(x => x.Amount > 0 && itemId == x.ItemId).ToList();
        }

        public ItemModel FindItemBySlot(int slot)
        {
            if (slot < 0) return null;

            return Items.FirstOrDefault(x => x.Slot == slot);
        }
        public ItemModel FindItemByTradeSlot(int slot)
        {
            if (slot < 0) return null;

            return Items.FirstOrDefault(x => x.TradeSlot == slot);
        }
        public ItemModel GiftFindItemBySlot(int slot)
        {
            if (slot < 0) return null;


            var ItemInfo = Items.FirstOrDefault(x => x.Slot == slot && x.ItemId > 0);

            if (ItemInfo == null)
                return null;

            return ItemInfo;
        }

        public bool UpdateGiftSlot()
        {
            var ItemInfo = Items.Where( x => x.ItemId > 0).ToList();

            if (ItemInfo.Count <= 0)
                return false;

            var slot = -1;

            foreach (var item in ItemInfo)
            {
                slot++;

                var newItem = new ItemModel();
                newItem.SetItemId(item.ItemId);
                newItem.SetAmount(item.Amount);
                newItem.SetItemInfo(item.ItemInfo);

                RemoveItem(item, (short)item.Slot);             
                AddItem(newItem);
            }

            return true;
        }
        /// <summary>
        /// Returns the first empty slot index or -1.
        /// </summary>
        public int GetEmptySlot => Items.FindIndex(x => x.ItemId == 0);

        public string DebugSummaryForItem(int itemId, int amount)
        {
            var emptySlots = TotalEmptySlots;
            var itemSlots = FindItemsById(itemId, true);
            var stackableSlots = itemSlots
                .Where(x => x.ItemId == itemId && x.ItemInfo != null && x.ItemInfo.Overlap > 1)
                .ToList();
            var stackCapacity = stackableSlots.Sum(x =>
            {
                var free = x.ItemInfo.Overlap - x.Amount;
                return free > 0 ? free : 0;
            });

            return $"Type={Type} ListId={Id} Size={Size} Count={Count} EmptySlots={emptySlots} " +
                   $"ItemId={itemId} ReqAmount={amount} ExistingSlots={itemSlots.Count} " +
                   $"StackableSlots={stackableSlots.Count} StackCapacity={stackCapacity} Bits={Bits}";
        }

        public int InsertItem(ItemModel newItem)
        {
            var targetSlot = GetEmptySlot;
            newItem.Id = Items[targetSlot].Id;
            newItem.Slot = targetSlot;
            newItem.ItemListId = Id;
            newItem.ItemList = this;

            Items[targetSlot] = newItem;

            return targetSlot;
        }

        public bool AddBits(long bits)
        {
            if (Bits + bits > long.MaxValue)
            {
                Bits = long.MaxValue;
                return false;
            }
            else
            {
                Bits += bits;
                return true;
            }
        }

        public bool RemoveBits(long bits)
        {
            if (Bits >= bits)
            {
                Bits -= bits;
                return true;
            }
            else
            {
                Bits = 0;
                return false;
            }
        }

        public bool AddItems(List<ItemModel> itemsToAdd)
        {
            var backup = BackupOperation();

            foreach (var itemToAdd in itemsToAdd)
            {
                if (itemToAdd.Amount == 0 || itemToAdd.ItemId == 0)
                    continue;

                FillExistentSlots(itemToAdd);
                AddNewSlots(itemToAdd);

                if (itemToAdd.Amount > 0)
                {
                    RevertOperation(backup);
                    return false;
                }
            }

            CheckEmptyItems();
            return true;
        }

        //TODO: Retornar objeto contendo slots afetados e resultado final
        public bool AddItem(ItemModel newItem)
        {
            if (newItem.Amount == 0 || newItem.ItemId == 0)
                return false;

            var backup = BackupOperation();

            var itemToAdd = (ItemModel)newItem.Clone();

            FillExistentSlots(itemToAdd);
            AddNewSlots(itemToAdd);

            if (itemToAdd.Amount > 0)
            {
                RevertOperation(backup);
                return false;
            }

            CheckEmptyItems();

            newItem.Slot = itemToAdd.Slot;

            return true;
        }

        /// <summary>
        /// Drops a single gift entry into the next empty slot, preserving the input
        /// <paramref name="newItem"/>'s full <c>Amount</c> in one slot regardless of the
        /// item's stack <c>Overlap</c>. Gift box semantics: each gift is one slot showing
        /// "ItemId × N" until the player claims it; only at claim-time do inventory stack
        /// rules apply. Using <see cref="AddItem"/> here would split a "9202 × 2" gift into
        /// two slots of 1 if the item's normal Overlap is 1.
        /// <para>
        /// Returns false if the gift box is full (no empty slot) or if the input is
        /// half-state (<c>ItemId == 0</c> or <c>Amount == 0</c>).
        /// </para>
        /// </summary>
        public bool AddGiftItem(ItemModel newItem)
        {
            if (newItem.Amount == 0 || newItem.ItemId == 0) return false;
            var emptySlotIdx = GetEmptySlot;
            if (emptySlotIdx < 0) return false;
            return AddItemWithSlot(newItem, emptySlotIdx);
        }

        public bool AddItemWithSlot(ItemModel itemToAdd, int slot)
        {
            if (itemToAdd.Amount == 0 || itemToAdd.ItemId == 0)
                return false;

            var targetSlot = FindItemBySlot(slot);
            if (targetSlot == null)
                return false;

            targetSlot.ItemId = itemToAdd.ItemId;
            targetSlot.Amount = itemToAdd.Amount;
            targetSlot.Power = itemToAdd.Power;
            targetSlot.RerollLeft = itemToAdd.RerollLeft;
            targetSlot.FamilyType = itemToAdd.FamilyType;
            targetSlot.Duration = itemToAdd.Duration;
            targetSlot.EndDate = itemToAdd.EndDate;
            targetSlot.FirstExpired = itemToAdd.FirstExpired;
            targetSlot.ItemInfo = itemToAdd.ItemInfo;
            targetSlot.ItemListId = Id;
            targetSlot.ItemList = this;
            targetSlot.AccessoryStatus = itemToAdd.AccessoryStatus
                .Select(status => new ItemAccessoryStatusModel(status.Slot) { Type = status.Type, Value = status.Value })
                .ToList();
            targetSlot.SocketStatus = itemToAdd.SocketStatus
                .Select(status => new ItemSocketStatusModel(status.Slot) { Type = status.Type, AttributeId = status.AttributeId, Value = status.Value })
                .ToList();

            return true;
        }

        public bool SplitItem(ItemModel itemToAdd, int targetSlot)
        {
            //TODO: Backup?

            if (itemToAdd == null || itemToAdd.Amount == 0 || itemToAdd.ItemId == 0)
                return false;

            FillExistentSlot(itemToAdd, targetSlot);

            CheckEmptyItems();

            return true;
        }

        private List<ItemModel> BackupOperation()
        {
            return Items.Select(item => (ItemModel)item.Clone()).ToList();
        }

        private void RevertOperation(List<ItemModel> backup)
        {
            Items.Clear();
            Items.AddRange(backup);
            CheckEmptyItems();
        }

        private void AddNewSlots(ItemModel itemToAdd)
        {
            var overlap = itemToAdd.ItemInfo?.Overlap > 0 ? itemToAdd.ItemInfo.Overlap : (short)1;
            while (itemToAdd.Amount > 0)
            {
                itemToAdd.Slot = GetEmptySlot;
                if (itemToAdd.Slot < 0)
                    break;

                var newItem = (ItemModel)itemToAdd.Clone();

                if (itemToAdd.Amount > overlap)
                {
                    itemToAdd.ReduceAmount(overlap);
                    newItem.SetAmount(overlap);
                }
                else
                {
                    newItem.SetAmount(itemToAdd.Amount);
                    itemToAdd.SetAmount();
                }

                InsertItem(newItem);
            }
        }

        internal void CheckExpiredItems()
        {
            
        }

        private void FillExistentSlots(ItemModel itemToAdd)
        {
            var targetItems = FindItemsById(itemToAdd.ItemId);

            foreach (var targetItem in targetItems.Where(x => x.ItemInfo.Overlap > 1))
            {
                if (targetItem.Amount + itemToAdd.Amount > itemToAdd.ItemInfo.Overlap)
                {
                    itemToAdd.ReduceAmount(itemToAdd.ItemInfo.Overlap - targetItem.Amount);
                    targetItem.SetAmount(itemToAdd.ItemInfo.Overlap);
                }
                else
                {
                    targetItem.IncreaseAmount(itemToAdd.Amount);
                    itemToAdd.SetAmount();
                }

                itemToAdd.Slot = targetItem.Slot;
            }
        }

        private void FillExistentSlot(ItemModel itemToAdd, int targetSlot)
        {
            var targetItem = FindItemBySlot(targetSlot);

            if (targetItem.ItemId == itemToAdd.ItemId || targetItem.ItemId == 0)
            {
                if (targetItem.Amount + itemToAdd.Amount > itemToAdd.ItemInfo.Overlap)
                {
                    itemToAdd.IncreaseAmount(itemToAdd.ItemInfo.Overlap - targetItem.Amount);
                    targetItem.SetAmount(targetItem.ItemInfo.Overlap);
                }
                else
                {
                    targetItem.IncreaseAmount(itemToAdd.Amount);
                    itemToAdd.SetAmount();
                }

                targetItem.SetItemId(itemToAdd.ItemId);
                targetItem.SetRemainingTime((uint)itemToAdd.ItemInfo.UsageTimeMinutes);
            }
        }

        public bool MoveItem(short originSlot, short destinationSlot)
        {
            return TryMoveWithinList(originSlot, destinationSlot);
        }

        public bool TryMoveWithinList(int originSlot, int destinationSlot)
        {
            var originItem = FindItemBySlot(originSlot);
            var destinationItem = FindItemBySlot(destinationSlot);

            if (originItem == null || destinationItem == null || originItem.ItemId == 0 || originItem.Amount <= 0)
                return false;

            if (originSlot == destinationSlot)
                return true;

            if (destinationItem.ItemId == originItem.ItemId && destinationItem.ItemId > 0)
            {
                if (!TryMergeIntoDestination(originItem, destinationItem, originItem.Amount))
                    return false;

                CheckEmptyItems();
                return true;
            }

            SwapSlotPayload(originItem, destinationItem);
            CheckEmptyItems();
            return true;
        }

        public bool TryMoveAcrossLists(ItemListModel destinationList, int originSlot, int destinationSlot)
        {
            var originItem = FindItemBySlot(originSlot);
            var destinationItem = destinationList.FindItemBySlot(destinationSlot);

            if (originItem == null || destinationItem == null || originItem.ItemId == 0 || originItem.Amount <= 0)
                return false;

            if (destinationItem.ItemId == originItem.ItemId && destinationItem.ItemId > 0)
            {
                if (!TryMergeIntoDestination(originItem, destinationItem, originItem.Amount))
                    return false;

                CheckEmptyItems();
                destinationList.CheckEmptyItems();
                return true;
            }

            SwapSlotPayload(originItem, destinationItem);
            CheckEmptyItems();
            destinationList.CheckEmptyItems();
            return true;
        }

        public bool TrySplitAcrossLists(ItemListModel destinationList, int originSlot, int destinationSlot, int splitAmount)
        {
            if (splitAmount <= 0)
                return false;

            var originItem = FindItemBySlot(originSlot);
            var destinationItem = destinationList.FindItemBySlot(destinationSlot);

            if (originItem == null || destinationItem == null || originItem.ItemId == 0 || originItem.Amount <= splitAmount)
                return false;

            if (destinationItem.ItemId != 0 && destinationItem.ItemId != originItem.ItemId)
                return false;

            if (destinationItem.ItemId == 0)
            {
                CopySlotPayload(destinationItem, originItem, splitAmount);
                originItem.ReduceAmount(splitAmount);
                CheckEmptyItems();
                destinationList.CheckEmptyItems();
                return true;
            }

            if (!destinationItem.CanIncrease(splitAmount))
                return false;

            destinationItem.IncreaseAmount(splitAmount);
            originItem.ReduceAmount(splitAmount);
            CheckEmptyItems();
            destinationList.CheckEmptyItems();
            return true;
        }

        private static bool TryMergeIntoDestination(ItemModel originItem, ItemModel destinationItem, int amountToMove)
        {
            var maxIncrease = destinationItem.GetMaxIncreaseCapacity();
            if (maxIncrease <= 0)
                return false;

            var movedAmount = amountToMove > maxIncrease ? maxIncrease : amountToMove;
            destinationItem.IncreaseAmount(movedAmount);
            originItem.ReduceAmount(movedAmount);
            return true;
        }

        private static void SwapSlotPayload(ItemModel firstSlot, ItemModel secondSlot)
        {
            var firstSnapshot = CreateSlotSnapshot(firstSlot);
            var secondSnapshot = CreateSlotSnapshot(secondSlot);

            ApplySlotSnapshot(firstSlot, secondSnapshot);
            ApplySlotSnapshot(secondSlot, firstSnapshot);
        }

        private static ItemModel CreateSlotSnapshot(ItemModel source)
        {
            var snapshot = new ItemModel();
            CopySlotPayload(snapshot, source, source.Amount);
            return snapshot;
        }

        private static void ApplySlotSnapshot(ItemModel destination, ItemModel snapshot)
        {
            destination.SetItemId(snapshot.ItemId);
            destination.SetAmount(snapshot.Amount);
            destination.SetPower(snapshot.Power);
            destination.SetReroll(snapshot.RerollLeft);
            destination.SetFamilyType(snapshot.FamilyType);
            destination.Duration = snapshot.Duration;
            destination.EndDate = snapshot.EndDate;
            destination.FirstExpired = snapshot.FirstExpired;
            destination.SetItemInfo(snapshot.ItemInfo);
            destination.AccessoryStatus = snapshot.AccessoryStatus
                .Select(status => new ItemAccessoryStatusModel(status.Slot) { Type = status.Type, Value = status.Value })
                .ToList();
            destination.SocketStatus = snapshot.SocketStatus
                .Select(status => new ItemSocketStatusModel(status.Slot) { Type = status.Type, AttributeId = status.AttributeId, Value = status.Value })
                .ToList();
        }

        private static void CopySlotPayload(ItemModel destination, ItemModel source, int amount)
        {
            destination.SetItemId(source.ItemId);
            destination.SetAmount(amount);
            destination.SetPower(source.Power);
            destination.SetReroll(source.RerollLeft);
            destination.SetFamilyType(source.FamilyType);
            destination.Duration = source.Duration;
            destination.EndDate = source.EndDate;
            destination.FirstExpired = source.FirstExpired;
            destination.SetItemInfo(source.ItemInfo);
            destination.AccessoryStatus = source.AccessoryStatus
                .Select(status => new ItemAccessoryStatusModel(status.Slot) { Type = status.Type, Value = status.Value })
                .ToList();
            destination.SocketStatus = source.SocketStatus
                .Select(status => new ItemSocketStatusModel(status.Slot) { Type = status.Type, AttributeId = status.AttributeId, Value = status.Value })
                .ToList();
        }

        public void Clear()
        {
            foreach (var item in Items)
            {
                item.SetItemId();
                item.SetAmount();
                item.SetRemainingTime();
                item.SetSellPrice(0);
            }
        }

        public bool RemoveOrReduceItems(List<ItemModel> itemsToRemoveOrReduce)
        {
            var backup = BackupOperation();

            //TODO: teste com 2 slots do mesmo itemId
            foreach (var itemToRemove in itemsToRemoveOrReduce)
            {
                if (itemToRemove.Amount == 0 || itemToRemove.ItemId == 0)
                    continue;

                var targetItems = FindItemsById(itemToRemove.ItemId);

                foreach (var targetItem in targetItems)
                {
                    if (targetItem.Amount >= itemToRemove.Amount)
                    {
                        targetItem.ReduceAmount(itemToRemove.Amount);
                        itemToRemove.SetAmount();
                        break;
                    }
                    else
                    {
                        itemToRemove.ReduceAmount(targetItem.Amount);
                        targetItem.SetAmount();
                    }
                }

                if (itemToRemove.Amount > 0)
                {
                    RevertOperation(backup);
                    return false;
                }
            }

            CheckEmptyItems();
            return true;
        }

        public bool RemoveOrReduceItem(ItemModel? itemToRemove, int amount, int slot = -1)
        {
            if (itemToRemove == null || amount == 0) return false;

            var tempItem = (ItemModel?)itemToRemove.Clone();
            tempItem?.SetAmount(amount);

            return slot > -1 ?
                RemoveOrReduceItemWithSlot(tempItem, slot) :
                RemoveOrReduceItemWithoutSlot(tempItem);
        }

        public bool RemoveOrReduceItemWithSlot(ItemModel? itemToRemove, int slot)
        {
            if (itemToRemove == null || itemToRemove.Amount == 0 || itemToRemove.ItemId == 0)
                return false;

            var backup = BackupOperation();

            var targetItem = FindItemBySlot(slot);
            targetItem?.ReduceAmount(itemToRemove.Amount);
            itemToRemove.SetAmount();

            if (itemToRemove.Amount > 0)
            {
                RevertOperation(backup);
                return false;
            }

            CheckEmptyItems();
            return true;
        }

        public bool RemoveOrReduceItemWithoutSlot(ItemModel? itemToRemove)
        {
            if (itemToRemove == null || itemToRemove.Amount == 0 || itemToRemove.ItemId == 0)
                return false;

            var backup = BackupOperation();

            var targetItems = FindItemsById(itemToRemove.ItemId);

            foreach (var targetItem in targetItems)
            {
                if (targetItem.Amount >= itemToRemove.Amount)
                {
                    targetItem.ReduceAmount(itemToRemove.Amount);
                    itemToRemove.SetAmount();
                    break;
                }
                else
                {
                    itemToRemove.ReduceAmount(targetItem.Amount);
                    targetItem.SetAmount();
                }
            }

            if (itemToRemove.Amount > 0)
            {
                RevertOperation(backup);
                return false;
            }

            CheckEmptyItems();
            return true;
        }

        public bool RemoveItem(ItemModel itemToRemove, short slot)
        {
            if (itemToRemove == null || itemToRemove.Amount == 0 || itemToRemove.ItemId == 0)
                return false;

            var backup = BackupOperation();

            var targetItem = FindItemBySlot(slot);

            if (targetItem == null)
                return false;

            if (targetItem.Amount >= itemToRemove.Amount)
            {
                targetItem.ReduceAmount(itemToRemove.Amount);
                itemToRemove.SetAmount();
                CheckEmptyItems();
                return true;
            }
            else
            {
                RevertOperation(backup);
                CheckEmptyItems();
                return false;
            }
        }

        public void CheckEmptyItems()
        {
            for (var slot = 0; slot < Size; slot++)
            {
                if (FindItemBySlot(slot) == null)
                {
                    Items.Add(new ItemModel(slot)
                    {
                        ItemListId = Id
                    });
                }
            }

            Items.ForEach(item =>
            {
                if (item.ItemId == 0 || item.Amount <= 0)
                {
                    item.SetItemId();
                    item.SetAmount();
                    item.SetRemainingTime();
                    item.SetSellPrice(0);
                }
            });
        }

        /// <summary>
        /// Serializes the current instance to byte array.
        /// </summary>
        /// <returns>The byte array result.</returns>
        public byte[] ToArray()
        {
            byte[] buffer;

            using (MemoryStream m = new())
            {
                var sortedItems = Items.OrderBy(x => x.Slot);

                foreach (var item in sortedItems)
                    m.Write(item.ToArray(), 0, 68);  // sizeof(cItemData)=68 with COMPAT_487 (incl. u8 ExtraBytes)

                buffer = m.ToArray();
            }

            return buffer;
        }

        /// <summary>
        /// Serializes the current instance to byte array.
        /// </summary>
        /// <returns>The byte array result.</returns>
        public byte[] GiftToArray()
        {
            byte[] buffer;

            using (MemoryStream m = new())
            {
                // Filter by Amount > 0 too — a row with ItemId>0 but Amount==0 represents
                // a partially-cleared/orphaned slot (typically from an incomplete grant or
                // a stash item that got consumed but never had its row purged). Sending it
                // makes the client's cIcon::RenderCount(Icon.cpp:235) assert.
                var filteredItems = Items.Where(x => x.ItemId > 0 && x.Amount > 0).OrderBy(x => x.Slot);

                if (filteredItems.Any())
                {
                    foreach (var item in filteredItems)
                    {
                        m.Write(item.GiftToArray(), 0, 68);
                    }
                    buffer = m.ToArray();
                }
                else
                {
                    buffer = new byte[0]; // Nenhum item com ItemId > 0 encontrado, retornar um array vazio.
                }
            }

            return buffer;
        }
    

        public string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Inventory{Id}");
            foreach (var item in Items.OrderBy(x => x.Slot))
            {
                sb.AppendLine($"Item[{item.Slot}] - {item.ItemId}");
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }
    }
}
