using DigitalWorldOnline.Application.GameAssets.Bins;
using Microsoft.Extensions.Configuration;

namespace DigitalWorldOnline.Game.Managers
{
    /// <summary>
    /// Drives the v487 click-to-claim attendance event (<c>pEvent::Attendance</c>, 3107).
    /// Window source is <c>appsettings:Attendance:Start</c> / <c>:End</c> rather than the
    /// bin's <c>EventTable.Attendance</c> values — the v487 bin's window is stale 2017
    /// data and would gate every claim. Bin values are still loaded for visibility but
    /// not enforced.
    /// </summary>
    public sealed class AttendanceService
    {
        private readonly EventTableBinLoader _eventTable;
        private readonly DateTime _configuredStart;
        private readonly DateTime _configuredEnd;

        public AttendanceService(IConfiguration config, EventTableBinLoader eventTable)
        {
            _eventTable = eventTable;
            _configuredStart = ParseConfig(config["Attendance:Start"]) ?? DateTime.MinValue;
            _configuredEnd   = ParseConfig(config["Attendance:End"])   ?? DateTime.MaxValue;
        }

        public DateTime ConfiguredStart => _configuredStart;
        public DateTime ConfiguredEnd   => _configuredEnd;
        public EventAttendanceWindow BinWindow => _eventTable.Data.Attendance;

        /// <summary>Returns true if <paramref name="now"/> is inside the configured window.
        /// When <c>Attendance:Start</c>/<c>:End</c> aren't set, defaults are
        /// <c>DateTime.MinValue</c> .. <c>DateTime.MaxValue</c> i.e. always-open — useful
        /// for development and CBT.</summary>
        public bool IsActive(DateTime now) => now >= _configuredStart && now <= _configuredEnd;

        /// <summary>Bit flag matching the v487 client's <c>nWorkDayHistory</c>: bit N set
        /// means day N+1 of the streak was claimed. Conservative consecutive-streak
        /// derivation; replace if a non-consecutive bitmap column is added later.</summary>
        public static int DerivedWorkDayHistory(byte totalDays)
            => totalDays >= 32 ? -1 /* all 32 bits set */ : (1 << totalDays) - 1;

        private static DateTime? ParseConfig(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
                return dt.ToUniversalTime();
            return null;
        }
    }
}
