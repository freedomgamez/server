using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer.Combat
{
    /// <summary>
    /// pSkill::ResultSkillChip (1122) → cCliGame::RecvMemorySkill_EffectSync.
    /// Cosmetic effect-sync broadcast accompanying a memory-skill cast — the damage
    /// itself rides on SkillHitPacket (1102).  v487 wire format is just:
    ///   u4 nTargetUID
    ///   u4 nSkillCode
    /// </summary>
    public class MemorySkillEffectSyncPacket : PacketWriter
    {
        private const int PacketNumber = 1122;

        public MemorySkillEffectSyncPacket(int targetHandler, int skillCode)
        {
            Type(PacketNumber);
            WriteInt(targetHandler);
            WriteInt(skillCode);
        }
    }
}
