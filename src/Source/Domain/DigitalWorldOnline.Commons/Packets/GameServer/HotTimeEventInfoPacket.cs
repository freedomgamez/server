using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pEvent::HotTimeEvent (3134) — server→client panel push.
    /// Wire format from <c>GS2C_NTF_HOTTIME_EVENT_INFO</c> in
    /// <c>common_vs2019/Protocol/Event_Protocol.h</c>:
    /// <list type="bullet">
    ///   <item><c>n1 nEventState</c> — <c>nsHotTimeEventState</c>:
    ///     <c>0=NO_EVENT</c>, <c>1=NOT_INTIME</c>, <c>2=INTIME</c></item>
    ///   <item><c>n1 nCurrentTableIdx</c> — <c>EventNo</c> of the entry whose campaign+weekday
    ///     matches now; <c>0</c> if none</item>
    ///   <item><c>n1 nNextTableIdx</c> — <c>EventNo</c> of the next-up entry; <c>0</c> if none</item>
    ///   <item><c>n1 bIsGetItem</c> — <c>0</c> if player can still claim, <c>1</c> if already claimed today</item>
    ///   <item><c>n4 nStartTimeLeftTs</c> — seconds until the time-of-day window opens
    ///     (negative-equivalent value can be sent if already in window; client adds it
    ///     to <c>net::game-&gt;GetTimeTS()</c> to compute an absolute timestamp at
    ///     <c>EventContents.cpp:660</c>)</item>
    ///   <item><c>n4 nEndTimeLeftTs</c> — seconds until the time-of-day window closes</item>
    /// </list>
    /// </summary>
    public class HotTimeEventInfoPacket : PacketWriter
    {
        private const int PacketNumber = 3134;

        public HotTimeEventInfoPacket(
            byte eventState,
            byte currentTableIdx,
            byte nextTableIdx,
            bool alreadyClaimed,
            int startTimeLeftSec,
            int endTimeLeftSec)
        {
            Type(PacketNumber);
            WriteByte(eventState);
            WriteByte(currentTableIdx);
            WriteByte(nextTableIdx);
            WriteByte((byte)(alreadyClaimed ? 1 : 0));
            WriteInt(startTimeLeftSec);
            WriteInt(endTimeLeftSec);
        }
    }
}
