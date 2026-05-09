using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// Server reply to <c>C2GS_SEND_DIGIMON_SKILL_LIMIT_OPEN</c> (pDigimon::DigimonSkillLimitOpen, 3245).
    /// Layout matches <c>GS2C_RECV_DIGIMON_SKILL_LIMIT_OPEN</c> in
    /// <c>common_vs2019/Protocol/DigimonSkill_Protocol.h</c>:
    ///
    ///   u4 nResult          (0 = Success, non-zero = error code → client cPrintMsg)
    ///   u4 nDigimonEvoArray (echo of request's evo-slot index)
    ///   cEvoUnit cEvo       (19 bytes — see layout in writer)
    ///   u4 nItemPos
    ///   u4 nItemType
    ///
    /// On non-success the client returns early before reading cEvoUnit, so we still write
    /// 19 zero bytes for that range to keep wire-shape predictable for any tools snooping it.
    ///
    /// cEvoUnit on wire (#pragma pack(1) in client):
    ///   bitfield u4 #1: SkillExp(26 bits, low) | SkillExpLevel(6 bits)
    ///   bitfield u4 #2: SlotState(4) | MaxSkillLevelStep(8) | Reserved(20)
    ///   u1 SkillPoint
    ///   u1 SkillLevel[5]      (nLimit::Skill = 5)
    ///   u1 SkillMaxLevel[5]   (SERVER_KSW_SKILL_LV_EXPEND_181205 — defined for KOR/USA/TW/TH/HK)
    /// </summary>
    public class DigimonSkillLimitOpenResultPacket : PacketWriter
    {
        private const int PacketNumber = 3245;
        private const int SkillCount = 5;

        public DigimonSkillLimitOpenResultPacket(
            uint result,
            int digimonEvoArray,
            DigimonEvolutionModel? evolution,
            int itemPos,
            int itemType)
        {
            Type(PacketNumber);
            WriteUInt(result);
            WriteInt(digimonEvoArray);

            // cEvoUnit (19 bytes)
            uint bitfield1 = 0;
            uint bitfield2 = 0;
            byte skillPoint = 0;
            byte[] skillLevel = new byte[SkillCount];
            byte[] skillMaxLevel = new byte[SkillCount];

            if (evolution != null)
            {
                uint skillExp = (uint)evolution.SkillExperience & 0x03FFFFFFu;       // 26 bits
                uint skillExpLevel = (uint)evolution.SkillMastery & 0x3Fu;            // 6 bits
                bitfield1 = (skillExpLevel << 26) | skillExp;

                uint slotState = (uint)evolution.Unlocked & 0x0Fu;                    // 4 bits
                uint maxSkillStep = 0;                                                // 8 bits unused server-side
                bitfield2 = (slotState) | (maxSkillStep << 4);

                skillPoint = evolution.SkillPoints;

                for (int i = 0; i < SkillCount && i < evolution.Skills.Count; i++)
                {
                    skillLevel[i] = evolution.Skills[i].CurrentLevel;
                    skillMaxLevel[i] = evolution.Skills[i].MaxLevel;
                }
            }

            WriteUInt(bitfield1);
            WriteUInt(bitfield2);
            WriteByte(skillPoint);
            for (int i = 0; i < SkillCount; i++) WriteByte(skillLevel[i]);
            for (int i = 0; i < SkillCount; i++) WriteByte(skillMaxLevel[i]);

            WriteInt(itemPos);
            WriteInt(itemType);
        }
    }

    /// <summary>nsSkillLimitOpenResult enum (client-side cPrintMsg codes for non-success).</summary>
    public static class SkillLimitOpenResult
    {
        public const uint Success = 0;
        public const uint NoItem = 1;
        public const uint InvalidEvolution = 2;
        public const uint StageNotAllowed = 3;
        public const uint AlreadyMaxed = 4;
        public const uint InternalError = 5;
    }
}
