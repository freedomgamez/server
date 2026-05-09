using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>DMBase.bin</c> section 1 (1440 rows = 12 tamer models × 120 levels) instead
    /// of the DB. The bin's <c>s_dwID</c> encodes <c>(model − 80000) × 1000 + level</c>; we
    /// project it back into the DTO's <c>Type</c> + <c>Level</c>.
    /// </summary>
    public class TamerLevelingAssetsQueryHandler : IRequestHandler<TamerLevelingAssetsQuery, List<CharacterLevelStatusAssetDTO>>
    {
        private readonly DMBaseBinLoader _dmBase;

        public TamerLevelingAssetsQueryHandler(DMBaseBinLoader dmBase)
        {
            _dmBase = dmBase;
        }

        public Task<List<CharacterLevelStatusAssetDTO>> Handle(TamerLevelingAssetsQuery request, CancellationToken cancellationToken)
        {
            var rows = _dmBase.Data.TamerStats;
            var list = new List<CharacterLevelStatusAssetDTO>(rows.Count);
            foreach (var rec in rows.Values)
            {
                int tamerModel = (rec.Id / 1000) + 80000;
                list.Add(new CharacterLevelStatusAssetDTO
                {
                    Type = (CharacterModelEnum)tamerModel,
                    Level = (byte)rec.Level,
                    ExpValue = rec.Exp,
                    HPValue = rec.HP,
                    DSValue = rec.DS,
                    MSValue = rec.MoveSpeed,
                    DEValue = rec.Defence,
                    EVValue = rec.Evasion,
                    CTValue = rec.Critical,
                    ATValue = rec.Attack,
                    HTValue = rec.HitRate
                });
            }
            return Task.FromResult(list);
        }
    }
}
