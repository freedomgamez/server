using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Monster.bin</c> §3 (<c>CsMonsterSkill::sINFO</c>) instead of the DB.
    /// Field map (DTO ← bin):
    /// <code>
    ///   SkillId        ← s_nSkill_IDX
    ///   Type           ← s_dwMonsterID            (denormalised join key)
    ///   MinValue       ← s_dwEff_Val_Min          (effect-value min — damage / heal / etc.)
    ///   MaxValue       ← s_dwEff_Val_Max          (effect-value max)
    ///   CastingTime    ← s_nCastTime              (ms)
    ///   Cooldown       ← s_dwCoolTime             (ms)
    ///   TargetCount    ← s_nTarget_Cnt            (default target slots)
    ///   TargetMin      ← s_nTarget_MinCnt
    ///   TargetMax      ← s_nTarget_MaxCnt
    ///   UseTerms       ← s_nUse_Terms             (CsMonsterSkill::eTERM_TYPE)
    ///   RangeId        ← s_nRangeIdx              (FK → CsMonsterSkillTerms.s_nIDX)
    ///   AnimationDelay ← s_nAni_Delay             (ms, client-only — kept for parity)
    ///   ActiveType     ← s_nActiveType            (0=self,1=target,2=coord — AoE origin)
    ///   SkillType      ← s_nSkillType             (CsMonsterSkill::eEFFECT_TYPE)
    ///   NoticeTime     ← s_fNoticeTime            (ms pre-cast telegraph)
    /// </code>
    /// Replaces <c>_repository.GetMonsterSkillInfoAssetsAsync()</c>.
    /// </summary>
    public class MonsterSkillInfoAssetsQueryHandler : IRequestHandler<MonsterSkillInfoAssetsQuery, List<MonsterSkillInfoAssetDTO>>
    {
        private readonly MonsterBinLoader _loader;

        public MonsterSkillInfoAssetsQueryHandler(MonsterBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<MonsterSkillInfoAssetDTO>> Handle(MonsterSkillInfoAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<MonsterSkillInfoAssetDTO>(_loader.Data.SkillsByIndex.Count);
            long syntheticId = 0;
            foreach (var skill in _loader.Data.SkillsByIndex.Values)
            {
                list.Add(new MonsterSkillInfoAssetDTO
                {
                    Id = ++syntheticId,
                    SkillId = skill.SkillIndex,
                    Type = skill.MonsterType,
                    MinValue = (int)skill.EffectValueMin,
                    MaxValue = (int)skill.EffectValueMax,
                    CastingTime = skill.CastTime,
                    Cooldown = (int)skill.Cooldown,
                    TargetCount = (byte)System.Math.Min(skill.TargetCount, byte.MaxValue),
                    TargetMin = (byte)System.Math.Min(skill.TargetMin, byte.MaxValue),
                    TargetMax = (byte)System.Math.Min(skill.TargetMax, byte.MaxValue),
                    UseTerms = (byte)System.Math.Min(skill.UseTerms, byte.MaxValue),
                    RangeId = skill.RangeIndex,
                    AnimationDelay = skill.AnimationDelay,
                    ActiveType = (byte)System.Math.Min(skill.ActiveType, (uint)byte.MaxValue),
                    SkillType = skill.SkillType,
                    NoticeTime = skill.NoticeTime,
                    // Bin-only factor trio — used by GROWTH/BERSERK stat scaling and
                    // SingleStack debuff dispatch.  Length 3; 0 entries are no-ops.
                    EffectFactor = skill.EffectFactor,
                    EffectFactorValue = skill.EffectFactorValue,
                });
            }
            return Task.FromResult(list);
        }
    }
}
