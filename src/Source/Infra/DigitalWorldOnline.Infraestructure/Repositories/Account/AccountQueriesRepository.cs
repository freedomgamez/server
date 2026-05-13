using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Extensions;
using DigitalWorldOnline.Commons.DTOs.Account;
using DigitalWorldOnline.Commons.DTOs.Base;
using DigitalWorldOnline.Commons.DTOs.Character;
using DigitalWorldOnline.Commons.Enums;
using Microsoft.EntityFrameworkCore;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using System.Linq;

namespace DigitalWorldOnline.Infraestructure.Repositories.Account
{
    public class AccountQueriesRepository : IAccountQueriesRepository
    {
        private readonly DatabaseContext _context;

        public AccountQueriesRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<AccountDTO?> GetAccountByUsernameAsync(string username)
        {
            return await _context.Account
                .AsNoTracking()
                .Include(x => x.SystemInformation)
                .Include(x => x.AccountBlock)
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<AccountDTO?> GetAccountByIdAsync(long id)
        {
            var dto = await _context.Account
                    .AsNoTracking()
                    .Include(x => x.SystemInformation)
                    .Include(x => x.AccountBlock)
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (dto != null)
            {
                dto.ItemList ??= new List<ItemListDTO>();
                await EnsureAccountListsFromOwnerStorageAsync(id, dto.ItemList);
                await HydrateListsFromOwnerStorageAsync(id, dto.ItemList);
            }

            dto?.ItemList.ForEach(itemList => itemList.Items = itemList.Items.OrderBy(x => x.Slot).ToList());

            return dto;
        }

        private async Task EnsureAccountListsFromOwnerStorageAsync(long accountId, List<ItemListDTO> itemLists)
        {
            var requiredTypes = new[]
            {
                ItemListEnum.AccountWarehouse,
                ItemListEnum.CashWarehouse,
                ItemListEnum.ShopWarehouse,
                ItemListEnum.BuyHistory
            };

            var storageLists = await _context.OwnerItemStorageAccount
                .Where(x => x.AccountId == accountId)
                .ToListAsync();

            var currentTypes = storageLists.Select(x => (ItemListEnum)x.Type).ToHashSet();
            foreach (var type in requiredTypes.Where(type => !currentTypes.Contains(type)))
            {
                _context.OwnerItemStorageAccount.Add(new OwnerItemStorageAccountReadModel
                {
                    AccountId = accountId,
                    Type = (int)type,
                    Size = new ItemListModel(type).Size,
                    Bits = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            storageLists = await _context.OwnerItemStorageAccount
                .AsNoTracking()
                .Where(x => x.AccountId == accountId && requiredTypes.Contains((ItemListEnum)x.Type))
                .OrderBy(x => x.Type)
                .ToListAsync();

            itemLists.Clear();
            itemLists.AddRange(storageLists.Select(storage => new ItemListDTO
            {
                Id = ComposeAccountStorageListId(accountId, (ItemListEnum)storage.Type),
                Type = (ItemListEnum)storage.Type,
                Size = (byte)storage.Size,
                Bits = storage.Bits,
                Items = new List<ItemDTO>()
            }));
        }

        private async Task HydrateListsFromOwnerStorageAsync(long accountId, List<ItemListDTO> itemLists)
        {
            if (itemLists.Count == 0)
                return;

            var listTypes = itemLists.Select(x => (int)x.Type).Distinct().ToList();

            var storageLists = await _context.OwnerItemStorageAccount
                .AsNoTracking()
                .Where(x => x.AccountId == accountId && listTypes.Contains(x.Type))
                .ToDictionaryAsync(x => x.Type);

            if (storageLists.Count == 0)
            {
                foreach (var itemList in itemLists)
                    itemList.Items = BuildEmptySlots(itemList.Id, itemList.Size);
                return;
            }

            var slots = await _context.OwnerItemStorageAccountSlots
                .AsNoTracking()
                .Where(x => x.AccountId == accountId && listTypes.Contains(x.Type))
                .OrderBy(x => x.Slot)
                .ToListAsync();

            var instanceIds = slots
                .Where(x => x.ItemInstanceId.HasValue)
                .Select(x => x.ItemInstanceId!.Value)
                .Distinct()
                .ToList();

            var instances = await _context.OwnerItemStorageInstances
                .AsNoTracking()
                .Where(x => instanceIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var accessoryStatuses = await _context.OwnerItemStorageAccessoryStatuses
                .AsNoTracking()
                .Where(x => instanceIds.Contains(x.ItemInstanceId))
                .ToListAsync();

            var socketStatuses = await _context.OwnerItemStorageSocketStatuses
                .AsNoTracking()
                .Where(x => instanceIds.Contains(x.ItemInstanceId))
                .ToListAsync();

            var accessoryLookup = accessoryStatuses.ToLookup(x => x.ItemInstanceId);
            var socketLookup = socketStatuses.ToLookup(x => x.ItemInstanceId);
            var slotGroups = slots.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var itemList in itemLists)
            {
                if (!storageLists.TryGetValue((int)itemList.Type, out var storageList))
                {
                    itemList.Items = BuildEmptySlots(itemList.Id, itemList.Size);
                    continue;
                }

                itemList.Size = (byte)storageList.Size;
                itemList.Bits = storageList.Bits;
                itemList.Type = (ItemListEnum)storageList.Type;
                itemList.Id = ComposeAccountStorageListId(accountId, itemList.Type);

                if (!slotGroups.TryGetValue((int)itemList.Type, out var listSlots))
                {
                    itemList.Items = BuildEmptySlots(itemList.Id, itemList.Size);
                    continue;
                }

                var slotByIndex = listSlots.ToDictionary(x => x.Slot, x => x);
                var rebuiltItems = new List<ItemDTO>(itemList.Size);
                for (var slotIndex = 0; slotIndex < itemList.Size; slotIndex++)
                {
                    if (!slotByIndex.TryGetValue((short)slotIndex, out var slot))
                    {
                        rebuiltItems.Add(CreateEmptyItem(itemList.Id, slotIndex));
                        continue;
                    }

                    if (!slot.ItemInstanceId.HasValue || !instances.TryGetValue(slot.ItemInstanceId.Value, out var instance))
                    {
                        rebuiltItems.Add(CreateEmptyItem(itemList.Id, slot.Slot));
                        continue;
                    }

                    rebuiltItems.Add(new ItemDTO
                    {
                        Id = instance.Id,
                        ItemListId = itemList.Id,
                        Slot = slot.Slot,
                        ItemId = instance.ItemId,
                        Amount = instance.Amount,
                        Duration = instance.Duration,
                        EndDate = instance.EndDate,
                        FirstExpired = instance.FirstExpired,
                        TamerShopSellPrice = instance.TamerShopSellPrice,
                        FamilyType = instance.FamilyType,
                        Power = instance.Power,
                        RerollLeft = instance.RerollLeft,
                        AccessoryStatus = accessoryLookup[instance.Id]
                            .OrderBy(x => x.Slot)
                            .Select(x => new ItemAccessoryStatusDTO
                            {
                                Id = x.Id,
                                ItemId = instance.Id,
                                Slot = x.Slot,
                                Type = (AccessoryStatusTypeEnum)x.Type,
                                Value = x.Value
                            })
                            .ToList(),
                        SocketStatus = socketLookup[instance.Id]
                            .OrderBy(x => x.Slot)
                            .Select(x => new ItemSocketStatusDTO
                            {
                                Id = x.Id,
                                ItemId = instance.Id,
                                Slot = x.Slot,
                                Type = (AccessoryStatusTypeEnum)x.Type,
                                AttributeId = x.AttributeId,
                                Value = x.Value
                            })
                            .ToList()
                    });
                }

                itemList.Items = rebuiltItems;
            }
        }

        private static ItemDTO CreateEmptyItem(long itemListId, int slot)
        {
            return new ItemDTO
            {
                Id = Guid.NewGuid(),
                ItemListId = itemListId,
                Slot = slot,
                ItemId = 0,
                Amount = 0,
                Duration = 0,
                EndDate = DateTime.MinValue,
                FirstExpired = true,
                TamerShopSellPrice = 0,
                FamilyType = 0,
                Power = 0,
                RerollLeft = 0,
                AccessoryStatus = new List<ItemAccessoryStatusDTO>(),
                SocketStatus = new List<ItemSocketStatusDTO>()
            };
        }

        private static List<ItemDTO> BuildEmptySlots(long itemListId, int size)
        {
            var items = new List<ItemDTO>(size);
            for (var slot = 0; slot < size; slot++)
                items.Add(CreateEmptyItem(itemListId, slot));

            return items;
        }

        private static long ComposeAccountStorageListId(long accountId, ItemListEnum type)
            => checked((accountId * 1000L) + (int)type);

        public async Task<AccountBlockDTO?> GetAccountBlockByIdAsync(long id)
        {
            return await _context.AccountBlock
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<SystemInformationDTO?> GetSystemInformationByIdAsync(long id)
        {
            return await _context.SystemInformation
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IList<AccountDTO>> GetAllAccountsAsync()
        {
            var accs = await _context.Account
                .AsNoTracking()
                .ToListAsync();

            accs.ForEach(acc =>
            {
                acc.Password = acc.Password.Base64Decrypt();
            });

            return accs;
        }

        public async Task<IList<CharacterDTO>> GetConnectedCharactersAsync()
        {
            return await _context.Character
                .AsNoTracking()
                .Where(x => x.State == CharacterStateEnum.Ready)
                .Include(x => x.Location)
                .Include(x => x.Digimons)
                .ToListAsync();
        }
    }
}
