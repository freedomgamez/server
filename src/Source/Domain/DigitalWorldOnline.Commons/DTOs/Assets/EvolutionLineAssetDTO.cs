namespace DigitalWorldOnline.Commons.DTOs.Assets
{
    public sealed class EvolutionLineAssetDTO
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Base type for the evolution.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Required slot level to unlock. Bin path: maps from <c>DigimonEvo.bin</c>'s
        /// <c>m_nEvoSlot</c>. Consumed by <c>QuestDeliverPacketProcessor</c> as an index
        /// into <c>Tamer.Partner.Evolutions[SlotLevel - 1]</c> — must be 1-based and populated,
        /// otherwise quest-driven evolution unlocks throw on -1 indexing.
        /// </summary>
        public byte SlotLevel { get; set; }

        /// <summary>
        /// Required partner level to unlock.
        /// </summary>
        public byte UnlockLevel { get; set; }

        /// <summary>
        /// Required quest to unlock.
        /// </summary>
        public short UnlockQuestId { get; set; }

        /// <summary>
        /// Required item to unlock.
        /// </summary>
        public int UnlockItemSection { get; set; }

        /// <summary>
        /// Required item amount to unlock.
        /// </summary>
        public int UnlockItemSectionAmount { get; set; }

        public int RequiredItem { get; private set; }

        public int RequiredAmount { get; private set; }

        /// <summary>
        /// Per-slot max skill levels for this evolution form, sourced from <c>DMBase.bin</c>
        /// section 12 (DigimonEvoMaxLevel) — typically <c>[10, 15, 20, 25]</c> for partner
        /// digimon. Null when populated from the legacy DB path; the receiving model defaults
        /// to 10 in that case. Bin-backed handlers populate this from
        /// <c>DMBase.DigimonEvoMaxLevel[evoStage].SkillMaxLevels</c> where <c>evoStage</c> is
        /// the line's digimon type's <c>EvolutionType</c> from <c>Digimon_List.bin</c>.
        /// </summary>
        public byte[]? SkillMaxLevels { get; set; }

        /// <summary>
        /// Available stages.
        /// </summary>
        public List<EvolutionStageAssetDTO> Stages { get; set; }

        /// <summary>
        /// Reference to the owner.
        /// </summary>
        public long EvolutionId { get; set; }
        public EvolutionAssetDTO Evolution { get; set; }
    }
}