using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pEvent::DailyEventInfo (3106) — server→client daily play-time panel push.
    /// Wire format from <c>GS2C_RECV_DAILY_EVENT_DATA</c>
    /// (<c>common_vs2019/Protocol/Event_Protocol.h:30-43</c>) and the recv handler at
    /// <c>cCliGameReceive.cpp:5999-6011</c>:
    /// <list type="bullet">
    ///   <item><c>n4 m_nEventNo</c> — <c>Event.bin</c> §2 <c>TableNo</c> the client looks
    ///     up via <c>g_pEventMng-&gt;GetMap(ET_DAILY, eventNo)</c>; <c>-1</c> closes the UI</item>
    ///   <item><c>u4 m_nRemainedTimeTS</c> — seconds until the current threshold fires</item>
    ///   <item><c>u4 m_nTotalTime</c> — total seconds for this threshold (client uses both
    ///     to compute the on-screen progress bar)</item>
    ///   <item><c>u1 m_nWeek</c> — current weekday (0=Sun..6=Sat); the client clamps
    ///     anything &gt; 6 to <c>0xFF</c> meaning "no weekday context"
    ///     (<c>cCliGameReceive.cpp:6007-6008</c>)</item>
    /// </list>
    /// Pre-existing bugs fixed in C7: was sending raw <c>RewardIndex</c> (0..3) as
    /// <c>nEventNo</c> instead of the bin <c>TableNo</c> (10000+) so the client's
    /// <c>GetMap(ET_DAILY, 0)</c> lookup never found a record; was duplicating
    /// <c>RemainingTime</c> in place of <c>TotalTime</c>; was hardcoding <c>nWeek = 1</c>.
    /// </summary>
    public class TimeRewardPacket : PacketWriter
    {
        private const int PacketNumber = 3106;

        public TimeRewardPacket(TimeReward timeReward)
        {
            Type(PacketNumber);
            WriteInt(timeReward.CurrentEventNo);
            WriteInt(Math.Max(0, timeReward.RemainingTime));
            WriteInt(timeReward.CurrentTotalSeconds);
            WriteByte((byte)DateTime.UtcNow.DayOfWeek);
        }
    }
}
