using System.Collections.Concurrent;
using DigitalWorldOnline.Application.GameAssets.Bins;

namespace DigitalWorldOnline.Game.Managers
{
    /// <summary>
    /// Drives the v487 client's Hot Time UI panel + click-to-claim logic. Backed by
    /// <c>Event.bin</c> §5 (parsed into <c>EventTableBinLoader.Data.HotTime</c>): one entry
    /// per weekday, each with a campaign-active window
    /// (<c>StartDate</c>..<c>EndDate</c>), a time-of-day window
    /// (<c>StartTime</c>..<c>EndTime</c>), and a single reward (<c>ItemId × ItemCount</c>).
    /// <para>
    /// **Claim tracking is in-memory only** — keys = <c>(characterId, eventNo)</c> →
    /// last-claim UTC date. Survives across map enters and channel switches but NOT across
    /// server restart. TODO: persist via a sibling <c>Event_HotTimeClaim</c> table when C10
    /// (DailyCheck) lands its own per-character event-state schema; this is the simplest
    /// version that exercises the v487 wire protocol end-to-end.
    /// </para>
    /// </summary>
    public sealed class HotTimeService
    {
        private readonly EventTableBinLoader _eventTable;
        private readonly ConcurrentDictionary<(long CharacterId, int EventNo), DateOnly> _lastClaim = new();

        public HotTimeService(EventTableBinLoader eventTable)
        {
            _eventTable = eventTable;
        }

        /// <summary>
        /// Mirrors <c>nsHotTimeEventState</c> from
        /// <c>common_vs2019/Protocol/Event_Define.h:14-22</c>.
        /// </summary>
        public enum State : byte
        {
            NoEvent = 0,
            NotInTime = 1,
            InTime = 2
        }

        /// <summary>One snapshot of hot-time state for a given <c>now</c>.</summary>
        public readonly record struct Snapshot(
            State CurrentState,
            EventHotTimeRecord? Current,
            EventHotTimeRecord? Next,
            int StartTimeLeftSec,
            int EndTimeLeftSec);

        /// <summary>
        /// Compute the panel snapshot for <paramref name="now"/>. Picks the entry whose
        /// <c>DayOfWeek</c> matches today AND whose campaign window covers <paramref name="now"/>.
        /// State distinguishes "before window opens" (NotInTime) from "in window" (InTime);
        /// other days / out-of-campaign return NoEvent.
        /// </summary>
        public Snapshot GetSnapshot(DateTime now)
        {
            if (_eventTable.Data.HotTime.Count == 0)
                return new Snapshot(State.NoEvent, null, null, 0, 0);

            int todayDow = (int)now.DayOfWeek;     // .NET: Sunday=0..Saturday=6 — same as the bin
            var current = FindForDay(now, todayDow);
            EventHotTimeRecord? nextEntry = null;
            for (int i = 1; i <= 7; i++)
            {
                var future = now.AddDays(i);
                var hit = FindForDay(future, (int)future.DayOfWeek);
                if (hit != null) { nextEntry = hit; break; }
            }

            if (current == null)
                return new Snapshot(State.NoEvent, null, nextEntry, 0, 0);

            var todaysStart = now.Date + current.StartTime;
            var todaysEnd = now.Date + current.EndTime;
            int startLeft = (int)Math.Max(0, (todaysStart - now).TotalSeconds);
            int endLeft = (int)Math.Max(0, (todaysEnd - now).TotalSeconds);
            var state = (now >= todaysStart && now <= todaysEnd) ? State.InTime : State.NotInTime;

            return new Snapshot(state, current, nextEntry, startLeft, endLeft);
        }

        /// <summary>
        /// True iff <paramref name="now"/> is INSIDE the time-of-day claim window for
        /// the given hot-time entry — used as the server-side gate before granting items.
        /// </summary>
        public bool IsClaimable(EventHotTimeRecord entry, DateTime now)
        {
            if (now < entry.StartDate || now > entry.EndDate) return false;
            if ((int)now.DayOfWeek != entry.DayOfWeek) return false;
            var todStart = now.Date + entry.StartTime;
            var todEnd = now.Date + entry.EndTime;
            return now >= todStart && now <= todEnd;
        }

        public EventHotTimeRecord? FindByEventNo(int eventNo)
        {
            foreach (var e in _eventTable.Data.HotTime)
                if (e.EventNo == eventNo) return e;
            return null;
        }

        /// <summary>True iff <paramref name="characterId"/> has already claimed
        /// <paramref name="eventNo"/> on the UTC date of <paramref name="now"/>.</summary>
        public bool HasClaimed(long characterId, int eventNo, DateTime now)
        {
            return _lastClaim.TryGetValue((characterId, eventNo), out var d)
                && d == DateOnly.FromDateTime(now.ToUniversalTime());
        }

        /// <summary>Records a successful claim. Subsequent <see cref="HasClaimed"/> calls
        /// for the same UTC date return true.</summary>
        public void RecordClaim(long characterId, int eventNo, DateTime now)
        {
            _lastClaim[(characterId, eventNo)] = DateOnly.FromDateTime(now.ToUniversalTime());
        }

        private EventHotTimeRecord? FindForDay(DateTime when, int dayOfWeek)
        {
            foreach (var entry in _eventTable.Data.HotTime)
            {
                if (entry.DayOfWeek != dayOfWeek) continue;
                if (when < entry.StartDate || when > entry.EndDate) continue;
                return entry;
            }
            return null;
        }
    }
}
