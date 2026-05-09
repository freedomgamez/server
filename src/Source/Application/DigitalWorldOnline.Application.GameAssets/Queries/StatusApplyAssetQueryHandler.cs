using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>DMBase.bin</c> section 11 (EvolutionStageApply, 17 entries) instead of the
    /// <c>Asset_StatusApply</c> DB table. Keys: each entry's <c>StageValue</c> matches an
    /// <c>EvolutionRankEnum</c> value (1 = Digi-Tama, 3 = Rookie, 6 = Mega, etc.). The
    /// <c>ApplyValue</c> is a per-stage stat-multiplier percentage (Rookie 100% → Mega 200% →
    /// BurstMode 250%) used by <c>StatusManager</c> when computing partner digimon stats.
    /// </summary>
    public class StatusApplyAssetQueryHandler : IRequestHandler<StatusApplyAssetQuery, List<StatusApplyAssetDTO>>
    {
        private readonly DMBaseBinLoader _dmBase;

        public StatusApplyAssetQueryHandler(DMBaseBinLoader dmBase)
        {
            _dmBase = dmBase;
        }

        public Task<List<StatusApplyAssetDTO>> Handle(StatusApplyAssetQuery request, CancellationToken cancellationToken)
        {
            var src = _dmBase.Data.EvolutionStageApply;
            var list = new List<StatusApplyAssetDTO>(src.Count);
            foreach (var (stageValue, applyValue) in src)
            {
                list.Add(new StatusApplyAssetDTO
                {
                    StageValue = stageValue,
                    ApplyValue = applyValue
                });
            }
            return Task.FromResult(list);
        }
    }
}
