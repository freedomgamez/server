using DigitalWorldOnline.Commons.DTOs.Character;
using DigitalWorldOnline.Commons.DTOs.Base;
using DigitalWorldOnline.Commons.DTOs.Digimon;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace DigitalWorldOnline.Infraestructure.Repositories.Character
{
    public class CharacterQueriesRepository : ICharacterQueriesRepository
    {
        private readonly DatabaseContext _context;

        public CharacterQueriesRepository(DatabaseContext context)
        {
            _context = context;
        }

        //TODO: migrar para o server repository
        public async Task<IDictionary<byte, byte>> GetChannelsByMapIdAsync(short mapId)
        {
            //TODO: remover limite inicial de 5 canais
            var channels = new Dictionary<byte, byte>
            {
                { 0, 0 },
                { 1, 0 },
                { 2, 0 }
            };

            var tamersChannel = await _context.Character
                .AsNoTracking()
                .Where(x => x.Location.MapId == mapId &&
                            x.Channel != byte.MaxValue)
                .Select(x => x.Channel)
                .ToListAsync();

            foreach (var tamerChannel in tamersChannel)
            {
                if (!channels.ContainsKey(tamerChannel))
                    channels.Add(tamerChannel, 1);
                else
                    channels[tamerChannel]++;
            }

            return channels;
        }

        public async Task<CharacterDTO?> GetCharacterAndItemsByIdAsync(long characterId)
        {
            var dto = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (dto != null)
            {
                dto.ItemList ??= new List<ItemListDTO>();
                await EnsureCharacterListsFromOwnerStorageAsync(dto.Id, dto.ItemList);
                await HydrateListsFromOwnerStorageAsync(dto.Id, dto.ItemList);
            }

            dto?.ItemList.ForEach(itemList => itemList.Items = itemList.Items.OrderBy(x => x.Slot).ToList());

            return dto;
        }

        public async Task<CharacterDTO?> GetCharacterByAccountIdAndPositionAsync(long accountId, byte position)
        {
            return await _context.Character
                .AsNoTracking()
                .Include(x => x.Location)
                .Include(x => x.Digimons)
                .FirstOrDefaultAsync(x => x.AccountId == accountId &&
                                            x.Position == position);
        }

        public async Task<CharacterDTO?> GetCharacterByIdAsync(long characterId)
        {
            var character = await _context.Character
                .AsSplitQuery()
                .AsNoTracking()
                .Include(x => x.ActiveEvolution)
                .Include(x => x.Incubator)
                .Include(x => x.Location)
                .Include(x => x.Xai)
                .Include(x => x.TimeReward)
                .Include(x => x.AttendanceReward)
                .Include(x => x.ActiveSkill)
                .Include(x => x.DailyPoints)
                .Include(x => x.ConsignedShop)
                .ThenInclude(y => y.Location)
                .Include(x => x.Progress)
                    .ThenInclude(x => x.InProgressQuestData)
                .Include(y => y.MapRegions)
                .Include(x => x.Points)
                .Include(x => x.BuffList)
                    .ThenInclude(y => y.Buffs)
                .Include(x => x.SealList)
                    .ThenInclude(y => y.Seals)
                .Include(x => x.DigimonArchive)
                    .ThenInclude(y => y.DigimonArchives)
                .Include(x => x.Digimons)
                    .ThenInclude(y => y.Digiclone)
                        .ThenInclude(z => z.History)
                .Include(x => x.Digimons)
                    .ThenInclude(y => y.AttributeExperience)
                .Include(x => x.Digimons)
                    .ThenInclude(y => y.Location)
                .Include(x => x.Digimons)
                    .ThenInclude(y => y.BuffList)
                        .ThenInclude(z => z.Buffs)
                .Include(x => x.Digimons)
                    .ThenInclude(y => y.Evolutions)
                        .ThenInclude(z => z.Skills)
                .Include(x => x.Digimons)
                    .ThenInclude(y => y.Evolutions)
                        .ThenInclude(z => z.MemorySkills)
                .SingleOrDefaultAsync(x => x.Id == characterId);

            if (character != null)
            {
                character.ItemList ??= new List<ItemListDTO>();
                await EnsureCharacterListsFromOwnerStorageAsync(character.Id, character.ItemList);
                await HydrateListsFromOwnerStorageAsync(character.Id, character.ItemList);

                character.ItemList.ForEach(itemList => itemList.Items = itemList.Items.OrderBy(x => x.Slot).ToList());
                character.Digimons = character.Digimons.Where(x => x.Slot <= 5).OrderBy(x => x.Slot).ToList();
            }

            return character;
        }

        public async Task<CharacterDTO?> GetCharacterByNameAsync(string characterName)
        {
            return await _context.Character
                .AsNoTracking().FirstOrDefaultAsync(x => x.Name == characterName);
        }

        public async Task<DigimonDTO?> GetDigimonByIdAsync(long digimonId)
        {
            return await _context.Digimon
                .AsNoTracking()
                .Include(x => x.Digiclone)
                .Include(x => x.AttributeExperience)
                .Include(x => x.Evolutions)
                    .ThenInclude(y => y.Skills)
                .Include(x => x.Evolutions)
                    .ThenInclude(y => y.MemorySkills)
                .Include(x => x.BuffList)
                    .ThenInclude(x => x.Buffs)
                .SingleOrDefaultAsync(x => x.Id == digimonId);
        }

        public async Task<IList<CharacterDTO>> GetCharactersByAccountIdAsync(long accountId)
        {
            //TODO: verificar necessidade de melhoria no tempo de resposta
            var characters = await _context.Character
                .AsSplitQuery()
                .AsNoTracking()
                .Include(x => x.Location)
                .Include(x => x.Xai)
                .Include(x => x.SealList)
                    .ThenInclude(y => y.Seals)
                .Include(x => x.Digimons)
                .Where(x => x.AccountId == accountId)
                .ToListAsync();

            characters.ForEach(character =>
            {
                if (character != null)
                {
                    character.ItemList ??= new List<ItemListDTO>();
                    character.ItemList.ForEach(itemList =>
                    {
                        itemList.Items ??= new List<ItemDTO>();
                        itemList.Items = itemList.Items.OrderBy(x => x.Slot).ToList();
                    });
                    character.Digimons ??= new List<DigimonDTO>();
                    character.Digimons = character.Digimons.Where(x => x.Slot <= 5).OrderBy(x => x.Slot).ToList();
                }
            });

            foreach (var character in characters)
            {
                character.ItemList ??= new List<ItemListDTO>();
                await EnsureCharacterListsFromOwnerStorageAsync(character.Id, character.ItemList);
                await HydrateListsFromOwnerStorageAsync(character.Id, character.ItemList);
                character.ItemList.ForEach(itemList =>
                {
                    itemList.Items ??= new List<ItemDTO>();
                    itemList.Items = itemList.Items.OrderBy(x => x.Slot).ToList();
                });
            }

            return characters;
        }

        public async Task<(string TamerName, string GuildName)> GetCharacterNameAndGuildByIdQAsync(long characterId)
        {
            var dto = await _context.Character
                  .AsNoTracking()
                  .FirstOrDefaultAsync(x => x.Id == characterId);

            var dtoGuild = await _context.Guild
                       .AsNoTracking()
                       .FirstOrDefaultAsync(g => g.Members.Any(m => m.CharacterId == characterId));


            if (dto != null && dtoGuild == null)
            {
                // Supondo que seu objeto Character possui propriedades TamerName e GuildName.
                return (dto.Name, string.Empty);
            }
            if (dto != null && dtoGuild != null)
            {
                return (dto.Name, dtoGuild.Name);
            }

            return (string.Empty, string.Empty);
        }

        private async Task HydrateListsFromOwnerStorageAsync(long characterId, List<ItemListDTO> itemLists)
        {
            if (itemLists.Count == 0)
                return;

            var listTypes = itemLists.Select(x => (int)x.Type).ToList();

            var storageLists = await _context.OwnerItemStorageCharacter
                .AsNoTracking()
                .Where(x => x.CharacterId == characterId && listTypes.Contains(x.Type))
                .ToDictionaryAsync(x => x.Type);

            if (storageLists.Count == 0)
            {
                foreach (var itemList in itemLists)
                    itemList.Items = BuildEmptySlots(itemList.Id, itemList.Size);
                return;
            }

            var slots = await _context.OwnerItemStorageCharacterSlots
                .AsNoTracking()
                .Where(x => x.CharacterId == characterId && listTypes.Contains(x.Type))
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

                itemList.Id = ComposeCharacterStorageListId(characterId, (ItemListEnum)storageList.Type);
                itemList.Size = (byte)storageList.Size;
                itemList.Bits = storageList.Bits;
                itemList.Type = (ItemListEnum)storageList.Type;

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

        private async Task EnsureCharacterListsFromOwnerStorageAsync(long characterId, List<ItemListDTO> itemLists)
        {
            var requiredTypes = new[]
            {
                ItemListEnum.Equipment,
                ItemListEnum.Inventory,
                ItemListEnum.Warehouse,
                ItemListEnum.Chipsets,
                ItemListEnum.JogressChipset,
                ItemListEnum.Digivice,
                ItemListEnum.TamerSkill,
                ItemListEnum.RewardWarehouse,
                ItemListEnum.GiftWarehouse,
                ItemListEnum.ConsignedWarehouse,
                ItemListEnum.TamerShop,
                ItemListEnum.ConsignedShop
            };
            var storageLists = await _context.OwnerItemStorageCharacter
                .Where(x => x.CharacterId == characterId)
                .ToListAsync();

            var currentTypes = storageLists.Select(x => (ItemListEnum)x.Type).ToHashSet();
            foreach (var type in requiredTypes.Where(type => !currentTypes.Contains(type)))
            {
                var existsNow = await _context.OwnerItemStorageCharacter
                    .AsNoTracking()
                    .AnyAsync(x => x.CharacterId == characterId && x.Type == (int)type);
                if (existsNow)
                    continue;

                var entity = new OwnerItemStorageCharacterReadModel
                {
                    CharacterId = characterId,
                    Type = (int)type,
                    Size = new ItemListModel(type).Size,
                    Bits = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.OwnerItemStorageCharacter.Add(entity);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.Entry(entity).State = EntityState.Detached;
                }
            }

            storageLists = await _context.OwnerItemStorageCharacter
                .AsNoTracking()
                .Where(x => x.CharacterId == characterId)
                .OrderBy(x => x.Type)
                .ToListAsync();

            itemLists.Clear();
            itemLists.AddRange(storageLists.Select(storage => new ItemListDTO
            {
                Id = ComposeCharacterStorageListId(characterId, (ItemListEnum)storage.Type),
                Type = (ItemListEnum)storage.Type,
                Size = (byte)storage.Size,
                Bits = storage.Bits,
                Items = new List<ItemDTO>()
            }));
        }

        private static long ComposeCharacterStorageListId(long characterId, ItemListEnum type)
            => checked((characterId * 1000L) + (int)type);
    }
}
