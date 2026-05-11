using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Buff.bin</c> instead of the DB. Each <c>BuffRecord</c> from the bin maps
    /// 1:1 to a <c>BuffAssetDTO</c>; the only field with no bin source is <c>Name</c> (string,
    /// skipped on the server side per <c>reference_bin_string_framing.md</c>) and the legacy
    /// sequential <c>Id</c> primary key (server consumers don't read it). Records flagged
    /// <c>s_bDelete</c> in the bin are dropped at load time.
    /// </summary>
    public class BuffInfoAssetsQueryHandler : IRequestHandler<BuffInfoAssetsQuery, List<BuffAssetDTO>>
    {
        private readonly BuffBinLoader _loader;

        public BuffInfoAssetsQueryHandler(BuffBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<BuffAssetDTO>> Handle(BuffInfoAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<BuffAssetDTO>(_loader.Data.ById.Count);
            foreach (var rec in _loader.Data.ById.Values)
            {
                // Existing semantics: drop deleted records from the asset list.  Memory-
                // skill cast queries the raw bin loader directly so it can still pick
                // up the regionally-disabled buffs (skillCode 9000xxx with s_bDelete=1).
                if (rec.IsDeleted) continue;

                list.Add(new BuffAssetDTO
                {
                    BuffId = rec.Id,
                    DigimonSkillCode = (int)rec.DigimonSkillCode,
                    SkillCode = (int)rec.SkillCode,
                    MinLevel = rec.MinLevel,
                    ConditionLevel = rec.ConditionLevel,
                    Class = (short)rec.BuffClass,
                    Type = rec.BuffType,
                    LifeType = rec.LifeType,
                    TimeType = rec.TimeType
                });
            }
            return Task.FromResult(list);
        }
    }
}
