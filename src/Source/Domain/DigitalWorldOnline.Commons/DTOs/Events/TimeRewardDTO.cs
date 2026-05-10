using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.DTOs.Character;

namespace DigitalWorldOnline.Commons.DTOs.Events
{
    public class TimeRewardDTO
    {
        /// <summary>
        /// Unique sequential identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The reward current index and duration.
        /// </summary>
        public TimeRewardIndexEnum RewardIndex { get; set; }

        /// <summary>
        /// The current index start time. Legacy field — was used as the absolute UTC
        /// timestamp the threshold would fire. Replaced by <see cref="RemainingSeconds"/>
        /// because absolute time kept counting down even while the player was offline.
        /// Kept for backward compat / DB column survival; no longer drives any logic.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Seconds remaining until the current threshold fires. Decremented in-memory
        /// each in-session tick; persisted on advance. Pauses while the player is offline
        /// (resumes from the last persisted value on next login). Default 1800 = 30 min,
        /// matching the First-tier duration.
        /// </summary>
        public int RemainingSeconds { get; set; }

        /// <summary>
        /// Reference to the owner.
        /// </summary>
        public long CharacterId { get; set; }
        public CharacterDTO Character { get; set; }
    }
}
