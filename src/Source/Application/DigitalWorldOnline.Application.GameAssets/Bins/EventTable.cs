namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// In-memory <c>Event.bin</c> contents — six discrete sections, each driving a different
/// timed-event subsystem on the v487 client. The loader parses all six; consumers wire up
/// to whichever sections their subsystem needs (Phase 3 swaps Monthly only; HotTime,
/// DailyCheck, and DailyPlay come online in subsequent commits with their own services).
/// </summary>
public sealed class EventTable
{
    /// <summary>§1 — global attendance event window. v487's value is stale (2017),
    /// so consumers should treat this as informational and drive the actual gate from
    /// <c>appsettings</c>.</summary>
    public EventAttendanceWindow Attendance { get; }

    /// <summary>§2 — daily play-time events. <c>TableNo</c> encodes (eventType × 10000) +
    /// number; <c>ET_DAILY = 10000</c>, so values 10001..10044 are daily-play rows.</summary>
    public IReadOnlyList<EventDailyRecord> Daily { get; }

    /// <summary>§3 — friend-recommend rewards. v487 client has no Send-Recommend
    /// path, so server can parse these but cannot trigger them; deferred to Phase 8.</summary>
    public IReadOnlyList<EventRecommendRecord> Recommend { get; }

    /// <summary>§4 — monthly attendance event with 28 daily rewards. v487 ships exactly
    /// one row.</summary>
    public IReadOnlyList<EventMonthlyRecord> Monthly { get; }

    /// <summary>§5 — hot-time XP/drop boost windows. v487 ships 7 entries (one per weekday).
    /// Each carries an active-campaign window plus an intra-day time-of-day window.</summary>
    public IReadOnlyList<EventHotTimeRecord> HotTime { get; }

    /// <summary>§6 — daily-login streak calendars. v487 ships one 100-day group.</summary>
    public IReadOnlyList<EventDailyCheckGroup> DailyCheck { get; }

    public EventTable(
        EventAttendanceWindow attendance,
        IReadOnlyList<EventDailyRecord> daily,
        IReadOnlyList<EventRecommendRecord> recommend,
        IReadOnlyList<EventMonthlyRecord> monthly,
        IReadOnlyList<EventHotTimeRecord> hotTime,
        IReadOnlyList<EventDailyCheckGroup> dailyCheck)
    {
        Attendance = attendance;
        Daily = daily;
        Recommend = recommend;
        Monthly = monthly;
        HotTime = hotTime;
        DailyCheck = dailyCheck;
    }
}

/// <summary>
/// §1 — Two CRT <c>tm</c> structs (sec/min/hr/mday/mon/year-since-1900/wday/yday/isdst)
/// converted to <see cref="DateTime"/>. v487 ships 2017-03-15 → 2017-04-26 (stale).
/// </summary>
public sealed record EventAttendanceWindow(DateTime Start, DateTime End);

/// <summary>§2 — daily play-time threshold reward.</summary>
public sealed record EventDailyRecord(int TableNo, int Minute, IReadOnlyList<EventReward> Rewards);

/// <summary>§3 — friend-recommend reward triggered by an invitee milestone.</summary>
public sealed record EventRecommendRecord(
    int TableNo, int Use, int Index, int Type,
    int SuccessType, int SuccessValue, int ItemKind,
    IReadOnlyList<EventReward> Rewards);

/// <summary>§4 — monthly calendar with 28 day-keyed rewards.</summary>
public sealed record EventMonthlyRecord(int TableNo, IReadOnlyList<EventReward> DailyRewards);

/// <summary>§5 — one weekday's hot-time window. <see cref="StartDate"/>/<see cref="EndDate"/>
/// gate the campaign; <see cref="StartTime"/>/<see cref="EndTime"/> gate the time-of-day
/// during the campaign; <see cref="DayOfWeek"/> picks which weekday this entry applies to
/// (0 = Sunday).</summary>
public sealed record EventHotTimeRecord(
    int EventNo, DateTime StartDate, DateTime EndDate,
    int DayOfWeek, TimeSpan StartTime, TimeSpan EndTime,
    int ItemId, int ItemCount);

/// <summary>§6 — a daily-check calendar group + its reward sequence indexed by streak day.</summary>
public sealed record EventDailyCheckGroup(
    uint GroupNo,
    string Tab, string Title, string Description,
    DateTime StartDate, DateTime EndDate, TimeSpan ResetTime,
    IReadOnlyList<EventDailyCheckReward> Rewards);

public sealed record EventDailyCheckReward(string CheckData, uint ItemId, uint ItemCount);

public sealed record EventReward(int ItemId, ushort Count);
