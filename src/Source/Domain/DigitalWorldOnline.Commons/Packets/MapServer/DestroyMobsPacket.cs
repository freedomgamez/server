using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.MapServer
{
    /// <summary>
    /// pGame::Sync (1006) sub-packet pSync::Delete (subtype 2). v487 client's
    /// <c>SyncDeleteObject</c> dispatched via legacy <c>GetClass(u2)</c> truncation —
    /// silently failed for Monster/Npc/Party/CommissionShop. Patched on the client to use
    /// <c>type.m_nClass</c>.
    /// </summary>
    public class DestroyMobsPacket : PacketWriter
    {
        private const int PacketNumber = 1006;

        /// <summary>
        /// Destroys the target mob.
        /// </summary>
        /// <param name="mob">The mob to destroy.</param>
        public DestroyMobsPacket(MobConfigModel mob)
        {
            Type(PacketNumber);
            WriteByte(2);
            WriteShort(1);
            WriteInt(mob.GeneralHandler);
            WriteInt(0); // cSyncType = 0 terminator (was missing pre-fix; client would
                          // pop past EOF reading next subtype — usually returned 0 by
                          // luck, but explicit terminator is safer).
        }
        public DestroyMobsPacket(SummonMobModel mob)
        {
            Type(PacketNumber);
            WriteByte(2);
            WriteShort(1);
            WriteInt(mob.GeneralHandler);
            WriteInt(0);
        }
    }
}