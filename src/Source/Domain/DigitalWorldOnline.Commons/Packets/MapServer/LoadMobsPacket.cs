using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.MapServer
{
    //TODO: abstrair
    public enum SyncConditionEnum
    {
        NormalState = 0,
        CastingSkill = 6,
        PersonalShopPrepare = 8
    }

    /// <summary>
    /// pGame::Sync (1006) sub-packet for spawning mobs in the player's view.
    ///
    /// v487 wire format (matches <c>SyncInObject</c> + <c>SyncInMonster</c> in
    /// <c>cCliGameSync.cpp</c>, with <c>MONSTER_SKILL_GROWTH</c> active in
    /// <c>EngDefine.h:58</c>). Per-mob payload (43 bytes) after the subtype byte +
    /// count short:
    /// <code>
    ///   nSync::Pos pos      8 B  (int X, int Y)
    ///   cType type          8 B  (low u4 = IDX:14|Class:5 — already encoded in
    ///                              GeneralHandler since HandlerRange=65538=
    ///                              (Monster=4 &lt;&lt; 14)+2; high u4 = Type:19)
    ///   nSync::Pos DstPos   8 B
    ///   u1 nHpRate          1 B
    ///   u1 nLevel           1 B  (was Short in pre-fix code — caused a 1-byte drift
    ///                              that corrupted everything after this)
    ///   u4 nMonSkill_Idx    4 B  (was Short — additional drift; 0 = no growth skill)
    ///   int nStack          4 B  (gated by MONSTER_SKILL_GROWTH on client)
    ///   u4 nCondition       4 B
    ///   u4 nCnt             4 B  (seed-sync count, mandatory; 0 = no seeds)
    /// </code>
    /// Trailing <c>WriteInt(0)</c> serves as the cSyncType=0 terminator for
    /// <c>RecvSyncData</c>'s loop.
    ///
    /// Subtype byte: 1 = pSync::New (spawn animation), 3 = pSync::In (already-existing
    /// mob coming into view). Both route through SyncInMonster in v487.
    /// </summary>
    public class LoadMobsPacket : PacketWriter
    {
        private const int PacketNumber = 1006;
        private const byte SubtypeNew = 1;
        private const byte SubtypeIn = 3;

        /// <summary>Spawns the target mob.</summary>
        /// <param name="mob">The mob to spawn.</param>
        public LoadMobsPacket(IList<MobConfigModel> mobs)
        {
            Type(PacketNumber);
            WriteByte(SubtypeIn);
            WriteShort((short)mobs.Count);

            foreach (var mob in mobs)
                WriteMobEntry(mob);

            WriteInt(0); // cSyncType = 0 terminator
        }

        public LoadMobsPacket(IList<SummonMobModel> mobs)
        {
            Type(PacketNumber);
            WriteByte(SubtypeIn);
            WriteShort((short)mobs.Count);

            foreach (var mob in mobs)
                WriteMobEntry(mob);

            WriteInt(0);
        }

        /// <summary>Spawns the target mob (single entry).</summary>
        /// <param name="mob">The mob to spawn.</param>
        public LoadMobsPacket(MobConfigModel mob)
        {
            Type(PacketNumber);
            WriteByte(mob.Respawn ? SubtypeNew : SubtypeIn);
            WriteShort(1);

            WriteMobEntry(mob);

            WriteInt(0);
        }

        public LoadMobsPacket(SummonMobModel mob, bool spawn = false)
        {
            Type(PacketNumber);
            WriteByte(spawn ? SubtypeNew : SubtypeIn);
            WriteShort(1);

            WriteMobEntry(mob);

            WriteInt(0);
        }

        /// <summary>
        /// Writes one mob's per-entry payload — 8 + 8 + 8 + 1 + 1 + 4 + 4 + 4 + 4 = 42 bytes.
        /// </summary>
        private void WriteMobEntry(MobConfigModel mob)
        {
            // pos: previous location (or current, if respawning at the same spot)
            if (mob.Respawn)
            {
                WriteInt(mob.Location.X);
                WriteInt(mob.Location.Y);
            }
            else
            {
                WriteInt(mob.PreviousLocation.X);
                WriteInt(mob.PreviousLocation.Y);
            }

            // cType: 8 bytes packed bitfield (MSVC u4-backed bit fields don't span 32-bit
            // storage units, so low u4 holds IDX|Class, high u4 holds Type).
            WriteUInt((uint)mob.GeneralHandler);
            WriteUInt((uint)mob.Type);

            // DstPos: current location (where the mob is heading / standing now)
            if (mob.Respawn)
            {
                WriteInt(mob.Location.X);
                WriteInt(mob.Location.Y);
            }
            else
            {
                WriteInt(mob.CurrentLocation.X);
                WriteInt(mob.CurrentLocation.Y);
            }

            WriteByte(mob.CurrentHpRate);                  // u1 nHpRate
            WriteByte((byte)mob.Level);                    // u1 nLevel  (was Short)
            WriteUInt(0);                                  // u4 nMonSkill_Idx (was Short(2))
            WriteInt(mob.GrowStack);                       // int nStack
            WriteUInt((uint)SyncConditionEnum.NormalState.GetHashCode()); // u4 nCondition
            WriteInt(0);                                   // u4 nCnt = 0 (NEW; mandatory)
        }

        private void WriteMobEntry(SummonMobModel mob)
        {
            WriteInt(mob.PreviousLocation.X);
            WriteInt(mob.PreviousLocation.Y);

            WriteUInt((uint)mob.GeneralHandler);
            WriteUInt((uint)mob.Type);

            WriteInt(mob.CurrentLocation.X);
            WriteInt(mob.CurrentLocation.Y);

            WriteByte(mob.CurrentHpRate);
            WriteByte((byte)mob.Level);
            WriteUInt(0);
            WriteInt(mob.GrowStack);
            WriteUInt((uint)SyncConditionEnum.NormalState.GetHashCode());
            WriteInt(0);
        }
    }
}
