using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.MapServer
{
    /// <summary>
    /// pGame::Sync (1006) sub-packet pSync::Out (subtype 4). Wire format is fine for v487.
    /// v487 client's <c>SyncOutObject</c> originally dispatched via the legacy
    /// <c>GetClass(u2 nUID)</c> helper which truncated u4 → u2 and silently failed for
    /// nClass ≥ 4 (Monster/Npc/Party/CommissionShop). Patched on the client in
    /// <c>cCliGameSync.cpp</c> to use <c>type.m_nClass</c> — bitfield read works for all classes.
    /// </summary>
    public class UnloadMobsPacket : PacketWriter
    {
        private const int PacketNumber = 1006;

        /// <summary>
        /// Despawns the target mob.
        /// </summary>
        /// <param name="mob">The mob to despawn.</param>
        public UnloadMobsPacket(MobConfigModel mob)
        {
            Type(PacketNumber);
            WriteByte(4);
            WriteShort(1);
            WriteInt(mob.GeneralHandler);
            WriteInt(mob.CurrentLocation.X);
            WriteInt(mob.CurrentLocation.Y);
            WriteInt(0);
        }
        public UnloadMobsPacket(SummonMobModel mob)
        {
            Type(PacketNumber);
            WriteByte(4);
            WriteShort(1);
            WriteInt(mob.GeneralHandler);
            WriteInt(mob.CurrentLocation.X);
            WriteInt(mob.CurrentLocation.Y);
            WriteInt(0);
        }
    }
}