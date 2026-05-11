namespace DigitalWorldOnline.Commons.DTOs.Assets
{
    /// <summary>
    /// One row of <c>CsMonsterSkill::sINFO</c> (Monster.bin §3) — see
    /// <c>Application.GameAssets/Bins/MonsterSkillRecord</c> for the canonical source.
    /// Field shapes preserved for back-compat with consumers that already use this DTO
    /// via <see cref="Models.Asset.MonsterSkillInfoAssetModel"/>.
    /// </summary>
    public sealed class MonsterSkillInfoAssetDTO
    {
        /// <summary>Synthetic sequential identifier (assigned at query time).</summary>
        public long Id { get; set; }

        /// <summary>Bin's <c>s_nSkill_IDX</c> — primary key into the per-mob skill list.</summary>
        public int SkillId { get; set; }

        /// <summary>Bin's <c>s_dwEff_Val_Min</c> — effect-value floor (damage / heal / buff strength).</summary>
        public int MinValue { get; set; }

        /// <summary>Bin's <c>s_dwEff_Val_Max</c> — effect-value ceiling.</summary>
        public int MaxValue { get; set; }

        /// <summary>Bin's <c>s_nCastTime</c> — pre-damage cast window (ms).</summary>
        public int CastingTime { get; set; }

        /// <summary>Bin's <c>s_dwCoolTime</c> — per-skill cooldown (ms).</summary>
        public int Cooldown { get; set; }

        /// <summary>Bin's <c>s_nTarget_Cnt</c> — default target slot count.</summary>
        public byte TargetCount { get; set; }

        /// <summary>Bin's <c>s_nTarget_MinCnt</c> — minimum target slots.</summary>
        public byte TargetMin { get; set; }

        /// <summary>Bin's <c>s_nTarget_MaxCnt</c> — maximum target slots.</summary>
        public byte TargetMax { get; set; }

        /// <summary>
        /// Bin's <c>s_nUse_Terms</c> — <c>CsMonsterSkill::eTERM_TYPE</c> rotation trigger
        /// (0 = no gate, 1..16 = HP/DS percentage/value above/below conditions).
        /// </summary>
        public byte UseTerms { get; set; }

        /// <summary>Bin's <c>s_nRangeIdx</c> — FK into <c>CsMonsterSkillTerms.s_nIDX</c> (AoE shape).</summary>
        public int RangeId { get; set; }

        /// <summary>Bin's <c>s_nAni_Delay</c> — client animation delay (ms). Server keeps for parity but doesn't act on it.</summary>
        public float AnimationDelay { get; set; }

        /// <summary>Bin's <c>s_nActiveType</c> — AoE origin: 0=self, 1=target, 2=coord.</summary>
        public byte ActiveType { get; set; }

        /// <summary>Bin's <c>s_nSkillType</c> — <c>CsMonsterSkill::eEFFECT_TYPE</c> (what the skill does).</summary>
        public int SkillType { get; set; }

        /// <summary>Bin's <c>s_fNoticeTime</c> — pre-cast telegraph duration (ms).</summary>
        public float NoticeTime { get; set; }

        /// <summary>Bin's <c>s_dwMonsterID</c> — denormalised so consumers can filter by mob without joining.</summary>
        public int Type { get; set; }
    }
}
