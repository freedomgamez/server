namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>Event.bin</c>, six sequential sections in this order
/// (see <c>LibProj/CsFileTable/Event.cpp</c> writer at <c>:896-969</c>):
///
///   §1  sATTENDANCE_INFO (72 B = two CRT tm × 36 B)
///   §2  [int n] + n × sEVENT      (1068 B/rec; trailing TCHAR[512] message skipped)
///   §3  [int n] + n × sRECOMMENDE (192 B/rec; trailing 2× TCHAR[32] dates skipped)
///   §4  [int n] + n × sMonthlyEvent (1196 B/rec; embedded TCHAR[512] message skipped)
///   §5  [int n] + variable HotTime entries (wstring framing)
///   §6  [int groupCount] + variable DailyCheck groups (wstring framing)
///
/// All wstrings use the standard <c>[int charCount][charCount × 2 bytes]</c> framing
/// from <c>reference_bin_string_framing.md</c>. <c>TCHAR[N]</c> arrays are fixed-width
/// UTF-16 (2 × N bytes) — those we just <c>Seek</c> past.
/// </summary>
public sealed class EventTableBinLoader
{
    private const string FileName = "Event.bin";

    private EventTable? _data;

    public EventTable Data => _data ?? throw new InvalidOperationException(
        $"{nameof(EventTableBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public EventTable Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static EventTable Parse(BinaryReader r)
    {
        var attendance = ReadAttendance(r);
        var daily = ReadDailyEvents(r);
        var recommend = ReadRecommend(r);
        var monthly = ReadMonthly(r);
        var hotTime = ReadHotTime(r);
        var dailyCheck = ReadDailyCheck(r);
        return new EventTable(attendance, daily, recommend, monthly, hotTime, dailyCheck);
    }

    // ---------------------------------------------------------------- §1
    private static EventAttendanceWindow ReadAttendance(BinaryReader r)
    {
        var start = ReadTm(r);
        var end = ReadTm(r);
        return new EventAttendanceWindow(start, end);
    }

    /// <summary>CRT <c>struct tm</c> = 9 ints {sec, min, hr, mday, mon, year-since-1900,
    /// wday, yday, isdst}. Out-of-range values (e.g. yday=0) are clamped to the start of
    /// the calendar so a malformed bin doesn't throw.</summary>
    private static DateTime ReadTm(BinaryReader r)
    {
        int sec = r.ReadInt32();
        int min = r.ReadInt32();
        int hr = r.ReadInt32();
        int mday = r.ReadInt32();
        int mon = r.ReadInt32();
        int year = r.ReadInt32();
        _ = r.ReadInt32(); // wday  — derivable
        _ = r.ReadInt32(); // yday  — sometimes garbage in v487 bins (saw 411319584)
        _ = r.ReadInt32(); // isdst
        try { return new DateTime(year + 1900, mon + 1, Math.Max(1, mday), hr, min, Math.Min(59, sec)); }
        catch (ArgumentOutOfRangeException) { return DateTime.MinValue; }
    }

    // ---------------------------------------------------------------- §2
    private static List<EventDailyRecord> ReadDailyEvents(BinaryReader r)
    {
        int n = r.ReadInt32();
        var list = new List<EventDailyRecord>(n);
        for (int i = 0; i < n; i++)
        {
            int tableNo = r.ReadInt32();
            int minute = r.ReadInt32();
            var rewards = ReadRewardsFixed(r, 6);
            r.BaseStream.Seek(1024, SeekOrigin.Current); // s_szMessage[512] (TCHAR = 2B)
            list.Add(new EventDailyRecord(tableNo, minute, rewards));
        }
        return list;
    }

    // ---------------------------------------------------------------- §3
    private static List<EventRecommendRecord> ReadRecommend(BinaryReader r)
    {
        int n = r.ReadInt32();
        var list = new List<EventRecommendRecord>(n);
        for (int i = 0; i < n; i++)
        {
            int tableNo = r.ReadInt32();
            int use = r.ReadInt32();
            int index = r.ReadInt32();
            int type = r.ReadInt32();
            int succType = r.ReadInt32();
            int succValue = r.ReadInt32();
            int itemKind = r.ReadInt32();
            var rewards = ReadRewardsFixed(r, 6);
            r.BaseStream.Seek(64 + 64, SeekOrigin.Current); // s_cStartTime/s_cEndTime TCHAR[32] each
            list.Add(new EventRecommendRecord(tableNo, use, index, type, succType, succValue, itemKind, rewards));
        }
        return list;
    }

    // ---------------------------------------------------------------- §4
    private static List<EventMonthlyRecord> ReadMonthly(BinaryReader r)
    {
        int n = r.ReadInt32();
        var list = new List<EventMonthlyRecord>(n);
        for (int i = 0; i < n; i++)
        {
            int tableNo = r.ReadInt32();
            r.BaseStream.Seek(1024, SeekOrigin.Current); // s_szMessage[512] TCHAR
            var rewards = ReadRewardsFixed(r, 28);
            list.Add(new EventMonthlyRecord(tableNo, rewards));
        }
        return list;
    }

    // ---------------------------------------------------------------- §5
    private static List<EventHotTimeRecord> ReadHotTime(BinaryReader r)
    {
        int n = r.ReadInt32();
        var list = new List<EventHotTimeRecord>(n);
        for (int i = 0; i < n; i++)
        {
            int eno = r.ReadInt32();
            if (eno == 0) continue;
            string sd = ReadWString(r);
            string ed = ReadWString(r);
            int day = r.ReadInt32();
            string st = ReadWString(r);
            string et = ReadWString(r);
            int itemNo = r.ReadInt32();
            int itemCnt = r.ReadInt32();
            list.Add(new EventHotTimeRecord(
                eno,
                ParseDate(sd), ParseDate(ed),
                day,
                ParseTime(st), ParseTime(et),
                itemNo, itemCnt));
        }
        return list;
    }

    // ---------------------------------------------------------------- §6
    private static List<EventDailyCheckGroup> ReadDailyCheck(BinaryReader r)
    {
        int groupCount = r.ReadInt32();
        var groups = new List<EventDailyCheckGroup>(groupCount);
        for (int g = 0; g < groupCount; g++)
        {
            uint groupNo = r.ReadUInt32();
            if (groupNo == 0) continue;
            string tab = ReadWString(r);
            string title = ReadWString(r);
            string desc = ReadWString(r);
            string startStr = ReadWString(r);
            string endStr = ReadWString(r);
            string resetStr = ReadWString(r);
            int rewardCount = r.ReadInt32();
            var rewards = new List<EventDailyCheckReward>(rewardCount);
            for (int k = 0; k < rewardCount; k++)
            {
                string checkData = ReadWString(r);
                uint itemId = r.ReadUInt32();
                uint itemCnt = r.ReadUInt32();
                rewards.Add(new EventDailyCheckReward(checkData, itemId, itemCnt));
            }
            groups.Add(new EventDailyCheckGroup(
                groupNo, tab, title, desc,
                ParseDate(startStr), ParseDate(endStr), ParseTime(resetStr),
                rewards));
        }
        return groups;
    }

    // ---------------------------------------------------------------- helpers
    /// <summary>Reads N reward slots: N×int item-id followed by N×ushort item-count.</summary>
    private static IReadOnlyList<EventReward> ReadRewardsFixed(BinaryReader r, int n)
    {
        var ids = new int[n];
        for (int i = 0; i < n; i++) ids[i] = r.ReadInt32();
        var rewards = new List<EventReward>(n);
        for (int i = 0; i < n; i++)
        {
            ushort cnt = r.ReadUInt16();
            rewards.Add(new EventReward(ids[i], cnt));
        }
        return rewards;
    }

    private static string ReadWString(BinaryReader r)
    {
        int charCount = r.ReadInt32();
        if (charCount <= 0) return string.Empty;
        var bytes = r.ReadBytes(charCount * 2);
        return System.Text.Encoding.Unicode.GetString(bytes);
    }

    private static DateTime ParseDate(string s)
    {
        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeLocal, out var dt))
            return dt;
        return DateTime.MinValue;
    }

    private static TimeSpan ParseTime(string s)
    {
        if (TimeSpan.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var ts))
            return ts;
        return TimeSpan.Zero;
    }
}
