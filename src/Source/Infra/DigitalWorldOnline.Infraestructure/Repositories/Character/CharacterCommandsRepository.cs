using AutoMapper;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Chat;
using DigitalWorldOnline.Commons.DTOs.Character;
using DigitalWorldOnline.Commons.DTOs.Chat;
using DigitalWorldOnline.Commons.DTOs.Digimon;
using Microsoft.EntityFrameworkCore;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.DTOs.Base;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Events;
using MediatR;
using DigitalWorldOnline.Commons.Models.Mechanics;
using DigitalWorldOnline.Commons.DTOs.Events;
using DigitalWorldOnline.Commons.Model.Character;
using DigitalWorldOnline.Commons.Models.Base;
using Microsoft.Extensions.Logging;
using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;

namespace DigitalWorldOnline.Infraestructure.Repositories.Character
{
    public class CharacterCommandsRepository : ICharacterCommandsRepository
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CharacterCommandsRepository> _logger;

        public CharacterCommandsRepository(
            DatabaseContext context,
            IMapper mapper,
            ILogger<CharacterCommandsRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<long> AddCharacterAsync(CharacterModel character)
        {
            var dto = _mapper.Map<CharacterDTO>(character);
            dto.ItemList = new List<ItemListDTO>();

            _context.Character.Add(dto);

            await _context.SaveChangesAsync();

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

            foreach (var type in requiredTypes)
                await EnsureCharacterItemListAsync(dto.Id, type);

            return dto.Id;
        }

        public async Task EnsureCharacterItemListAsync(long characterId, ItemListEnum type)
        {
            var characterExists = await _context.Character
                .AsNoTracking()
                .AnyAsync(x => x.Id == characterId);
            if (!characterExists)
                return;

            var existing = await _context.OwnerItemStorageCharacter
                .AsNoTracking()
                .AnyAsync(x => x.CharacterId == characterId && x.Type == (int)type);
            if (existing)
                return;

            var storage = new OwnerItemStorageCharacterReadModel
            {
                CharacterId = characterId,
                Type = (int)type,
                Size = new ItemListModel(type).Size,
                Bits = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.OwnerItemStorageCharacter.Add(storage);
            await _context.SaveChangesAsync();
        }

        public async Task<DigimonDTO> AddDigimonAsync(DigimonModel digimon)
        {
            var tamerDto = await _context.Character
                .AsNoTracking()
                .Include(x => x.Digimons)
                .SingleOrDefaultAsync(x => x.Id == digimon.CharacterId);

            var dto = _mapper.Map<DigimonDTO>(digimon);

            if (tamerDto != null)
            {
                tamerDto.Digimons.Add(dto);

                _context.Update(tamerDto);

                _context.SaveChanges();
            }

            return dto;
        }

        public async Task<DeleteCharacterResultEnum> DeleteCharacterByAccountAndPositionAsync(long accountId, byte characterPosition)
        {
            try
            {
                var dto = await _context.Character
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(x => x.Incubator)
                    .Include(x => x.Location)
                    .Include(x => x.Xai)
                    .Include(x => x.TimeReward)
                    .Include(x => x.AttendanceReward)
                    .Include(x => x.ActiveSkill)
                    .Include(x => x.DailyPoints)
                    .Include(x => x.ConsignedShop)
                    .Include(x => x.MapRegions)
                    .Include(x => x.Points)
                    .Include(x => x.BuffList)
                        .ThenInclude(y => y.Buffs)
                    .Include(x => x.SealList)
                        .ThenInclude(y => y.Seals)
                    .Include(x => x.Digimons)
                        .ThenInclude(y => y.Digiclone)
                    .Include(x => x.Digimons)
                        .ThenInclude(y => y.AttributeExperience)
                    .Include(x => x.Digimons)
                        .ThenInclude(y => y.Location)
                    .Include(x => x.Digimons)
                        .ThenInclude(y => y.BuffList)
                            .ThenInclude(z => z.Buffs)
                    .Include(x => x.Digimons)
                        .ThenInclude(z => z.Evolutions)
                    .SingleOrDefaultAsync(x => x.AccountId == accountId &&
                                                x.Position == characterPosition);

                if (dto != null)
                {
                    _context.Remove(dto);
                    _context.SaveChanges();
                }

                return DeleteCharacterResultEnum.Deleted;
            }
            catch
            {
                return DeleteCharacterResultEnum.Error;
            }
        }

        public async Task UpdateCharacterChannelByIdAsync(long characterId, byte channel)
        {
            var character = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (character != null)
            {
                character.Channel = channel;

                _context.Character.Update(character);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterLocationAsync(CharacterLocationModel location)
        {
            var dto = await _context.CharacterLocation
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == location.Id);

            if (dto != null)
            {
                dto.MapId = location.MapId;
                dto.X = location.X;
                dto.Y = location.Y;
                dto.Z = location.Z;

                _context.CharacterLocation.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateDigimonLocationAsync(DigimonLocationModel location)
        {
            var dto = await _context.DigimonLocation
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == location.Id);

            if (dto != null)
            {
                dto.MapId = location.MapId;
                dto.X = location.X;
                dto.Y = location.Y;
                dto.Z = location.Z;

                _context.DigimonLocation.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterResourcesAsync(CharacterModel tamer)
        {
            var tamerDto = await _context.Character
                .AsNoTracking()
                .Include(x => x.Digimons)
                .FirstOrDefaultAsync(x => x.Id == tamer.Id);

            if (tamerDto != null)
            {
                tamerDto.CurrentHp = tamer.CurrentHp;
                tamerDto.CurrentDs = tamer.CurrentDs;
                tamerDto.Digimons = _mapper.Map<List<DigimonDTO>>(tamer.Digimons);

                _context.Update(tamerDto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharactersStateAsync(CharacterStateEnum state)
        {
            var characters = await _context.Character
                .AsNoTracking()
                .ToListAsync();

            characters.ForEach(character =>
            {
                character.State = state;
                character.EventState = CharacterEventStateEnum.None;
            });

            _context.Character.UpdateRange(characters);
            _context.SaveChanges();
        }

        public async Task UpdateCharacterStateByIdAsync(long characterId, CharacterStateEnum state)
        {
            var character = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (character != null)
            {
                character.State = state;

                _context.Character.Update(character);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterExperienceAsync(long tamerId, long currentExperience, byte level)
        {
            var dto = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tamerId);

            if (dto != null)
            {
                dto.CurrentExperience = currentExperience;
                dto.Level = level;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateDigimonExperienceAsync(DigimonModel digimon)
        {
            var dto = await _context.Digimon
                .AsNoTracking()
                .Include(x => x.Evolutions)
                .Include(x => x.AttributeExperience)
                .FirstOrDefaultAsync(x => x.Id == digimon.Id);

            if (dto != null)
            {
                dto.CurrentExperience = digimon.CurrentExperience;
                dto.CurrentSkillExperience = digimon.CurrentSkillExperience;
                dto.TranscendenceExperience = digimon.TranscendenceExperience;
                dto.Level = digimon.Level;

                dto.AttributeExperience.Data = digimon.AttributeExperience.Data;
                dto.AttributeExperience.Vaccine = digimon.AttributeExperience.Vaccine;
                dto.AttributeExperience.Virus = digimon.AttributeExperience.Virus;

                dto.AttributeExperience.Ice = digimon.AttributeExperience.Ice;
                dto.AttributeExperience.Water = digimon.AttributeExperience.Water;
                dto.AttributeExperience.Fire = digimon.AttributeExperience.Fire;
                dto.AttributeExperience.Land = digimon.AttributeExperience.Land;
                dto.AttributeExperience.Wind = digimon.AttributeExperience.Wind;
                dto.AttributeExperience.Wood = digimon.AttributeExperience.Wood;
                dto.AttributeExperience.Light = digimon.AttributeExperience.Light;
                dto.AttributeExperience.Dark = digimon.AttributeExperience.Dark;
                dto.AttributeExperience.Thunder = digimon.AttributeExperience.Thunder;
                dto.AttributeExperience.Steel = digimon.AttributeExperience.Steel;

                foreach (var evolutionDto in dto.Evolutions)
                {
                    var evolutionModel = digimon.Evolutions
                        .FirstOrDefault(x => x.Id == evolutionDto.Id);

                    if (evolutionModel != null)
                    {
                        evolutionDto.Type = evolutionModel.Type;
                        evolutionDto.Unlocked = evolutionModel.Unlocked;
                        evolutionDto.SkillPoints = evolutionModel.SkillPoints;
                        evolutionDto.SkillMastery = evolutionModel.SkillMastery;
                        evolutionDto.SkillExperience = evolutionModel.SkillExperience;
                    }
                }

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterSealsAsync(CharacterSealListModel sealList)
        {
            var dto = await _context.CharacterSealList
                .AsNoTracking()
                .Include(x => x.Seals)
                .FirstOrDefaultAsync(x => x.Id == sealList.Id);

            if (dto != null)
            {
                dto.SealLeaderId = sealList.SealLeaderId;

                foreach (var seal in sealList.Seals)
                {
                    var dtoSeal = dto.Seals.FirstOrDefault(x => x.Id == seal.Id);
                    if (dtoSeal != null)
                    {
                        dtoSeal.SealId = seal.SealId;
                        dtoSeal.SequentialId = seal.SequentialId;
                        dtoSeal.Favorite = seal.Favorite;
                        dtoSeal.Amount = seal.Amount;
                        _context.Update(dtoSeal);
                    }
                    else
                    {
                        dtoSeal = _mapper.Map<CharacterSealDTO>(seal);
                        dtoSeal.SealListId = sealList.Id;
                        dto.Seals.Add(dtoSeal);
                        _context.Add(dtoSeal);
                    }
                }

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task AddChatMessageAsync(ChatMessageModel chatMessage)
        {
            var dto = _mapper.Map<ChatMessageDTO>(chatMessage);
            if (dto != null)
            {
                _context.Add(dto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePartnerCurrentTypeAsync(DigimonModel digimon)
        {
            var dto = await _context.Digimon
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == digimon.Id);

            if (dto != null)
            {
                dto.CurrentType = digimon.CurrentType;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateDigicloneAsync(DigimonDigicloneModel digiclone)
        {
            var dto = await _context.DigimonDigiclone
                .AsNoTracking()
                .Include(x => x.History)
                .FirstOrDefaultAsync(x => x.Id == digiclone.Id);

            if (dto != null)
            {
                dto.ATLevel = digiclone.ATLevel;
                dto.BLLevel = digiclone.BLLevel;
                dto.CTLevel = digiclone.CTLevel;
                dto.EVLevel = digiclone.EVLevel;
                dto.HPLevel = digiclone.HPLevel;

                dto.ATValue = digiclone.ATValue;
                dto.BLValue = digiclone.BLValue;
                dto.CTValue = digiclone.CTValue;
                dto.EVValue = digiclone.EVValue;
                dto.HPValue = digiclone.HPValue;

                dto.History.ATValues = digiclone.History.ATValues;
                dto.History.BLValues = digiclone.History.BLValues;
                dto.History.CTValues = digiclone.History.CTValues;
                dto.History.EVValues = digiclone.History.EVValues;
                dto.History.HPValues = digiclone.History.HPValues;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterTitleByIdAsync(long characterId, short titleId)
        {
            var dto = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (dto != null)
            {
                dto.CurrentTitle = titleId;

                _context.Update(dto);

                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterProgressCompleteAsync(CharacterProgressModel progress)
        {
            var dto = await _context.CharacterProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == progress.Id);

            if (dto != null)
            {

                dto.CompletedData = progress.CompletedData;
                dto.CompletedDataValue = progress.CompletedDataValue;

                _context.Update(dto);

                _context.SaveChanges();
            }
        }



        public async Task UpdateCharacterBuffListAsync(CharacterBuffListModel buffList)
        {
            var dto = await _context.CharacterBuffList
                .AsNoTracking()
                .Include(x => x.Buffs)
                .FirstOrDefaultAsync(x => x.Id == buffList.Id);

            if (dto != null)
            {
                // Remove os buffs em dto que não existem em buffList
                var buffsToRemove = dto.Buffs.Where(dtoBuff => !buffList.Buffs.Any(buff => buff.Id == dtoBuff.Id)).ToList();
                foreach (var buffToRemove in buffsToRemove)
                {
                    dto.Buffs.Remove(buffToRemove);
                    _context.Remove(buffToRemove);
                }

                foreach (var buff in buffList.Buffs.Where(x => !x.Expired))
                {
                    var dtoBuff = dto.Buffs.FirstOrDefault(x => x.Id == buff.Id);
                    if (dtoBuff != null)
                    {
                        dtoBuff.Duration = buff.Duration;
                        dtoBuff.EndDate = buff.EndDate;
                        dtoBuff.SkillId = buff.SkillId;
                        dtoBuff.TypeN = buff.TypeN;
                        _context.Update(dtoBuff);
                    }
                    else
                    {
                        dtoBuff = _mapper.Map<CharacterBuffDTO>(buff);
                        dtoBuff.BuffListId = buffList.Id;
                        dto.Buffs.Add(dtoBuff);
                        _context.Add(dtoBuff);
                    }
                }

                _context.SaveChanges();

            }
        }

        public async Task UpdateDigimonBuffListAsync(DigimonBuffListModel buffList)
        {
            var dto = await _context.DigimonBuffList
                .AsNoTracking()
                .Include(x => x.Buffs)
                .FirstOrDefaultAsync(x => x.Id == buffList.Id);

            if (dto != null)
            {
                // Remove os buffs em dto que não existem em buffList
                var buffsToRemove = dto.Buffs.Where(dtoBuff => !buffList.Buffs.Any(buff => buff.Id == dtoBuff.Id)).ToList();
                foreach (var buffToRemove in buffsToRemove)
                {
                    dto.Buffs.Remove(buffToRemove);
                    _context.Remove(buffToRemove);
                    _context.SaveChanges();
                }

                 
                // Atualiza ou adiciona os buffs em dto com base em buffList
                foreach (var buff in buffList.Buffs)
                {
                    var dtoBuff = dto.Buffs.FirstOrDefault(dtoBuff => dtoBuff.Id == buff.Id);
                    if (dtoBuff != null)
                    {
                        dtoBuff.Duration = buff.Duration;
                        dtoBuff.EndDate = buff.EndDate;
                        dtoBuff.SkillId = buff.SkillId;
                        dtoBuff.TypeN = buff.TypeN;
                        dtoBuff.CoolEndDate = buff.CoolEndDate;
                        dtoBuff.Cooldown = buff.Cooldown;
                        _context.Update(dtoBuff);
                        _context.SaveChanges();
                    }
                    else
                    {
                        dtoBuff = _mapper.Map<DigimonBuffDTO>(buff);
                        dtoBuff.BuffListId = buffList.Id;
                        dto.Buffs.Add(dtoBuff);
                        _context.Add(dtoBuff);
                        _context.SaveChanges();
                    }
                }

             

            }
        }

        public async Task UpdateCharacterActiveEvolutionAsync(CharacterActiveEvolutionModel activeEvolution)
        {
            var dto = await _context.CharacterActiveEvolution
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == activeEvolution.Id);

            if (dto != null)
            {
                dto.XgPerSecond = activeEvolution.XgPerSecond;
                dto.DsPerSecond = activeEvolution.DsPerSecond;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterBasicInfoAsync(CharacterModel character)
        {
            var dto = await _context.Character
                .AsNoTracking()
                .Include(x => x.Digimons)
                .SingleOrDefaultAsync(x => x.Id == character.Id);

            if (dto != null)
            {
                dto.CurrentHp = character.CurrentHp;
                dto.CurrentDs = character.CurrentDs;
                dto.XGauge = character.XGauge;
                dto.XCrystals = character.XCrystals;

                foreach (var digimonDto in dto.Digimons)
                {
                    var digimonModel = character.Digimons
                        .FirstOrDefault(x => x.Id == digimonDto.Id);

                    if (digimonModel != null)
                    {
                        digimonDto.CurrentHp = digimonModel.CurrentHp;
                        digimonDto.CurrentDs = digimonModel.CurrentDs;
                        digimonDto.CurrentType = digimonModel.CurrentType;
                    }
                }

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateItemListBitsAsync(long itemListId, long bits)
        {
            await UpsertOwnerListBitsAsync(itemListId, bits);
        }

        public async Task UpdateItemsAsync(List<ItemModel> items)
        {
            if (!items.Any())
                return;

            var groupedByList = items
                .GroupBy(x => x.ItemListId)
                .ToList();

            if (groupedByList.Count > 1)
            {
                throw new InvalidOperationException(
                    $"UpdateItemsAsync received mixed ItemListId payload: {string.Join(",", groupedByList.Select(x => x.Key))}.");
            }

            foreach (var itemListGroup in groupedByList)
                await UpsertOwnerItemsAsync(itemListGroup.ToList());
        }

        public async Task UpdateItemAccessoryStatusAsync(ItemModel item)
        {
            await UpsertOwnerItemsAsync(new List<ItemModel> { item });
        }

        public async Task UpdateItemSocketStatusAsync(ItemModel item)
        {
            await UpsertOwnerItemsAsync(new List<ItemModel> { item });
        }

        public async Task UpdateItemAsync(ItemModel item)
        {
            await UpsertOwnerItemsAsync(new List<ItemModel> { item });
        }

        public async Task UpdateItemListSizeAsync(long itemListId, byte newSize)
        {
            await UpsertOwnerListSizeAsync(itemListId, newSize);
        }

        private enum OwnerStorageKind
        {
            Character = 0,
            Account = 1
        }

        private static (long OwnerId, int StorageType, OwnerStorageKind OwnerKind) ParseOwnerStorageToken(long itemListId)
        {
            var ownerId = itemListId / 1000L;
            var storageType = (int)(itemListId % 1000L);

            if (ownerId <= 0)
                throw new InvalidOperationException($"Invalid ItemListId {itemListId}: missing owner token.");

            if (!Enum.IsDefined(typeof(ItemListEnum), storageType))
                throw new InvalidOperationException($"Invalid ItemListId {itemListId}: invalid ItemListEnum token {storageType}.");

            var ownerKind = IsAccountStorageType(storageType)
                ? OwnerStorageKind.Account
                : OwnerStorageKind.Character;

            return (ownerId, storageType, ownerKind);
        }

        private static bool IsAccountStorageType(int storageType)
            => storageType is (int)ItemListEnum.ShopWarehouse
                or (int)ItemListEnum.AccountWarehouse
                or (int)ItemListEnum.CashWarehouse
                or (int)ItemListEnum.BuyHistory;

        private async Task UpsertOwnerListBitsAsync(long itemListId, long bits)
        {
            if (itemListId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemListId), itemListId, "ItemListId must be greater than zero.");

            var (ownerId, storageType, ownerKind) = ParseOwnerStorageToken(itemListId);

            var updated = ownerKind == OwnerStorageKind.Account
                ? await _context.OwnerItemStorageAccount
                    .Where(x => x.AccountId == ownerId && x.Type == storageType)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Bits, bits)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow))
                : await _context.OwnerItemStorageCharacter
                    .Where(x => x.CharacterId == ownerId && x.Type == storageType)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Bits, bits)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

            if (updated <= 0)
                throw new InvalidOperationException($"Invalid ItemListId {itemListId}: owner storage list does not exist.");

            await _context.SaveChangesAsync();
        }

        private async Task UpsertOwnerListSizeAsync(long itemListId, byte newSize)
        {
            if (itemListId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemListId), itemListId, "ItemListId must be greater than zero.");

            var (ownerId, storageType, ownerKind) = ParseOwnerStorageToken(itemListId);

            var updated = ownerKind == OwnerStorageKind.Account
                ? await _context.OwnerItemStorageAccount
                    .Where(x => x.AccountId == ownerId && x.Type == storageType)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Size, (short)newSize)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow))
                : await _context.OwnerItemStorageCharacter
                    .Where(x => x.CharacterId == ownerId && x.Type == storageType)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Size, (short)newSize)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

            if (updated <= 0)
                throw new InvalidOperationException($"Invalid ItemListId {itemListId}: owner storage list does not exist.");

            await _context.SaveChangesAsync();
        }

        private async Task UpsertOwnerItemsAsync(List<ItemModel> items)
        {
            if (!items.Any())
                return;

            var itemListId = items.First().ItemListId;
            if (itemListId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemListId), itemListId, "ItemListId must be greater than zero.");

            if (items.Any(x => x.ItemListId != itemListId))
            {
                throw new InvalidOperationException(
                    $"Update payload contains multiple ItemListId values: {string.Join(",", items.Select(x => x.ItemListId).Distinct())}.");
            }

            var (ownerId, storageType, ownerKind) = ParseOwnerStorageToken(itemListId);
            if (ownerKind == OwnerStorageKind.Account)
            {
                await UpsertOwnerItemsForAccountAsync(ownerId, storageType, items, sourceModelList: items.Select(x => x.ItemList).FirstOrDefault(x => x != null));
                return;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sourceModelList = items
                    .Select(x => x.ItemList)
                    .FirstOrDefault(x => x != null);

                if (sourceModelList != null && (int)sourceModelList.Type != storageType)
                    throw new InvalidOperationException(
                        $"ItemList type drift detected for list token {itemListId}: payload type={(int)sourceModelList.Type}.");

                short? inferredSize = sourceModelList?.Size > 0 ? sourceModelList.Size : null;
                long? inferredBits = sourceModelList?.Bits;

                var storage = await _context.OwnerItemStorageCharacter
                    .FirstOrDefaultAsync(x => x.CharacterId == ownerId && x.Type == storageType);
                if (storage == null)
                    throw new InvalidOperationException($"Invalid ItemListId {itemListId}: owner storage list does not exist for character {ownerId}.");

                if (inferredSize.HasValue)
                    storage.Size = inferredSize.Value;
                if (inferredBits.HasValue)
                    storage.Bits = inferredBits.Value;
                storage.UpdatedAt = DateTime.UtcNow;
                _context.OwnerItemStorageCharacter.Update(storage);

                var slots = await _context.OwnerItemStorageCharacterSlots
                    .Where(x => x.CharacterId == ownerId && x.Type == storageType)
                    .ToListAsync();
                var slotsByPosition = slots.ToDictionary(x => (int)x.Slot);

                var instanceIds = slots
                    .Where(x => x.ItemInstanceId.HasValue)
                    .Select(x => x.ItemInstanceId!.Value)
                    .ToHashSet();

                var instances = await _context.OwnerItemStorageInstances
                    .Where(x => instanceIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

                var accessoryStatuses = await _context.OwnerItemStorageAccessoryStatuses
                    .Where(x => instanceIds.Contains(x.ItemInstanceId))
                    .ToListAsync();
                var socketStatuses = await _context.OwnerItemStorageSocketStatuses
                    .Where(x => instanceIds.Contains(x.ItemInstanceId))
                    .ToListAsync();

                var seenInstanceIds = new HashSet<Guid>();
                var accessoryStatusesToInsert = new List<OwnerItemStorageInstanceAccessoryStatusReadModel>();
                var socketStatusesToInsert = new List<OwnerItemStorageInstanceSocketStatusReadModel>();

                foreach (var item in items)
                {
                if (!slotsByPosition.TryGetValue(item.Slot, out var slot))
                {
                        slot = new OwnerItemStorageCharacterSlotReadModel
                        {
                            CharacterId = ownerId,
                            Type = storageType,
                            Slot = (short)item.Slot,
                            UpdatedAt = DateTime.UtcNow
                    };
                    _context.OwnerItemStorageCharacterSlots.Add(slot);
                    slotsByPosition[item.Slot] = slot;
                }

                slot.UpdatedAt = DateTime.UtcNow;

                if (item.ItemId <= 0 || item.Amount <= 0)
                {
                    slot.ItemInstanceId = null;
                    continue;
                }

                if (item.Id == Guid.Empty)
                {
                    slot.ItemInstanceId = null;
                    continue;
                }

                slot.ItemInstanceId = item.Id;
                seenInstanceIds.Add(item.Id);

                if (!instances.TryGetValue(item.Id, out var instance))
                {
                    instance = new OwnerItemStorageInstanceReadModel
                    {
                        Id = item.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OwnerItemStorageInstances.Add(instance);
                    instances[item.Id] = instance;
                }

                instance.ItemId = item.ItemId;
                instance.Amount = item.Amount;
                instance.Power = item.Power;
                instance.RerollLeft = item.RerollLeft;
                instance.FamilyType = item.FamilyType;
                instance.Duration = item.Duration;
                instance.EndDate = item.EndDate;
                instance.FirstExpired = item.FirstExpired;
                instance.TamerShopSellPrice = item.TamerShopSellPrice;

                if (accessoryStatuses.Any(x => x.ItemInstanceId == item.Id))
                {
                    await _context.OwnerItemStorageAccessoryStatuses
                        .Where(x => x.ItemInstanceId == item.Id)
                        .ExecuteDeleteAsync();
                    accessoryStatuses.RemoveAll(x => x.ItemInstanceId == item.Id);
                }

                if (socketStatuses.Any(x => x.ItemInstanceId == item.Id))
                {
                    await _context.OwnerItemStorageSocketStatuses
                        .Where(x => x.ItemInstanceId == item.Id)
                        .ExecuteDeleteAsync();
                    socketStatuses.RemoveAll(x => x.ItemInstanceId == item.Id);
                }

                foreach (var status in item.AccessoryStatus)
                {
                    accessoryStatusesToInsert.Add(new OwnerItemStorageInstanceAccessoryStatusReadModel
                    {
                        Id = Guid.NewGuid(),
                        ItemInstanceId = item.Id,
                        Slot = status.Slot,
                        Type = (short)status.Type,
                        Value = status.Value
                    });
                }

                foreach (var status in item.SocketStatus)
                {
                    socketStatusesToInsert.Add(new OwnerItemStorageInstanceSocketStatusReadModel
                    {
                        Id = Guid.NewGuid(),
                        ItemInstanceId = item.Id,
                        Slot = status.Slot,
                        Type = (short)status.Type,
                        AttributeId = status.AttributeId,
                        Value = status.Value
                    });
                }
                }

                var referencedInstanceIds = slotsByPosition.Values
                    .Where(x => x.ItemInstanceId.HasValue)
                    .Select(x => x.ItemInstanceId!.Value)
                    .ToHashSet();

                var removedInstanceIds = instanceIds
                    .Where(x => !referencedInstanceIds.Contains(x))
                    .ToList();

                foreach (var instanceId in removedInstanceIds)
                {
                    await _context.OwnerItemStorageAccessoryStatuses
                        .Where(x => x.ItemInstanceId == instanceId)
                        .ExecuteDeleteAsync();
                    await _context.OwnerItemStorageSocketStatuses
                        .Where(x => x.ItemInstanceId == instanceId)
                        .ExecuteDeleteAsync();
                    await _context.OwnerItemStorageInstances
                        .Where(x => x.Id == instanceId)
                        .ExecuteDeleteAsync();
                }

                await _context.SaveChangesAsync();

                var persistedInstanceIds = await _context.OwnerItemStorageInstances
                    .AsNoTracking()
                    .Where(x => seenInstanceIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();
                var persistedInstanceIdsSet = persistedInstanceIds.ToHashSet();

                if (accessoryStatusesToInsert.Any())
                    _context.OwnerItemStorageAccessoryStatuses.AddRange(
                        accessoryStatusesToInsert.Where(x => persistedInstanceIdsSet.Contains(x.ItemInstanceId)));

                if (socketStatusesToInsert.Any())
                    _context.OwnerItemStorageSocketStatuses.AddRange(
                        socketStatusesToInsert.Where(x => persistedInstanceIdsSet.Contains(x.ItemInstanceId)));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpsertOwnerItemsForAccountAsync(long accountId, int storageType, List<ItemModel> items, ItemListModel? sourceModelList)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (sourceModelList != null && (int)sourceModelList.Type != storageType)
                    throw new InvalidOperationException(
                        $"ItemList type drift detected for account token {accountId * 1000L + storageType}: payload type={(int)sourceModelList.Type}.");

                short? inferredSize = sourceModelList?.Size > 0 ? sourceModelList.Size : null;
                long? inferredBits = sourceModelList?.Bits;

                var storage = await _context.OwnerItemStorageAccount
                    .FirstOrDefaultAsync(x => x.AccountId == accountId && x.Type == storageType);
                if (storage == null)
                    throw new InvalidOperationException($"Invalid ItemListId {accountId * 1000L + storageType}: owner storage list does not exist for account {accountId}.");

                if (inferredSize.HasValue)
                    storage.Size = inferredSize.Value;
                if (inferredBits.HasValue)
                    storage.Bits = inferredBits.Value;
                storage.UpdatedAt = DateTime.UtcNow;
                _context.OwnerItemStorageAccount.Update(storage);

                var slots = await _context.OwnerItemStorageAccountSlots
                    .Where(x => x.AccountId == accountId && x.Type == storageType)
                    .ToListAsync();
                var slotsByPosition = slots.ToDictionary(x => (int)x.Slot);

                var existingInstanceIds = slots
                    .Where(x => x.ItemInstanceId.HasValue)
                    .Select(x => x.ItemInstanceId!.Value)
                    .Distinct()
                    .ToList();

                var instances = await _context.OwnerItemStorageInstances
                    .Where(x => existingInstanceIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);
                var accessoryStatusesToInsert = new List<OwnerItemStorageInstanceAccessoryStatusReadModel>();
                var socketStatusesToInsert = new List<OwnerItemStorageInstanceSocketStatusReadModel>();
                var payloadInstanceIds = new HashSet<Guid>();

                foreach (var item in items)
                {
                    var slotIndex = item.Slot;
                    if (slotIndex < 0)
                        continue;

                    if (!slotsByPosition.TryGetValue(slotIndex, out var slot))
                    {
                        slot = new OwnerItemStorageAccountSlotReadModel
                        {
                            AccountId = accountId,
                            Type = storageType,
                            Slot = (short)slotIndex,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.OwnerItemStorageAccountSlots.Add(slot);
                        slotsByPosition[slotIndex] = slot;
                    }

                    slot.UpdatedAt = DateTime.UtcNow;

                    var isEmpty = item.ItemId <= 0 || item.Amount <= 0;
                    if (isEmpty)
                    {
                        slot.ItemInstanceId = null;
                        continue;
                    }

                    var instanceId = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
                    payloadInstanceIds.Add(instanceId);
                    slot.ItemInstanceId = instanceId;

                    if (!instances.TryGetValue(instanceId, out var instance))
                    {
                        instance = new OwnerItemStorageInstanceReadModel { Id = instanceId };
                        _context.OwnerItemStorageInstances.Add(instance);
                        instances[instanceId] = instance;
                    }

                    instance.ItemId = item.ItemId;
                    instance.Amount = item.Amount;
                    instance.Power = item.Power;
                    instance.RerollLeft = item.RerollLeft;
                    instance.FamilyType = item.FamilyType;
                    instance.Duration = item.Duration;
                    instance.EndDate = item.EndDate;
                    instance.FirstExpired = item.FirstExpired;
                    instance.TamerShopSellPrice = item.TamerShopSellPrice;
                    if (instance.CreatedAt == default)
                        instance.CreatedAt = DateTime.UtcNow;

                    await _context.OwnerItemStorageAccessoryStatuses
                        .Where(x => x.ItemInstanceId == instanceId)
                        .ExecuteDeleteAsync();

                    await _context.OwnerItemStorageSocketStatuses
                        .Where(x => x.ItemInstanceId == instanceId)
                        .ExecuteDeleteAsync();

                    foreach (var accessory in item.AccessoryStatus)
                    {
                        accessoryStatusesToInsert.Add(new OwnerItemStorageInstanceAccessoryStatusReadModel
                        {
                            Id = Guid.NewGuid(),
                            ItemInstanceId = instanceId,
                            Slot = (byte)accessory.Slot,
                            Type = (short)accessory.Type,
                            Value = accessory.Value
                        });
                    }

                    foreach (var socket in item.SocketStatus)
                    {
                        socketStatusesToInsert.Add(new OwnerItemStorageInstanceSocketStatusReadModel
                        {
                            Id = Guid.NewGuid(),
                            ItemInstanceId = instanceId,
                            Slot = (byte)socket.Slot,
                            Type = (short)socket.Type,
                            AttributeId = socket.AttributeId,
                            Value = socket.Value
                        });
                    }
                }

                var orphanedInstanceIds = existingInstanceIds
                    .Except(payloadInstanceIds)
                    .ToList();

                if (orphanedInstanceIds.Count > 0)
                {
                    await _context.OwnerItemStorageAccessoryStatuses
                        .Where(x => orphanedInstanceIds.Contains(x.ItemInstanceId))
                        .ExecuteDeleteAsync();
                    await _context.OwnerItemStorageSocketStatuses
                        .Where(x => orphanedInstanceIds.Contains(x.ItemInstanceId))
                        .ExecuteDeleteAsync();
                    await _context.OwnerItemStorageInstances
                        .Where(x => orphanedInstanceIds.Contains(x.Id))
                        .ExecuteDeleteAsync();
                }

                if (accessoryStatusesToInsert.Count > 0)
                {
                    _context.OwnerItemStorageAccessoryStatuses.AddRange(
                        accessoryStatusesToInsert.Where(x => payloadInstanceIds.Contains(x.ItemInstanceId)));
                }
                if (socketStatusesToInsert.Count > 0)
                {
                    _context.OwnerItemStorageSocketStatuses.AddRange(
                        socketStatusesToInsert.Where(x => payloadInstanceIds.Contains(x.ItemInstanceId)));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddInventorySlotsAsync(List<ItemModel> items)
        {
            if (!items.Any())
                return;

            await UpsertOwnerItemsAsync(items);

            var itemListId = items.First().ItemListId;
            var newSize = (byte)(items.Max(x => x.Slot) + 1);
            await UpsertOwnerListSizeAsync(itemListId, newSize);
        }

        public async Task UpdateCharacterEventStateByIdAsync(long characterId, CharacterEventStateEnum state)
        {
            var dto = await _context.Character.FirstOrDefaultAsync(x => x.Id == characterId);

            if (dto != null)
            {
                dto.EventState = state;

                _context.Update(dto);

                _context.SaveChanges();
            }
        }

        public async Task UpdateEvolutionAsync(DigimonEvolutionModel evolution)
        {
            var dto = await _context.DigimonEvolution
                 .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == evolution.Id);

            if (dto != null)
            {
                dto.Type = evolution.Type;
                dto.Unlocked = evolution.Unlocked;
                dto.SkillPoints = evolution.SkillPoints;
                dto.SkillMastery = evolution.SkillMastery;
                dto.SkillExperience = evolution.SkillExperience;

                dto.Skills = _mapper.Map<List<DigimonEvolutionSkillDTO>>(evolution.Skills);

                _context.Update(dto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> RemoveMemorySkillAsync(long evolutionId, int skillId)
        {
            var row = await _context.DigimonMemorySkill
                .FirstOrDefaultAsync(x => x.EvolutionId == evolutionId && x.SkillId == skillId);
            if (row == null) return false;
            _context.DigimonMemorySkill.Remove(row);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<long> AddMemorySkillAsync(long evolutionId, int skillId, byte maxLevel)
        {
            var existing = await _context.DigimonMemorySkill
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EvolutionId == evolutionId && x.SkillId == skillId);
            if (existing != null) return 0;

            var dto = new DigimonMemorySkillDTO
            {
                EvolutionId = evolutionId,
                SkillId = skillId,
                MaxLevel = maxLevel,
                CurrentLevel = 1,
                AcquiredAt = DateTime.UtcNow
            };
            _context.DigimonMemorySkill.Add(dto);
            await _context.SaveChangesAsync();
            return dto.Id;
        }

        public async Task UpdateMemorySkillCooldownAsync(long evolutionId, int skillId, DateTime cooldownEndsAt)
        {
            var row = await _context.DigimonMemorySkill
                .FirstOrDefaultAsync(x => x.EvolutionId == evolutionId && x.SkillId == skillId);
            if (row == null) return;
            row.CooldownEndsAt = cooldownEndsAt;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateIncubatorAsync(CharacterIncubatorModel incubator)
        {
            var dto = await _context.CharacterIncubator
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == incubator.Id);

            if (dto != null)
            {
                dto.EggId = incubator.EggId;
                dto.HatchLevel = incubator.HatchLevel;
                dto.BackupDiskId = incubator.BackupDiskId;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterMapRegionAsync(CharacterMapRegionModel mapRegion)
        {
            var dto = await _context.CharacterMapRegion
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == mapRegion.Id);

            if (dto != null)
            {
                dto.Unlocked = mapRegion.Unlocked;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateDigimonSizeAsync(long digimonId, short size)
        {
            var dto = await _context.Digimon
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == digimonId);

            if (dto != null)
            {
                dto.Size = size;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterSizeAsync(long characterId, short size)
        {
            var dto = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (dto != null)
            {
                dto.Size = size;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateDigimonGradeAsync(long digimonId, DigimonHatchGradeEnum grade)
        {
            var dto = await _context.Digimon
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == digimonId);

            if (dto != null)
            {
                dto.HatchGrade = grade;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterDigimonsOrderAsync(CharacterModel character)
        {
            foreach (var digimon in character.Digimons)
            {
                var dto = await _context.Digimon
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == digimon.Id);

                if (dto != null)
                {
                    dto.Slot = digimon.Slot;

                    _context.Update(dto);
                }
            }

            _context.SaveChanges();
        }

        public async Task DeleteDigimonAsync(long digimonId)
        {
            var dto = await _context.Digimon
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == digimonId);

            if (dto != null)
            {
                _context.Remove(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterDigimonArchiveItemAsync(CharacterDigimonArchiveItemModel characterDigimonArchiveItem)
        {
            var dto = await _context.CharacterDigimonArchiveItem
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == characterDigimonArchiveItem.Id);

            if (dto != null)
            {
                dto.DigimonId = characterDigimonArchiveItem.DigimonId;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateDigimonSlotAsync(long digimonId, byte digimonSlot)
        {
            var dto = await _context.Digimon
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == digimonId);

            if (dto != null)
            {
                dto.Slot = digimonSlot;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterXaiAsync(CharacterXaiModel xai)
        {
            var dto = await _context.CharacterXai
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == xai.Id);

            if (dto != null)
            {
                dto.ItemId = xai.ItemId;
                dto.XCrystals = xai.XCrystals;
                dto.XGauge = xai.XGauge;

                _context.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task AddDigimonArchiveSlotAsync(Guid archiveId, CharacterDigimonArchiveItemModel archiveItem)
        {
            var archiveDto = await _context.CharacterDigimonArchive
                .AsNoTracking()
                .Include(x => x.DigimonArchives)
                .SingleOrDefaultAsync(x => x.Id == archiveId);

            if (archiveDto != null)
            {
                var dto = _mapper.Map<CharacterDigimonArchiveItemDTO>(archiveItem);
                dto.DigimonArchiveId = archiveId;
                _context.CharacterDigimonArchiveItem.Add(dto);
                archiveDto.Slots++;

                _context.Update(archiveDto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterDigimonSlotsAsync(long characterId, byte slots)
        {
            var characterDto = await _context.Character
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == characterId);

            if (characterDto != null)
            {
                characterDto.DigimonSlots = slots;

                _context.Update(characterDto);
                _context.SaveChanges();
            }
        }
        public async Task<CharacterDTO> ChangeCharacterNameAsync(long characterId, string NewCharacterName)
        {

            var dto = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (dto != null)
            {
                dto.Name = NewCharacterName;

                _context.Update(dto);

                _context.SaveChanges();
            }

            return dto;
        }

        public async Task<CharacterDTO> ChangeTamerModelAsync(long characterId, CharacterModelEnum model)
        {
            var dto = await _context.Character
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId);

            if (dto != null)
            {
                dto.Model = model;

                _context.Update(dto);

                _context.SaveChanges();
            }

            return dto;
        }
        public async Task UpdateTamerSkillCooldownAsync(CharacterTamerSkillModel activeSkill)
        {
            var dto = await _context.ActiveSkills
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == activeSkill.Id);

            if (dto != null)
            {
                dto.SkillId = activeSkill.SkillId;
                dto.Cooldown = activeSkill.Cooldown;
                dto.EndCooldown = activeSkill.EndCooldown;
                dto.Type = activeSkill.Type;
                dto.Duration = activeSkill.Duration;
                dto.EndDate = activeSkill.EndDate;
                _context.Update(dto);

                _context.SaveChanges();
            }

        }



        public async Task AddInventorySlotAsync(ItemModel newSlot)
        {
            await UpsertOwnerItemsAsync(new List<ItemModel> { newSlot });

            var newSize = (byte)(newSlot.Slot + 1);
            await UpsertOwnerListSizeAsync(newSlot.ItemListId, newSize);
        }

        public async Task UpdateCharacterArenaPointsAsync(CharacterArenaPointsModel points)
        {
            var dto = await _context.CharacterPoints
            .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == points.Id);

            if (dto != null)
            {
                dto.CurrentStage = points.CurrentStage;
                dto.Amount = points.Amount;
                dto.ItemId = points.ItemId;

                _context.CharacterPoints.Update(dto);
                _context.SaveChanges();
            }
        }

        public async Task UpdateCharacterInProgressAsync(InProgressQuestModel progress)
        {
            var dto = await _context.InProgressQuest.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == progress.Id);

            if (dto != null)
            {

                dto.FirstCondition = progress.FirstCondition;
                dto.SecondCondition = progress.SecondCondition;
                dto.ThirdCondition = progress.ThirdCondition;
                dto.FourthCondition = progress.FourthCondition;
                dto.FifthCondition = progress.FifthCondition;

                _context.InProgressQuest.Update(dto);
                _context.SaveChanges();
            }


        }

        public async Task AddCharacterProgressAsync(CharacterProgressModel progress)
        {
            var dto = await _context.CharacterProgress
                .Include(x => x.InProgressQuestData)
                .FirstOrDefaultAsync(x => x.Id == progress.Id);

            if (dto != null)
            {
                var questsToAdd = progress.InProgressQuestData
                    .Where(quest => dto.InProgressQuestData.All(q => q.Id != quest.Id))
                    .ToList();

                foreach (var newQuest in questsToAdd)
                {
                    var questDto = _mapper.Map<InProgressQuestDTO>(newQuest);
                    questDto.CharacterProgressId = progress.Id;

                    _context.InProgressQuest.Add(questDto);
                    _context.SaveChanges();
                }

            }
        }


        public async Task UpdateTamerAttendanceRewardAsync(AttendanceRewardModel attendanceRewardModel)
        {
            var dto = await _context.AttendanceReward.
                 AsNoTracking()
                 .FirstOrDefaultAsync(x => x.CharacterId == attendanceRewardModel.CharacterId);

            if (dto != null)
            {
                dto.LastRewardDate = attendanceRewardModel.LastRewardDate;
                dto.TotalDays = attendanceRewardModel.TotalDays;

                _context.AttendanceReward.Update(dto);
                _context.SaveChanges();
            }

            return;
        }

        public async Task UpdateTamerTimeRewardAsync(TimeReward timeReward)
        {
            var dto = await _context.TimeReward
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CharacterId == timeReward.CharacterId);

            if (dto == null) return;

            dto.RewardIndex = timeReward.RewardIndex;
            dto.RemainingSeconds = timeReward.RemainingSeconds;
            dto.StartTime = timeReward.StartTime;  // legacy column; updated for parity, not consumed

            _context.TimeReward.Update(dto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCharacterArenaDailyPointsAsync(CharacterArenaDailyPointsModel points)
        {
            var dto = await _context.CharacterDailyPoints
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == points.Id);

            if(dto != null)
            {
                dto.InsertDate = points.InsertDate;
                dto.Points = points.Points;
                _context.CharacterDailyPoints.Update(dto);
                _context.SaveChanges();
            }
        }
    }
}
