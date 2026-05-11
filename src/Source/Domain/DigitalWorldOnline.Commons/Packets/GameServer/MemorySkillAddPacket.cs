using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pSkill::AddSkillChip (1118) → cCliGame::RecvDigimonAddSkillSuccess.
    /// Sent after a player consumes a memory-skill chip item (Type 67/68/69) — the
    /// client adds the skill to the partner's evolution and plays the VFX.
    /// Wire format (3× u4):
    ///   u4 nDigimonUID  — partner's handler
    ///   u4 nSkillCode   — CsSkill::s_dwID of the granted memory skill
    ///   u4 nItemIDX     — the chip item type (used by the client to look up the
    ///                     attach VFX via CsItem::sINFO::s_cNif)
    /// </summary>
    public class MemorySkillAddPacket : PacketWriter
    {
        private const int PacketNumber = 1118;

        public MemorySkillAddPacket(int partnerHandler, int skillCode, int chipItemType)
        {
            Type(PacketNumber);
            WriteInt(partnerHandler);
            WriteInt(skillCode);
            WriteInt(chipItemType);
        }
    }
}
