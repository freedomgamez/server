using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.GameServer
{
    /// <summary>
    /// pEvent::Attendance (3107) reply, server→client. Wire format from
    /// <c>cCliGameEvent.cpp:18-65</c> (<c>RecvAttendance</c>):
    /// <list type="bullet">
    ///   <item><c>n4 nResCode</c> — see constants below; only on success
    ///     (<c>ResultSuccess = 100</c>) does the rest of the payload follow</item>
    ///   <item><c>u4 nGiveItemNo</c> — index into the bin's monthly reward array
    ///     (0..31 maps to a real item, 32+ means "no item granted")</item>
    ///   <item><c>n4 nWorkDayHistory</c> — bit flag of which days the player has claimed
    ///     this month (bit N set = day N+1 claimed). Server derives as
    ///     <c>(1 &lt;&lt; TotalDays) - 1</c> for consecutive streaks; future enhancement
    ///     can track non-consecutive days via a stored bitmap column.</item>
    /// </list>
    /// </summary>
    public class AttendanceResponsePacket : PacketWriter
    {
        private const int PacketNumber = 3107;

        // Result codes per the client's switch in cCliGameEvent.cpp:23-49.
        public const int ResultNoEvent = 1;            // event ended or none configured
        public const int ResultNotStarted = 2;
        public const int ResultAlreadyClaimedToday = 3;
        public const int ResultNotEnoughTime = 4;
        public const int ResultStartTimeCheck = 5;
        public const int ResultEventReset = 6;
        public const int ResultSuccess = 100;

        /// <summary>Error reply — only sends <c>nResCode</c>, no item payload.</summary>
        public AttendanceResponsePacket(int resultCode)
        {
            Type(PacketNumber);
            WriteInt(resultCode);
        }

        /// <summary>Success reply — sends <c>nResCode = 100</c> + item index + day-history bitmap.</summary>
        public AttendanceResponsePacket(uint giveItemNo, int workDayHistory)
        {
            Type(PacketNumber);
            WriteInt(ResultSuccess);
            WriteUInt(giveItemNo);
            WriteInt(workDayHistory);
        }
    }
}
