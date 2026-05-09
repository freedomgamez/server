using DigitalWorldOnline.Application.CharacterAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.CharacterAssets.Queries
{
    /// <summary>
    /// Backed by <c>DMBase.bin</c> section 1 instead of the DB. Character.Host only consults this
    /// query at character creation — for which the level is always 1 — so the loader keeps just
    /// level-1 rows in memory (the bin's full 1440 records are filtered down to 12 at parse).
    /// If a higher level is asked for (which shouldn't happen in this host), the handler throws.
    /// Game.Host has its own scaling-aware loader for the full per-level table.
    /// </summary>
    public class TamerLevelStatusQueryHandler : IRequestHandler<TamerLevelStatusQuery, CharacterLevelStatusAssetDTO>
    {
        private readonly DMBaseBinLoader _dmBase;

        public TamerLevelStatusQueryHandler(DMBaseBinLoader dmBase)
        {
            _dmBase = dmBase;
        }

        public Task<CharacterLevelStatusAssetDTO> Handle(TamerLevelStatusQuery request, CancellationToken cancellationToken)
        {
            int tamerModel = (int)request.Type;
            var rec = _dmBase.Data.FindTamerStats(tamerModel, request.Level)
                ?? throw new InvalidOperationException(
                    $"DMBase.bin section 1 has no level-{request.Level} stats for tamer model " +
                    $"{tamerModel}. Character.Host's loader filters to level 1 only — querying " +
                    "any other level here means the caller is in the wrong host.");

            return Task.FromResult(new CharacterLevelStatusAssetDTO
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
    }
}
