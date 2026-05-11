using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    public class AvailableChannelsPacket : PacketWriter
    {
        // pSvr::ChannelInfo in common_vs2019/pServer.h, counting from
        // Begin = nScope::Svr (1700) → ChannelInfo = 1713.  Same packet ID
        // bidirectionally — client request carries no payload, server reply
        // carries (u1 idx, u1 load) × N + 0xFF sentinel.  Prior "fix" miscounted
        // to 1712 (which is actually pSvr::TryLogin and undefined as a recv
        // handler in the client → client logged "unknown protocol(1712)").
        private const int PacketNumber = 1713;

        /// <summary>
        /// Sends the current available channels list.
        /// </summary>
        public AvailableChannelsPacket(Dictionary<byte, byte> channels)
        {
            Type(PacketNumber);
            foreach (var channel in channels)
            {
                WriteByte(channel.Key);
                WriteByte(UtilitiesFunctions.GetChannelLoad(channel.Value));
            }
            WriteByte(byte.MaxValue);
        }
    }
}