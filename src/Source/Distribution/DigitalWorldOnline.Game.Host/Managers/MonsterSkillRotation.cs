using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Digimon;

namespace DigitalWorldOnline.Game.Managers
{
    /// <summary>
    /// Filters a monster skill against its <c>CsMonsterSkill::s_nUse_Terms</c>
    /// (<c>eTERM_TYPE</c>) trigger condition before the rotation picks it.
    ///
    /// Term values mirror the client enum in <c>Monster.h:103-124</c> (we mirror the
    /// numeric values here as bare constants — enums live in BinTool only):
    /// <code>
    ///   0  TERM_TYPE_NONE       always eligible
    ///   1  TARGET_HP_PER_DOWN   target.HP%  &lt;= MinValue
    ///   2  TARGET_HP_PER_UP     target.HP%  &gt;= MinValue
    ///   3  TARGET_HP_VAL_DOWN   target.HP   &lt;= MinValue
    ///   4  TARGET_HP_VAL_UP     target.HP   &gt;= MinValue
    ///   5  OWN_HP_PER_DOWN      mob.HP%     &lt;= MinValue
    ///   6  OWN_HP_PER_UP        mob.HP%     &gt;= MinValue
    ///   7  OWN_HP_VAL_DOWN      mob.HP      &lt;= MinValue
    ///   8  OWN_HP_VAL_UP        mob.HP      &gt;= MinValue
    ///   9..16  same pattern for DS
    /// </code>
    /// Threshold value lives in <c>s_dwEff_Val_Min</c> on the same row
    /// (mapped onto <see cref="MonsterSkillInfoAssetModel.MinValue"/>).
    ///
    /// Without this gate the rotation pure-randoms across every skill regardless
    /// of mob/target state — bosses fire phase-2 attacks at full HP, etc.
    /// </summary>
    public static class MonsterSkillRotation
    {
        // ─── eTERM_TYPE values from CsMonsterSkill (Monster.h:103-124) ─────
        private const int TermNone                = 0;
        private const int TermTargetHpPercentDown = 1;
        private const int TermTargetHpPercentUp   = 2;
        private const int TermTargetHpValueDown   = 3;
        private const int TermTargetHpValueUp     = 4;
        private const int TermOwnHpPercentDown    = 5;
        private const int TermOwnHpPercentUp      = 6;
        private const int TermOwnHpValueDown      = 7;
        private const int TermOwnHpValueUp        = 8;
        private const int TermTargetDsPercentDown = 9;
        private const int TermTargetDsPercentUp   = 10;
        private const int TermTargetDsValueDown   = 11;
        private const int TermTargetDsValueUp     = 12;
        private const int TermOwnDsPercentDown    = 13;
        private const int TermOwnDsPercentUp      = 14;
        private const int TermOwnDsValueDown      = 15;
        private const int TermOwnDsValueUp        = 16;

        public static bool TermMatches(MonsterSkillInfoAssetModel skill, MobConfigModel mob, DigimonModel? target)
        {
            int threshold = skill.MinValue;

            switch (skill.UseTerms)
            {
                case TermNone:
                    return true;

                // ─── target HP ──────────────────────────────────────────
                case TermTargetHpPercentDown:
                    return target is not null && target.HP > 0 && TargetHpPercent(target) <= threshold;
                case TermTargetHpPercentUp:
                    return target is not null && target.HP > 0 && TargetHpPercent(target) >= threshold;
                case TermTargetHpValueDown:
                    return target is not null && target.CurrentHp <= threshold;
                case TermTargetHpValueUp:
                    return target is not null && target.CurrentHp >= threshold;

                // ─── own HP ─────────────────────────────────────────────
                case TermOwnHpPercentDown:
                    return mob.HPValue > 0 && (mob.CurrentHP * 100 / mob.HPValue) <= threshold;
                case TermOwnHpPercentUp:
                    return mob.HPValue > 0 && (mob.CurrentHP * 100 / mob.HPValue) >= threshold;
                case TermOwnHpValueDown:
                    return mob.CurrentHP <= threshold;
                case TermOwnHpValueUp:
                    return mob.CurrentHP >= threshold;

                // ─── target DS ──────────────────────────────────────────
                case TermTargetDsPercentDown:
                    return target is not null && target.DS > 0 && TargetDsPercent(target) <= threshold;
                case TermTargetDsPercentUp:
                    return target is not null && target.DS > 0 && TargetDsPercent(target) >= threshold;
                case TermTargetDsValueDown:
                    return target is not null && target.CurrentDs <= threshold;
                case TermTargetDsValueUp:
                    return target is not null && target.CurrentDs >= threshold;

                // ─── own DS ─────────────────────────────────────────────
                // MobConfigModel doesn't expose a DSValue baseline — own-DS terms
                // pass-through until DS state is added.
                case TermOwnDsPercentDown:
                case TermOwnDsPercentUp:
                case TermOwnDsValueDown:
                case TermOwnDsValueUp:
                    return true;

                default:
                    // Unknown term — pass-through rather than block, so a bin field we don't
                    // recognise (future content) doesn't silently disable a mob's rotation.
                    return true;
            }
        }

        private static int TargetHpPercent(DigimonModel target) => target.CurrentHp * 100 / target.HP;
        private static int TargetDsPercent(DigimonModel target) => target.CurrentDs * 100 / target.DS;
    }
}
