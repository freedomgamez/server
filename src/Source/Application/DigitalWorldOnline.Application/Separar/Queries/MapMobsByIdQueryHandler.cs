using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Queries
{
    public class MapMobsByIdQueryHandler : IRequestHandler<MapMobsByIdQuery, List<MobConfigDTO>>
    {
        private readonly MapBinLoader _mapBin;
        private readonly MonsterBinLoader _monsterBin;
        private readonly DigimonListBinLoader _digimonListBin;

        public MapMobsByIdQueryHandler(
            MapBinLoader mapBin,
            MonsterBinLoader monsterBin,
            DigimonListBinLoader digimonListBin)
        {
            _mapBin = mapBin;
            _monsterBin = monsterBin;
            _digimonListBin = digimonListBin;
        }

        public Task<List<MobConfigDTO>> Handle(MapMobsByIdQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded && _monsterBin.IsLoaded)
            {
                if (!_mapBin.Data.MonstersByMapId.TryGetValue(request.MapId, out var mapMobs) || mapMobs.Count == 0)
                    return Task.FromResult(new List<MobConfigDTO>());

                var result = new List<MobConfigDTO>(mapMobs.Count);
                long id = 1;

                foreach (var mapMob in mapMobs)
                {
                    foreach (var spawn in MapMonsterSpawnMaterializer.Expand(mapMob))
                    {
                        if (!_monsterBin.Data.ByType.TryGetValue(spawn.MonsterTableId, out var mon))
                            continue;

                        long mobId = id++;
                        var digimon = _digimonListBin.Data.FindByType(spawn.MonsterTableId);
                        var attribute = digimon != null && Enum.IsDefined(typeof(DigimonAttributeEnum), digimon.Attribute)
                            ? (DigimonAttributeEnum)digimon.Attribute
                            : DigimonAttributeEnum.None;
                        var element = digimon != null && Enum.IsDefined(typeof(DigimonElementEnum), digimon.Element)
                            ? (DigimonElementEnum)digimon.Element
                            : DigimonElementEnum.Neutral;
                        var family1 = digimon != null && Enum.IsDefined(typeof(DigimonFamilyEnum), digimon.Family1)
                            ? (DigimonFamilyEnum)digimon.Family1
                            : DigimonFamilyEnum.None;
                        var family2 = digimon != null && Enum.IsDefined(typeof(DigimonFamilyEnum), digimon.Family2)
                            ? (DigimonFamilyEnum)digimon.Family2
                            : DigimonFamilyEnum.None;
                        var family3 = digimon != null && Enum.IsDefined(typeof(DigimonFamilyEnum), digimon.Family3)
                            ? (DigimonFamilyEnum)digimon.Family3
                            : DigimonFamilyEnum.None;

                        result.Add(new MobConfigDTO
                        {
                            Id = mobId,
                            Type = spawn.MonsterTableId,
                            Model = mon.ModelId,
                            Name = $"Mob {spawn.MonsterTableId}",
                            Level = (byte)Math.Min(byte.MaxValue, mon.Level),
                            ViewRange = mon.Sight,
                            HuntRange = mon.HuntRange,
                            Class = mon.Class,
                            Coliseum = false,
                            Round = 0,
                            WeekDay = DungeonDayOfWeekEnum.Sunday,
                            ColiseumMobType = ColiseumMobTypeEnum.Normal,
                            ReactionType = DigimonReactionTypeEnum.Passive,
                            Attribute = attribute,
                            Element = element,
                            Family1 = family1,
                            Family2 = family2,
                            Family3 = family3,
                            RespawnInterval = spawn.RespawnSeconds,
                            HPValue = mon.Hp,
                            DSValue = mon.Ds,
                            DEValue = mon.DefPower,
                            EVValue = mon.Evasion,
                            MSValue = mon.MoveSpeed,
                            WSValue = mon.WalkSpeed,
                            CTValue = mon.CriticalRate,
                            ATValue = mon.AttPower,
                            ASValue = mon.AttSpeed,
                            ARValue = mon.AttRange,
                            HTValue = mon.HitRate,
                            BLValue = 0,
                            Location = new MobLocationConfigDTO
                            {
                                Id = mobId,
                                MobConfigId = mobId,
                                MapId = (short)request.MapId,
                                X = spawn.X,
                                Y = spawn.Y
                            },
                            ExpReward = new MobExpRewardConfigDTO
                            {
                                Id = mobId,
                                MobId = mobId,
                                TamerExperience = mon.ExpMax,
                                DigimonExperience = mon.ExpMax,
                                NatureExperience = 0,
                                ElementExperience = 0,
                                SkillExperience = 0
                            },
                            DropReward = new MobDropRewardConfigDTO
                            {
                                Id = mobId,
                                MobId = mobId,
                                MinAmount = 0,
                                MaxAmount = 0
                            },
                            GameMapConfigId = request.MapId
                        });
                    }
                }

                return Task.FromResult(result);
            }

            throw new InvalidOperationException("Map mob catalogs must come from bins (MapMobsByIdQuery).");
        }
    }
}
