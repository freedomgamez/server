namespace DigitalWorldOnline.Commons.DTOs.Digimon
{
    /// <summary>
    /// Cash-shop-acquired memory skill attached to a specific digimon evolution.  Distinct
    /// from <see cref="DigimonEvolutionSkillDTO"/> (which represents fixed slot 0..3
    /// inherent skills from <c>Digimon_List.bin s_Skill[4]</c>): memory skills are
    /// identified by <see cref="SkillId"/> rather than a slot index, are added/removed
    /// dynamically per character, and the bin's <c>s_nMemorySkill</c> flag distinguishes
    /// them.
    /// </summary>
    public class DigimonMemorySkillDTO
    {
        public long Id { get; set; }

        /// <summary>
        /// CsSkill id — references <see cref="DigitalWorldOnline.Commons.Models.Asset.SkillInfoAssetModel.SkillId"/>.
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// Current skill level (memory skills also level up via the same skill-points pool).
        /// </summary>
        public byte CurrentLevel { get; set; }

        /// <summary>
        /// Cap for <see cref="CurrentLevel"/>; bin's <c>s_nMaxLevel</c>.
        /// </summary>
        public byte MaxLevel { get; set; }

        /// <summary>
        /// When the player acquired this memory skill on this digimon evolution.
        /// </summary>
        public DateTime AcquiredAt { get; set; }

        /// <summary>
        /// When the current cooldown ends (UTC).  Cooldown duration sourced from the bin's
        /// <c>s_fCooldownTime</c> on the matching CsSkill row at cast time.
        /// </summary>
        public DateTime CooldownEndsAt { get; set; }

        /// <summary>
        /// FK back to the owning evolution.
        /// </summary>
        public DigimonEvolutionDTO Evolution { get; set; }
        public long EvolutionId { get; set; }
    }
}
