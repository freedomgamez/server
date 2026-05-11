using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Skill.bin §2 CsTamerSkill</c> instead of the DB.
    ///
    /// <para>
    /// <b>Important divergence from the previous DB-backed query:</b> the v487 client bin
    /// has 64 tamer-skill rows; the previous owner's DB had a 22-row curated subset where
    /// (a) some <c>SkillCode</c> values were author-remapped to point at different real
    /// v487 client skills (Tier B/D from the audit) and (b) six entries (idx 57, 58, 60,
    /// 61, 62, 94) were post-v487 skills with codes that do not exist in v487 Skill.bin §1
    /// at all (Tier C — added from a newer client).  The bin migration uses the
    /// <i>canonical v487 mapping</i>: idx → code straight from the bin, with no overrides.
    /// </para>
    /// <para>
    /// Consequences:
    /// </para>
    /// <list type="bullet">
    /// <item>The 6 post-v487 skills stop working until they're added back via a future bin-edit tool.</item>
    /// <item>Tier B/D <c>SkillCode</c> values change, so <c>BuffInfo.DigimonSkillCode</c> lookups land on different (canonical) buffs.</item>
    /// <item>The first six rows (idx 1-6) are internal tamer-action placeholders with <c>s_dwSkillCode == 0</c>; the bin emits them but downstream lookups by SkillCode treat 0 as "no buff" and ignore them, matching v487 client behavior.</item>
    /// </list>
    /// <para>
    /// <c>Duration</c> is also missing from any bin (it's a server-tuning column the
    /// previous owner hand-curated).  We default to <c>s_fDamageTime</c> from the matching
    /// <c>CsSkill</c> row — most v487 buff-style tamer skills had Duration matching their
    /// DamageTime in seconds anyway.  Skills with no DamageTime get <c>0</c> (effectively
    /// instant); cases where the previous owner overrode this manually are the casualty.
    /// </para>
    /// </summary>
    public class TamerSkillAssetsQueryHandler : IRequestHandler<TamerSkillAssetsQuery, List<TamerSkillAssetDTO>>
    {
        private readonly SkillBinLoader _loader;

        public TamerSkillAssetsQueryHandler(SkillBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<TamerSkillAssetDTO>> Handle(TamerSkillAssetsQuery request, CancellationToken cancellationToken)
        {
            var skills = _loader.Data.ById;
            var list = new List<TamerSkillAssetDTO>(_loader.Data.TamerSkillsByIndex.Count);

            foreach (var ts in _loader.Data.TamerSkillsByIndex.Values)
            {
                int durationSeconds = 0;
                if (skills.TryGetValue(ts.SkillCode, out var skill))
                    durationSeconds = (int)skill.DamageTime;

                list.Add(new TamerSkillAssetDTO
                {
                    Id = ts.Index,
                    SkillId = ts.Index,
                    SkillCode = ts.SkillCode,
                    Duration = durationSeconds,
                    UseState = (byte)ts.UseState,
                    UseAreaCheck = (byte)ts.UseAreaCheck,
                    Available = (byte)ts.Available
                });
            }
            return Task.FromResult(list);
        }
    }
}
