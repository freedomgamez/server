using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer.Combat
{
    /// <summary>
    /// pSkill::ChipCoolTime (1121) — server's "memory-skill cast succeeded" reply.
    /// Triggers two things on the v487 client (cCliGameSkill::RecvDigimonMemorySkill_UseSuccess):
    ///   1) Cooldown UI starts on the skill icon (`s_CoolTime.Start()`).  The
    ///      client uses its own bin cooldown value, not the cooldownMs we send —
    ///      the wire field is decoded but discarded.
    ///   2) Memory-chip count decrements locally on the inventory UI
    ///      (`DecreaseItem_TypeLS(DigimonSkillMemoryChip, ...)`).
    /// Without this packet the skill fires but its icon never enters cooldown
    /// state and the chip stack count stays out of sync until the next inventory
    /// refresh.
    /// </summary>
    public class MemorySkillUseSuccessPacket : PacketWriter
    {
        private const int PacketNumber = 1121;

        public MemorySkillUseSuccessPacket(int skillCode, int cooldownMs)
        {
            Type(PacketNumber);
            WriteInt(skillCode);
            WriteInt(cooldownMs);
        }
    }
}
