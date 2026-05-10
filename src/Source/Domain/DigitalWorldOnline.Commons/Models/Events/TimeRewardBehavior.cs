using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Utils;

namespace DigitalWorldOnline.Commons.Models
{
    public sealed partial class TimeReward
    {
        /// <summary>
        /// Seconds left until the current threshold fires. Reads from
        /// <see cref="RemainingSeconds"/>, which is decremented per tick while the
        /// player is in-session and frozen while offline. Negative for
        /// <see cref="TimeRewardIndexEnum.Ended"/>.
        /// </summary>
        public int RemainingTime
        {
            get
            {
                switch (RewardIndex)
                {
                    default: return -1;
                    case TimeRewardIndexEnum.First:
                    case TimeRewardIndexEnum.Second:
                    case TimeRewardIndexEnum.Third:
                    case TimeRewardIndexEnum.Fourth:
                        return Math.Max(0, RemainingSeconds);
                }
            }
        }

        /// <summary>
        /// Decrements <see cref="RemainingSeconds"/> by the in-session time elapsed
        /// since the last tick. First call after model load (when
        /// <see cref="LastTickTime"/> is <see cref="DateTime.MinValue"/>) just seeds
        /// the timestamp without subtracting — that's how the timer pauses across the
        /// offline gap. Returns true when the threshold has fired this tick.
        /// </summary>
        public bool Tick(DateTime now)
        {
            if (RewardIndex == TimeRewardIndexEnum.Ended) return false;

            if (LastTickTime == DateTime.MinValue)
            {
                LastTickTime = now;
                return false;
            }

            var deltaSec = (int)(now - LastTickTime).TotalSeconds;
            if (deltaSec <= 0) return false;

            LastTickTime = now;
            RemainingSeconds = Math.Max(0, RemainingSeconds - deltaSec);
            return RemainingSeconds == 0;
        }

        public void UpdateRewardIndex()
        {
            switch (RewardIndex)
            {
                case TimeRewardIndexEnum.First:
                    RewardIndex = TimeRewardIndexEnum.Second;
                    RemainingSeconds = (int)TimeRewardDurationEnum.Second;
                    StartTime = DateTime.Now.AddSeconds(RemainingSeconds);
                    break;

                case TimeRewardIndexEnum.Second:
                    RewardIndex = TimeRewardIndexEnum.Third;
                    RemainingSeconds = (int)TimeRewardDurationEnum.Third;
                    StartTime = DateTime.Now.AddSeconds(RemainingSeconds);
                    break;

                case TimeRewardIndexEnum.Third:
                    RewardIndex = TimeRewardIndexEnum.Fourth;
                    RemainingSeconds = (int)TimeRewardDurationEnum.Fourth;
                    StartTime = DateTime.Now.AddSeconds(RemainingSeconds);
                    break;

                case TimeRewardIndexEnum.Fourth:
                    RewardIndex = TimeRewardIndexEnum.Ended;
                    RemainingSeconds = 0;
                    StartTime = DateTime.Now;
                    break;
            }
        }
    }
}