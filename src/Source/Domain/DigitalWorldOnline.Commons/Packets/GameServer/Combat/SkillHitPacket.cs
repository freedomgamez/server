using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer.Combat
{
    public class SkillHitPacket : PacketWriter
    {
        private const int PacketNumber = 1102;

        /// <summary>
        /// Uses the skill into the target.
        /// </summary>
        /// <param name="attackerHandler">The source of the skill</param>
        /// <param name="targetHandler">The target of the skill</param>
        /// <param name="skillSlot">The skill source slot</param>
        /// <param name="finalDamage">The final damage of the skill</param>
        /// <param name="targetCurrentHpRate">The final hp rate of the target</param>
        public SkillHitPacket(int attackerHandler, int targetHandler, byte skillSlot, int finalDamage, byte targetCurrentHpRate)
        {
            // pSkill::ApplyAround (1102) → _RecvSkillApplyAround_ExistHitter
            // (cCliGameSkill.cpp:924). v487 wire format:
            //   u1 bIsItem            (0 = skill, not item)
            //   u4 nHitterUID, u4 nTargetUID, u4 nSkillArrIDX
            //   u1 nHitterHpRate, u1 nTargetHpRate
            //   [hitter attribute loop] u1 nP — terminates when nP >= MaxExtStat (=18)
            //   [target attribute loop] u1 nP, n4 nV; repeated until u1 nP >= 18
            //   u4 nBattleOption
            // Pre-fix code wrote WriteByte(0) for nBattleOption (1 byte) where client reads 4 →
            // buffer underrun corrupting the next packet. Fixed to WriteInt(0).
            Type(PacketNumber);
            WriteByte(0);                          // bIsItem = 0 (skill)
            WriteInt(attackerHandler);             // u4 nHitterUID
            WriteInt(targetHandler);               // u4 nTargetUID
            WriteInt(skillSlot);                   // u4 nSkillArrIDX
            WriteByte(255);                        // u1 nHitterHpRate (max)
            WriteByte(targetCurrentHpRate);        // u1 nTargetHpRate
            WriteByte(18);                         // hitter loop sentinel (MaxExtStat=18 terminates)
            WriteByte(4);                          // target loop: nP=4 (HP attribute)
            WriteInt(finalDamage * -1);            // n4 nV (damage)
            WriteByte(255);                        // target loop sentinel
            WriteInt(0);                           // u4 nBattleOption  (was WriteByte — underrun)
        }
    }
}