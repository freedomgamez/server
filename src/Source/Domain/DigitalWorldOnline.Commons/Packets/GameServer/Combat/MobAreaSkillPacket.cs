using System.Collections.Generic;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer.Combat
{
    /// <summary>
    /// Wide-skill apply packet sent by mob skills like ASSEMBLE / DISPERSE / GatheringExt /
    /// DisperseExt where the cast paints a "damage-mark" VFX on each marked target before
    /// resolution.  Maps to client handler <c>cCliGame::RecvWideSkillApply</c> →
    /// <c>_RecvWideSkillApplyAround_ExistHitter</c> in <c>cCliGameSkill.cpp:232-444</c>.
    ///
    /// Packet ID = <c>pSkill::AroundSkill</c> (1110 = nScope::Skill[=1100] + 10).
    ///
    /// Wire format (v487):
    /// <code>
    ///   u4 nHitterUID         — caster mob's GeneralHandler
    ///   u4 nSkillArrIDX       — skill index (CsMonsterSkill::s_nSkill_IDX)
    ///   IF skill.SkillType ∈ { ASSEMBLE (16), DISPERSE (17), GatheringExt (25), DisperseExt (26) }:
    ///     u4 nEffectTargetCnt
    ///     u4 × nEffectTargetCnt  marked target UIDs (Damage_Mark.nif placed by client)
    ///   u2 nTargetCount       — number of per-target damage entries
    ///   IF nEffectTargetCnt was set (mark loop ran):
    ///     u4 nTmpUID          — throwaway value (client pops + ignores)
    ///   ELSE:
    ///     u4 nTargetUID       — first hit target (when no marks)
    ///   FOR each nTargetCount:
    ///     u1 uDead            — 1 if newHp ≤ 0
    ///     u4 nUserUID         — target's handler
    ///     n4 nDamage          — damage value
    ///     u1 nTargetHpRate    — target HP percentage (0–255)
    /// </code>
    ///
    /// The conditional mark-list IS what tells the client to display the gather/scatter
    /// VFX on those targets — case 4 (HP_VAL_DECREASE) writes the same shape WITHOUT
    /// the mark list (so the client's switch on SkillType skips that block when parsing).
    /// </summary>
    public class MobAreaSkillPacket : PacketWriter
    {
        private const int PacketNumber = 1110;

        public readonly struct Hit
        {
            public readonly int Handler;
            public readonly int Damage;
            public readonly byte HpRate;
            public readonly bool Dead;
            public Hit(int handler, int damage, byte hpRate, bool dead)
            {
                Handler = handler;
                Damage = damage;
                HpRate = hpRate;
                Dead = dead;
            }
        }

        /// <summary>
        /// <paramref name="marks"/> is the optional damage-mark target list (ASSEMBLE / DISPERSE
        /// family).  Pass an empty list for plain wide-AoE (matches HP_VAL_DECREASE wire shape).
        /// <paramref name="hits"/> drives the per-target damage tail.
        /// </summary>
        public MobAreaSkillPacket(int casterHandler, int skillIndex, IReadOnlyList<int> marks, IReadOnlyList<Hit> hits)
        {
            Type(PacketNumber);
            WriteInt(casterHandler);
            WriteInt(skillIndex);

            // Mark block — present only when the server populated marks.  The client's
            // switch on SkillType decides whether to pop this — caller MUST ensure marks
            // are only passed for SkillType ∈ {16,17,25,26}.
            if (marks.Count > 0)
            {
                WriteInt(marks.Count);
                foreach (var m in marks) WriteInt(m);
            }

            WriteShort((short)hits.Count);

            if (marks.Count > 0)
            {
                // nTmpUID — client pops and discards.  Use first hit's handler (any value works).
                WriteInt(hits.Count > 0 ? hits[0].Handler : 0);
            }
            else
            {
                // Primary target UID — first hit (or 0 if none).
                WriteInt(hits.Count > 0 ? hits[0].Handler : 0);
            }

            foreach (var h in hits)
            {
                WriteByte((byte)(h.Dead ? 1 : 0));
                WriteInt(h.Handler);
                WriteInt(h.Damage);
                WriteByte(h.HpRate);
            }
        }
    }
}
