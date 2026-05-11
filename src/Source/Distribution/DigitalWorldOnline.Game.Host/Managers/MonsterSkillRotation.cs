using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Digimon;

namespace DigitalWorldOnline.Game.Managers
{
    /// <summary>
    /// Filters a monster skill against its <c>CsMonsterSkill::s_nUse_Terms</c>
    /// (<c>eTERM_TYPE</c>) trigger condition before the rotation picks it.
    ///
    /// Term values mirror the client enum in <c>Monster.h:103-124</c>:
    /// <code>
    ///   0  TERM_TYPE_NONE       always eligible
    ///   1  TARGET_HP_PER_DOWN   target.HP%  ≤ Val_Min
    ///   2  TARGET_HP_PER_UP     target.HP%  ≥ Val_Min
    ///   3  TARGET_HP_VAL_DOWN   target.HP   ≤ Val_Min
    ///   4  TARGET_HP_VAL_UP     target.HP   ≥ Val_Min
    ///   5  OWN_HP_PER_DOWN      mob.HP%     ≤ Val_Min
    ///   6  OWN_HP_PER_UP        mob.HP%     ≥ Val_Min
    ///   7  OWN_HP_VAL_DOWN      mob.HP      ≤ Val_Min
    ///   8  OWN_HP_VAL_UP        mob.HP      ≥ Val_Min
    ///   9..16  same pattern for DS
    /// </code>
    /// The threshold value lives in <c>s_dwEff_Val_Min</c> on the same row
    /// (mapped onto <see cref="MonsterSkillInfoAssetModel.MinValue"/>).
    ///
    /// Without this gate the rotation pure-randoms across every skill regardless
    /// of mob/target state — bosses fire their phase-2 attacks at full HP, etc.
    /// </summary>
    public static class MonsterSkillRotation
    {
        public static bool TermMatches(MonsterSkillInfoAssetModel skill, MobConfigModel mob, DigimonModel? target)
        {
            int threshold = skill.MinValue;

            switch ((MonsterSkillTermType)skill.UseTerms)
            {
                case MonsterSkillTermType.None:
                    return true;

                // ─── target HP ──────────────────────────────────────────
                case MonsterSkillTermType.TargetHpPercentDown:
                    return target is not null && target.HP > 0 && TargetHpPercent(target) <= threshold;
                case MonsterSkillTermType.TargetHpPercentUp:
                    return target is not null && target.HP > 0 && TargetHpPercent(target) >= threshold;
                case MonsterSkillTermType.TargetHpValueDown:
                    return target is not null && target.CurrentHp <= threshold;
                case MonsterSkillTermType.TargetHpValueUp:
                    return target is not null && target.CurrentHp >= threshold;

                // ─── own HP ─────────────────────────────────────────────
                case MonsterSkillTermType.OwnHpPercentDown:
                    return mob.HPValue > 0 && (mob.CurrentHP * 100 / mob.HPValue) <= threshold;
                case MonsterSkillTermType.OwnHpPercentUp:
                    return mob.HPValue > 0 && (mob.CurrentHP * 100 / mob.HPValue) >= threshold;
                case MonsterSkillTermType.OwnHpValueDown:
                    return mob.CurrentHP <= threshold;
                case MonsterSkillTermType.OwnHpValueUp:
                    return mob.CurrentHP >= threshold;

                // ─── target DS ──────────────────────────────────────────
                case MonsterSkillTermType.TargetDsPercentDown:
                    return target is not null && target.DS > 0 && TargetDsPercent(target) <= threshold;
                case MonsterSkillTermType.TargetDsPercentUp:
                    return target is not null && target.DS > 0 && TargetDsPercent(target) >= threshold;
                case MonsterSkillTermType.TargetDsValueDown:
                    return target is not null && target.CurrentDs <= threshold;
                case MonsterSkillTermType.TargetDsValueUp:
                    return target is not null && target.CurrentDs >= threshold;

                // ─── own DS ─────────────────────────────────────────────
                // MobConfigModel doesn't expose a DSValue baseline — DS percent uses
                // an absolute-vs-absolute check only.  If/when DS is added to MobConfigModel,
                // swap to the percent formula like HP above.
                case MonsterSkillTermType.OwnDsPercentDown:
                case MonsterSkillTermType.OwnDsPercentUp:
                case MonsterSkillTermType.OwnDsValueDown:
                case MonsterSkillTermType.OwnDsValueUp:
                    return true;     // not gated until mob.DS state is tracked

                default:
                    // Unknown term value — accept rather than block, so a bin field we don't
                    // recognise (future content) doesn't silently disable that mob's rotation.
                    return true;
            }
        }

        private static int TargetHpPercent(DigimonModel target) => target.CurrentHp * 100 / target.HP;
        private static int TargetDsPercent(DigimonModel target) => target.CurrentDs * 100 / target.DS;
    }

    /// <summary>
    /// <c>CsMonsterSkill::eTERM_TYPE</c> mirror.  Stored in
    /// <c>MonsterSkillInfoAssetModel.UseTerms</c> (bin's <c>s_nUse_Terms</c>).
    /// </summary>
    public enum MonsterSkillTermType : byte
    {
        None                 = 0,
        TargetHpPercentDown  = 1,
        TargetHpPercentUp    = 2,
        TargetHpValueDown    = 3,
        TargetHpValueUp      = 4,
        OwnHpPercentDown     = 5,
        OwnHpPercentUp       = 6,
        OwnHpValueDown       = 7,
        OwnHpValueUp         = 8,
        TargetDsPercentDown  = 9,
        TargetDsPercentUp    = 10,
        TargetDsValueDown    = 11,
        TargetDsValueUp      = 12,
        OwnDsPercentDown     = 13,
        OwnDsPercentUp       = 14,
        OwnDsValueDown       = 15,
        OwnDsValueUp         = 16,
    }
}
