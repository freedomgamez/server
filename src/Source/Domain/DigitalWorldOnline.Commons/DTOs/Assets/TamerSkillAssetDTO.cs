namespace DigitalWorldOnline.Commons.DTOs.Assets
{
    public sealed class TamerSkillAssetDTO
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; set; }

 
        public int SkillId { get; set; }

 
        public int SkillCode { get; set; }

        public int Duration { get; set; }

        /// <summary>
        /// CsTamerSkill::sTSkill_INFO::s_nUseState — 0 = always usable, 1 = forbidden during
        /// combat (server checks <c>InBattle</c> on cast).
        /// </summary>
        public byte UseState { get; set; }

        /// <summary>
        /// CsTamerSkill::sTSkill_INFO::s_nUse_Are_Check — 1 = consult the AreaCheck table
        /// (Skill.bin §3) for this skill before allowing cast; 0 = no map gate.
        /// </summary>
        public byte UseAreaCheck { get; set; }

        /// <summary>
        /// CsTamerSkill::sTSkill_INFO::s_nAvailable — interpretation depends on
        /// <see cref="UseAreaCheck"/>: 1 = blacklist (skill forbidden in listed maps),
        /// 2 = whitelist (skill ONLY usable in listed maps).
        /// </summary>
        public byte Available { get; set; }
    }
}