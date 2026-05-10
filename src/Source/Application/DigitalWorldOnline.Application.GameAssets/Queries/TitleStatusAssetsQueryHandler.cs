using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Achieve.bin</c> — single-row sibling of <c>AllTitleStatusAssetsQuery</c>.
    /// Returns the bin's achievement record for the requested <c>TitleId</c>, projected to
    /// the same shape <c>AllTitleStatusAssetsQueryHandler</c> emits (the 23 inherited
    /// <c>StatusDTO</c> stat fields stay zero — v487 has no per-title flat-stat block, the
    /// title effect is the buff at <c>BuffCode → Buff.bin → Skill.bin</c>).
    /// </summary>
    public class TitleStatusAssetsQueryHandler : IRequestHandler<TitleStatusAssetsQuery, TitleStatusAssetDTO?>
    {
        private readonly AchieveBinLoader _loader;

        public TitleStatusAssetsQueryHandler(AchieveBinLoader loader)
        {
            _loader = loader;
        }

        public Task<TitleStatusAssetDTO?> Handle(TitleStatusAssetsQuery request, CancellationToken cancellationToken)
        {
            TitleStatusAssetDTO? result = null;
            foreach (var rec in _loader.Data.All)
            {
                if (rec.BuffCode <= 0) continue;
                if (rec.QuestId != request.TitleId) continue;

                result = new TitleStatusAssetDTO
                {
                    Id = rec.QuestId,
                    AchievementId = rec.QuestId
                    // ItemId, Name, 23 stat fields default to zero/null.
                };
                break;
            }
            return Task.FromResult(result);
        }
    }
}
