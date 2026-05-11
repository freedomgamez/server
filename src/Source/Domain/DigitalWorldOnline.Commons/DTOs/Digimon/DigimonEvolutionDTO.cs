namespace DigitalWorldOnline.Commons.DTOs.Digimon
{
    public class DigimonEvolutionDTO
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Flag for unlocked status.
        /// </summary>
        public byte Unlocked { get; set; }

        /// <summary>
        /// Evolution Type.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Current skill experience.
        /// </summary>
        public int SkillExperience { get; set; }

        /// <summary>
        /// Current total skill points.
        /// </summary>
        public byte SkillPoints { get; set; }

        /// <summary>
        /// Current skill mastery.
        /// </summary>
        public byte SkillMastery { get; set; }

        /// <summary>
        /// Available skills for the evolution (slot 0..3 inherent skills from Digimon_List.bin).
        /// </summary>
        public List<DigimonEvolutionSkillDTO> Skills { get; set; }

        /// <summary>
        /// Cash-shop-acquired memory skills attached to this evolution.  Distinct from
        /// <see cref="Skills"/> — variable-length, identified by SkillId rather than slot.
        /// </summary>
        public List<DigimonMemorySkillDTO> MemorySkills { get; set; } = new();

        /// <summary>
        /// Reference to the owner.
        /// </summary>
        public DigimonDTO Digimon { get; set; }
        public long DigimonId { get; set; }
    }
}