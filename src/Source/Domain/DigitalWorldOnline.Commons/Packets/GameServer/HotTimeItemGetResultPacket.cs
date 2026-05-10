using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pEvent::HotTimeItemRequest (3135) reply, server→client. Wire format from
    /// <c>GS2C_RECV_HOTTIME_GET_RESULT</c>: single <c>n4 nResult</c> per
    /// <c>nsHotTimeResult</c>. Codes are message-string IDs the client passes straight
    /// to <c>cPrintMsg::PrintMsg</c> (<c>EventContents.cpp:686</c>).
    /// </summary>
    public class HotTimeItemGetResultPacket : PacketWriter
    {
        private const int PacketNumber = 3135;

        public const int ResultSuccess = 0;
        public const int ResultAlreadyClaimed = 30597;
        public const int ResultNotInTime = 30598;

        public HotTimeItemGetResultPacket(int result)
        {
            Type(PacketNumber);
            WriteInt(result);
        }
    }
}
