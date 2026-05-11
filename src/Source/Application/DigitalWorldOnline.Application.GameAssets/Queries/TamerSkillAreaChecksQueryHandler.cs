using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Skill.bin §3 CsAreaCheck</c> (93 rows in v487).  Each row pairs a
    /// tamer-skill index with up to 30 MapIds; zero entries are unused slots and dropped at
    /// load.  The corresponding <c>TamerSkillAssetModel.UseAreaCheck</c> +
    /// <c>Available</c> flags govern interpretation server-side.
    /// </summary>
    public sealed class TamerSkillAreaChecksQueryHandler : IRequestHandler<TamerSkillAreaChecksQuery, List<TamerSkillAreaCheckAssetDTO>>
    {
        private readonly SkillBinLoader _loader;

        public TamerSkillAreaChecksQueryHandler(SkillBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<TamerSkillAreaCheckAssetDTO>> Handle(TamerSkillAreaChecksQuery request, CancellationToken cancellationToken)
        {
            var list = new List<TamerSkillAreaCheckAssetDTO>(_loader.Data.AreaChecksByIndex.Count);
            foreach (var rec in _loader.Data.AreaChecksByIndex.Values)
            {
                var nonzero = rec.Areas.Where(a => a != 0).Select(a => (int)a).ToArray();
                list.Add(new TamerSkillAreaCheckAssetDTO
                {
                    Index = rec.Index,
                    MapIds = nonzero
                });
            }
            return Task.FromResult(list);
        }
    }
}
