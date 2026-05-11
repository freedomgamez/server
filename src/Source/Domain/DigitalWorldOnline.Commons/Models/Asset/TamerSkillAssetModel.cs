
namespace DigitalWorldOnline.Commons.Models.Asset
{
    public sealed partial class TamerSkillAssetModel
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; set; }


        public int SkillId { get; set; }


        public int SkillCode { get; set; }

        public int Duration { get; set; }

        /// <summary>
        /// CsTamerSkill::sTSkill_INFO::s_nUseState — 0 = always usable, 1 = forbidden in combat.
        /// </summary>
        public byte UseState { get; set; }

        /// <summary>
        /// CsTamerSkill::sTSkill_INFO::s_nUse_Are_Check — 1 = consult AreaCheck table.
        /// </summary>
        public byte UseAreaCheck { get; set; }

        /// <summary>
        /// CsTamerSkill::sTSkill_INFO::s_nAvailable — 1 = blacklist, 2 = whitelist.
        /// </summary>
        public byte Available { get; set; }
    }
}
