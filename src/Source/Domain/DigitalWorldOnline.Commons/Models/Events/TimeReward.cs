using DigitalWorldOnline.Commons.Enums;

namespace DigitalWorldOnline.Commons.Models
{
    public sealed partial class TimeReward
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Reference to the owner.
        /// </summary>
        public long CharacterId { get; private set; }

        /// <summary>
        /// The current index start time. Legacy field — was used as the absolute UTC
        /// timestamp the threshold would fire. Replaced by <see cref="RemainingSeconds"/>
        /// because absolute time kept counting down even while the player was offline.
        /// Kept for backward compat / DB column survival; no longer drives any logic.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Seconds remaining until the current threshold fires. Decremented in-memory
        /// each in-session tick (see <see cref="Tick"/>); persisted on advance. The
        /// pause-while-offline behavior comes from <see cref="LastTickTime"/> being
        /// reset to <c>Now</c> on the first tick of a new session — offline gap is
        /// not subtracted.
        /// </summary>
        public int RemainingSeconds { get; private set; }

        /// <summary>
        /// In-memory only — the timestamp of the last in-session tick that decremented
        /// <see cref="RemainingSeconds"/>. Defaults to <see cref="DateTime.MinValue"/>
        /// after model load; first <see cref="Tick"/> seeds it to <c>Now</c> without
        /// decrementing, so offline time doesn't roll the timer forward.
        /// </summary>
        public DateTime LastTickTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The reward current index and duration.
        /// </summary>
        public TimeRewardIndexEnum RewardIndex { get; private set; }

        public TimeReward()
        {
            RewardIndex = TimeRewardIndexEnum.First;
            RemainingSeconds = (int)TimeRewardDurationEnum.First;    // 1800 s = 30 min
            // StartTime kept for AutoMapper / DB schema; not used for fire-time logic.
            StartTime = DateTime.Now.AddSeconds((int)TimeRewardDurationEnum.First);
        }

        /// <summary>
        /// Offset within the daily-event group that the client uses as <c>m_nEventNo</c>.
        /// The client's <c>CsEventTable::GetMap(nType, nNO)</c> at <c>Event.cpp:981-984</c>
        /// keys <c>m_mapEvent</c> at <c>nType + nNO</c>, with <c>ET_DAILY = 10000</c> as the
        /// type. Bin records are stored at the full TableNo (10000, 10001, ...), so the
        /// server must send the OFFSET (0, 1, 2, ...) for the lookup to land on the right
        /// row. Sending the full TableNo here would make the client query
        /// <c>m_mapEvent[20000+]</c> and miss.
        /// </summary>
        public int CurrentEventNo => RewardIndex switch
        {
            TimeRewardIndexEnum.First => 0,
            TimeRewardIndexEnum.Second => 1,
            TimeRewardIndexEnum.Third => 2,
            TimeRewardIndexEnum.Fourth => 3,
            _ => -1,    // Ended → client closes the panel
        };

        /// <summary>Total seconds for the current threshold (v487 client's <c>m_nTotalTime</c>).</summary>
        public int CurrentTotalSeconds => RewardIndex switch
        {
            TimeRewardIndexEnum.First => (int)TimeRewardDurationEnum.First,
            TimeRewardIndexEnum.Second => (int)TimeRewardDurationEnum.Second,
            TimeRewardIndexEnum.Third => (int)TimeRewardDurationEnum.Third,
            TimeRewardIndexEnum.Fourth => (int)TimeRewardDurationEnum.Fourth,
            _ => 0,
        };
    }
}