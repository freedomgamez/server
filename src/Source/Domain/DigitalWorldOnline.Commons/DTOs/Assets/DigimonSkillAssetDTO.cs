namespace DigitalWorldOnline.Commons.DTOs.Assets
{
    public sealed class DigimonSkillAssetDTO
    {
        /// <summary>
        /// Unique sequential identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Digimon type/model
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Skill slot (1=F1, 2=F2... x=Fx)
        /// </summary>
        public byte Slot { get; set; }

        /// <summary>
        /// Skill id.
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// Required level of the PREVIOUS skill slot before this one can be leveled up.
        /// Mirrors the bin's <c>s_Skill[i].s_nReqPrevSkillLevel</c> (client
        /// <c>DigimonSkill.cpp:113</c>).  0 = no gate (slot 0 always uses 0).
        /// </summary>
        public byte RequiredLevel { get; set; }
    }
}