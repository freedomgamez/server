using DigitalWorldOnline.Commons.Models;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    /// <summary>
    /// Persists the player's daily play-time event progress (<c>RewardIndex</c> + the
    /// absolute timestamp <c>StartTime</c> when the current threshold fires) to
    /// <c>Event_TimeReward</c>. Issued by <c>DailyEventService.TickAsync</c> when a
    /// threshold fires and the index advances.
    /// </summary>
    public class UpdateTamerTimeRewardCommand : IRequest
    {
        public TimeReward TimeReward { get; set; }

        public UpdateTamerTimeRewardCommand(TimeReward timeReward)
        {
            TimeReward = timeReward;
        }
    }
}
