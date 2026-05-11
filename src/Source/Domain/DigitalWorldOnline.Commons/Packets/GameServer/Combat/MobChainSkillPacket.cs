using System.Collections.Generic;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer.Combat
{
    /// <summary>
    /// Chain-skill VFX broadcast for mob ChainBounce (eEFFECT_TYPE = 28) skills.
    /// Maps to client handler <c>cCliGame::RecvRaidChainSkill</c>
    /// (<c>cCliGameSkill.cpp:2826-2889</c>) which renders the Qinglongmon chain-lightning
    /// VFX between consecutive target pairs.  Packet ID = <c>pGameTwo::SkillHitEffect</c>
    /// = 16027 (auto-incremented from <c>nScope::GameTwo = 16000</c>, 27 entries deep).
    ///
    /// Wire format (v487):
    /// <code>
    ///   u4 nCasterUID
    ///   u4 nCastingSkillIDX
    ///   u2 nSkillType        — 1 selects the chain-VFX rendering path
    ///   n4 nTargetCnt        — total chain length (>= 2)
    ///   u4 uTargetUID        — first target's GeneralHandler
    ///   u4 × (nTargetCnt-1)  — remaining chain target GeneralHandlers in order
    /// </code>
    ///
    /// IMPORTANT: damage is NOT carried in this packet — the client only renders the
    /// chain VFX.  Apply per-target damage separately via <see cref="SkillHitPacket"/>
    /// before/alongside this broadcast.
    /// </summary>
    public class MobChainSkillPacket : PacketWriter
    {
        private const int PacketNumber = 16027;
        private const ushort ChainVfxSubtype = 1;

        public MobChainSkillPacket(int casterHandler, int skillIndex, IReadOnlyList<int> chainHandlers)
        {
            Type(PacketNumber);
            WriteInt(casterHandler);
            WriteInt(skillIndex);
            WriteShort((short)ChainVfxSubtype);
            WriteInt(chainHandlers.Count);
            foreach (var h in chainHandlers) WriteInt(h);
        }
    }
}
