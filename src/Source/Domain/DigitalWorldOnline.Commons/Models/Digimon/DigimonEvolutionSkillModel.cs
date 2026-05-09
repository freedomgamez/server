namespace DigitalWorldOnline.Commons.Models.Digimon
{
    public sealed partial class DigimonEvolutionSkillModel
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Current skill level.
        /// </summary>
        public byte CurrentLevel { get; private set; }

        /// <summary>
        /// Current Skill Cooldown.
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        /// Current Skill Cooldown End Time.
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Max skill level.
        /// </summary>
        public byte MaxLevel { get; private set; }

        public DigimonEvolutionSkillModel()
        {
            CurrentLevel = 1;
            MaxLevel = 10; // default for serialization paths; the real value is plumbed via SetMaxLevel from DMBase.bin section 12 (DigimonEvoMaxLevel) — caps differ by evolution stage and skill slot.
        }

        /// <summary>
        /// Sets the per-slot max skill level. Source of truth is <c>DMBase.bin</c> section 12
        /// (DigimonEvoMaxLevel). Called by <see cref="DigimonEvolutionModel"/> when it propagates
        /// the bin-resolved caps after construction.
        /// </summary>
        public void SetMaxLevel(byte maxLevel)
        {
            MaxLevel = maxLevel;
        }
    }
}
