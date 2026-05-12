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

        public MapMobsByIdQueryHandler(
            MapBinLoader mapBin,
            MonsterBinLoader monsterBin)
        {
            _mapBin = mapBin;
            _monsterBin = monsterBin;
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
                    if (!_monsterBin.Data.ByType.TryGetValue(mapMob.MonsterTableId, out var mon))
                        continue;

                    result.Add(new MobConfigDTO
                    {
                        Id = id++,
                        Type = mapMob.MonsterTableId,
                        Model = mon.ModelId,
                        Name = $"Mob {mapMob.MonsterTableId}",
                        Level = (byte)Math.Min(byte.MaxValue, mon.Level),
                        ViewRange = mon.Sight,
                        HuntRange = mon.HuntRange,
                        Class = mon.Class,
                        Coliseum = false,
                        Round = 0,
                        WeekDay = DungeonDayOfWeekEnum.Sunday,
                        ColiseumMobType = ColiseumMobTypeEnum.Normal,
                        ReactionType = DigimonReactionTypeEnum.Passive,
                        Attribute = (DigimonAttributeEnum)0,
                        Element = (DigimonElementEnum)0,
                        Family1 = (DigimonFamilyEnum)0,
                        Family2 = (DigimonFamilyEnum)0,
                        Family3 = (DigimonFamilyEnum)0,
                        RespawnInterval = mapMob.RespawnSeconds,
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
                            Id = id,
                            MobConfigId = id,
                            MapId = (short)request.MapId,
                            X = mapMob.CenterX,
                            Y = mapMob.CenterY
                        },
                        ExpReward = new MobExpRewardConfigDTO
                        {
                            Id = id,
                            MobId = id,
                            TamerExperience = mon.ExpMax,
                            DigimonExperience = mon.ExpMax,
                            NatureExperience = 0,
                            ElementExperience = 0,
                            SkillExperience = 0
                        },
                        DropReward = new MobDropRewardConfigDTO
                        {
                            Id = id,
                            MobId = id,
                            MinAmount = 0,
                            MaxAmount = 0
                        },
                        GameMapConfigId = request.MapId
                    });
                }

                return Task.FromResult(result);
            }

            throw new InvalidOperationException("Map mob catalogs must come from bins (MapMobsByIdQuery).");
        }
    }
}
