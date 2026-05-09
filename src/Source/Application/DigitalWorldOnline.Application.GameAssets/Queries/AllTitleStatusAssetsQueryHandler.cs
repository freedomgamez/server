using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Achieve.bin</c> §B — derives a title row for every achievement that
    /// grants a buff (<c>BuffCode &gt; 0</c>). Returned rows carry only the join keys
    /// (<c>Id == AchievementId == QuestId</c>); the 23 stat fields (HP/AT/CT/DE/DS/EV/HT/MS
    /// + 12 element CDs) inherited from <c>StatusDTO</c> are left at their default zero —
    /// retired by Phase 3 alongside the per-tamer-base-status retirement (see CHANGELOG).
    /// v487 has no per-title flat-stat block: title effect = applied
    /// <c>BuffId</c> &rarr; <c>Buff.bin</c> &rarr; <c>Skill.bin</c> apply rows. The retired
    /// stat-bonus path is still callable via <c>StatusManager.GetTitleStatus()</c> if the
    /// commented-out scaffolding is uncommented; see C4b commit for the full retirement.
    /// </summary>
    public class AllTitleStatusAssetsQueryHandler : IRequestHandler<AllTitleStatusAssetsQuery, List<TitleStatusAssetDTO>>
    {
        private readonly AchieveBinLoader _loader;

        public AllTitleStatusAssetsQueryHandler(AchieveBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<TitleStatusAssetDTO>> Handle(AllTitleStatusAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<TitleStatusAssetDTO>();
            foreach (var rec in _loader.Data.All)
            {
                if (rec.BuffCode <= 0) continue;
                list.Add(new TitleStatusAssetDTO
                {
                    Id = rec.QuestId,
                    AchievementId = rec.QuestId
                    // ItemId, Name, and 23 stat fields default to zero/null.
                });
            }
            return Task.FromResult(list);
        }
    }
}
