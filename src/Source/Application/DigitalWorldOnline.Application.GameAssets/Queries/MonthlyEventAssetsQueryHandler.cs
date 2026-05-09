using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Event.bin</c> §4. Each <c>EventMonthlyRecord</c> in the bin carries a
    /// 28-element reward array indexed by streak day; this handler flattens it into 28
    /// <c>MonthlyEventAssetDTO</c> rows (one per day) so the existing consumer shape
    /// (<c>MapServerTamerOperation</c> / <c>DungeonsServerTamerOperation</c> filtering by
    /// <c>CurrentDay == AttendanceReward.TotalDays</c>) keeps working unchanged. Empty
    /// reward slots (<c>ItemId == 0</c>) are dropped to keep the DTO list compact.
    /// </summary>
    public class MonthlyEventAssetsQueryHandler : IRequestHandler<MonthlyEventAssetsQuery, List<MonthlyEventAssetDTO>>
    {
        private readonly EventTableBinLoader _loader;

        public MonthlyEventAssetsQueryHandler(EventTableBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<MonthlyEventAssetDTO>> Handle(MonthlyEventAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<MonthlyEventAssetDTO>();
            foreach (var monthly in _loader.Data.Monthly)
            {
                for (int i = 0; i < monthly.DailyRewards.Count; i++)
                {
                    var reward = monthly.DailyRewards[i];
                    if (reward.ItemId == 0) continue;
                    list.Add(new MonthlyEventAssetDTO
                    {
                        CurrentDay = i + 1,
                        ItemId = reward.ItemId,
                        ItemCount = reward.Count
                    });
                }
            }
            return Task.FromResult(list);
        }
    }
}
