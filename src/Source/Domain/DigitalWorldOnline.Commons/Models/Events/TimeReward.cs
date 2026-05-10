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
        /// The current index start time.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// The reward current index and duration.
        /// </summary>
        public TimeRewardIndexEnum RewardIndex { get; private set; }

        public TimeReward()
        {
            RewardIndex = TimeRewardIndexEnum.First;
            // StartTime is the absolute timestamp when this index's threshold elapses.
            // Pre-existing bug: was set to Now (RemainingTime would be ~0 immediately).
            // Fix: add the First-tier duration so the threshold fires after 30 min of play.
            StartTime = DateTime.Now.AddSeconds(TimeRewardDurationEnum.First.GetHashCode());
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