using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer.Combat
{
    /// <summary>
    /// pGame::Sync (1006) sub-packet pSync::Move (subtype 6). v487 client's
    /// <c>SyncMoveObject</c> dispatched via legacy <c>GetClass(u2)</c> — broken for
    /// nClass ≥ 4. Patched on the client to use <c>type.m_nClass</c>.
    /// </summary>
    public class MobRunPacket : PacketWriter
    {
        private const int PacketNumber = 1006;

        /// <summary>
        /// Default mob movimentation packet.
        /// </summary>
        /// <param name="mob">The mob that is moving</param>
        public MobRunPacket(MobConfigModel mob)
        {
            Type(PacketNumber);
            WriteByte(6);
            WriteShort(1);
            WriteUInt((uint)mob.GeneralHandler);
            WriteInt(mob.CurrentLocation.X);
            WriteInt(mob.CurrentLocation.Y);
            WriteInt(0);
        }
        public MobRunPacket(SummonMobModel mob)
        {
            Type(PacketNumber);
            WriteByte(6);
            WriteShort(1);
            WriteUInt((uint)mob.GeneralHandler);
            WriteInt(mob.CurrentLocation.X);
            WriteInt(mob.CurrentLocation.Y);
            WriteInt(0);
        }
        public MobRunPacket(int X,int Y, uint Handler)
        {
            Type(PacketNumber);
            WriteByte(6);
            WriteShort(1);
            WriteUInt((uint)Handler);
            WriteInt(X);
            WriteInt(Y);
            WriteInt(0);
        }
    }
}