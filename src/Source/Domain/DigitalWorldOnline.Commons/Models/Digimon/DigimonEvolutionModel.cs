namespace DigitalWorldOnline.Commons.Models.Digimon
{
    public sealed partial class DigimonEvolutionModel
    {

        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Flag for unlocked evolution.
        /// </summary>
        public byte Unlocked { get; private set; }

        /// <summary>
        /// Evolution Type.
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// Current skill experience.
        /// </summary>
        public int SkillExperience { get; private set; }

        /// <summary>
        /// Current total skill points.
        /// </summary>
        public byte SkillPoints { get; private set; }

        /// <summary>
        /// Current skill mastery.
        /// </summary>
        public byte SkillMastery { get; private set; }

        /// <summary>
        /// Available skills for the evolution (fixed slot 0..3 inherent skills).
        /// </summary>
        public List<DigimonEvolutionSkillModel> Skills { get; private set; }

        /// <summary>
        /// Cash-shop-acquired memory skills attached to this evolution (variable-length).
        /// </summary>
        public List<DigimonMemorySkillModel> MemorySkills { get; private set; } = new();
        
        public DigimonEvolutionModel(int type)
        {
 
            Type = type;

            Skills = new List<DigimonEvolutionSkillModel>()
            {
                new DigimonEvolutionSkillModel(),
                new DigimonEvolutionSkillModel(),
                new DigimonEvolutionSkillModel(),
                new DigimonEvolutionSkillModel(),
                new DigimonEvolutionSkillModel()
            };
        }
    }
}