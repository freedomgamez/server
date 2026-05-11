using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Skill.bin §1 CsSkill</c> instead of the DB — same source bin/section as
    /// <c>SkillCodeAssetsQueryHandler</c>, just a different DTO shape used by the
    /// gameplay-numbers consumers (cast time, cooldown, target type, AoE range, etc).
    /// String fields (<c>Name</c>, <c>Description</c>) are skipped per the bin loader's
    /// no-strings convention.
    ///
    /// <para>
    /// Field mapping (CsSkill::sINFO → DTO):
    /// </para>
    /// <code>
    ///   s_dwID                → SkillId, Id
    ///   s_nFamilyType         → FamilyType
    ///   s_nUseDS              → DSUsage
    ///   s_nUseHP              → HPUsage
    ///   s_fCastingTime        → CastingTime
    ///   s_fCooldownTime (ms)  → Cooldown   (kept in ms; consumers divide /1000 where needed)
    ///   s_nMaxLevel           → MaxLevel
    ///   s_nLevelupPoint       → RequiredPoints
    ///   s_nTarget             → Target
    ///   s_nAttSphere          → AreaOfEffect
    ///   s_fAttRange_MinDmg    → AoEMinDamage
    ///   s_fAttRange_MaxDmg    → AoEMaxDamage
    ///   s_fAttRange           → Range
    ///   s_nLimitLevel         → UnlockLevel
    ///   s_nReq_Item           → MemoryChips
    ///   s_Apply[0..2].BuffCode → First/Second/ThirdConditionCode
    ///   s_nSkillType          → Type
    ///   (no source — left 0)  → Value         (DB column was always 0 in v487 dataset)
    /// </code>
    /// </summary>
    public class SkillInfoAssetsQueryHandler : IRequestHandler<SkillInfoAssetsQuery, List<SkillInfoAssetDTO>>
    {
        private readonly SkillBinLoader _loader;

        public SkillInfoAssetsQueryHandler(SkillBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<SkillInfoAssetDTO>> Handle(SkillInfoAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<SkillInfoAssetDTO>(_loader.Data.ById.Count);
            foreach (var rec in _loader.Data.ById.Values)
            {
                list.Add(new SkillInfoAssetDTO
                {
                    Id = rec.Id,
                    SkillId = rec.Id,
                    Name = string.Empty,
                    FamilyType = (byte)rec.FamilyType,
                    DSUsage = rec.UseDS,
                    HPUsage = rec.UseHP,
                    Value = 0,
                    CastingTime = rec.CastingTime,
                    Cooldown = (int)rec.CooldownTimeMs,
                    MaxLevel = (byte)rec.MaxLevel,
                    RequiredPoints = (byte)rec.LevelupPoint,
                    Target = (byte)rec.Target,
                    AreaOfEffect = rec.AttSphere,
                    AoEMinDamage = (int)rec.AttRangeMinDmg,
                    AoEMaxDamage = (int)rec.AttRangeMaxDmg,
                    Range = (int)rec.AttRange,
                    UnlockLevel = (byte)rec.LimitLevel,
                    MemoryChips = (byte)rec.ReqItem,
                    FirstConditionCode = rec.Apply0.BuffCode,
                    SecondConditionCode = rec.Apply1.BuffCode,
                    ThirdConditionCode = rec.Apply2.BuffCode,
                    Type = rec.SkillType,
                    AttType = rec.AttType,
                    MemorySkill = (byte)Math.Min((int)rec.MemorySkill, byte.MaxValue),
                    SkillGroup = rec.SkillGroup,
                    SkillRank = rec.SkillRank,
                    Description = string.Empty
                });
            }
            return Task.FromResult(list);
        }
    }
}
