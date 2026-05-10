using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.Managers
{
    /// <summary>
    /// Drives the v487 client's daily play-time event panel (<c>pEvent::DailyEventInfo</c>,
    /// 3106). Backed by <c>Event.bin</c> §2 records and the existing <c>TimeReward</c>
    /// per-character scaffolding.
    /// <para>
    /// v487 design: there's no client-side claim button. The server tracks elapsed
    /// play-time per character and auto-grants the bin's <c>EventDailyRecord.Rewards</c>
    /// to the player's <c>GiftWarehouse</c> (event mail) when each threshold fires, then
    /// advances to the next index. The client just reflects the server-pushed panel state;
    /// the player picks up rewards from the gift box at their leisure.
    /// </para>
    /// <para>
    /// Per-character state is persisted via <c>UpdateTamerTimeRewardCommand</c> on the
    /// existing <c>Event_TimeReward</c> table; the model's <c>StartTime</c> stores the
    /// absolute UTC timestamp when the current threshold fires, and
    /// <c>RewardIndex</c> tracks which threshold (First/Second/Third/Fourth/Ended).
    /// </para>
    /// </summary>
    public sealed class DailyEventService
    {
        private readonly EventTableBinLoader _eventTable;
        private readonly AssetsLoader _assets;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public DailyEventService(
            EventTableBinLoader eventTable,
            AssetsLoader assets,
            ISender sender,
            ILogger logger)
        {
            _eventTable = eventTable;
            _assets = assets;
            _sender = sender;
            _logger = logger;
        }

        /// <summary>
        /// Resolves an <c>Event.bin</c> §2 record by the offset the client expects
        /// (<c>TimeReward.CurrentEventNo</c>, 0..N-1). Bin records are stored in TableNo
        /// order (10000, 10001, ...), and the client reverses the same arithmetic to look
        /// them up via <c>GetMap(ET_DAILY, eventNo)</c>. Returns null when the offset is
        /// out of range — caller treats as "no more rewards today".
        /// </summary>
        public EventDailyRecord? FindByOffset(int offset)
        {
            if (offset < 0 || offset >= _eventTable.Data.Daily.Count) return null;
            return _eventTable.Data.Daily[offset];
        }

        /// <summary>
        /// Per-tamer tick called from the map-server's per-character loop. If the current
        /// threshold has elapsed, grant the bin's reward set to <c>GiftWarehouse</c>,
        /// advance the index, persist, and broadcast the updated panel.
        /// </summary>
        public async Task TickAsync(GameClient client)
        {
            if (client.Tamer?.TimeReward == null) return;
            var reward = client.Tamer.TimeReward;
            if (reward.RewardIndex == TimeRewardIndexEnum.Ended) return;
            // Tick decrements RemainingSeconds by elapsed in-session time. Returns true
            // when the threshold has fired this tick. First tick after model load just
            // seeds LastTickTime — that's how the timer pauses while the player is offline
            // (offline gap is not subtracted).
            var now = DateTime.UtcNow;
            if (!reward.Tick(now)) return;

            // Threshold fired — capture current event before we advance.
            var current = FindByOffset(reward.CurrentEventNo);

            reward.UpdateRewardIndex();

            if (current != null)
            {
                int delivered = 0;
                foreach (var rwd in current.Rewards)
                {
                    if (rwd.ItemId == 0 || rwd.Count == 0) continue;
                    var info = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == rwd.ItemId);
                    if (info == null)
                    {
                        _logger.Warning(
                            "DailyEvent reward for tamer {TamerId} references unknown item {ItemId}; skipping.",
                            client.TamerId, rwd.ItemId);
                        continue;
                    }
                    var item = new ItemModel();
                    item.SetItemInfo(info);
                    item.ItemId = rwd.ItemId;
                    item.Amount = rwd.Count;
                    // Gift-box claim window — separate from in-game item duration; default
                    // 14 days from grant. Picked up by ItemModelBehavior.GiftToArray which
                    // derives the wire's "remaining minutes" directly from EndDate.
                    item.EndDate = DateTime.UtcNow.AddDays(14);
                    if (item.IsTemporary)
                        item.SetRemainingTime((uint)item.ItemInfo.UsageTimeMinutes);

                    if (client.Tamer.GiftWarehouse.AddGiftItem(item))
                        delivered++;
                    else
                        _logger.Warning(
                            "DailyEvent reward for tamer {TamerId} item {ItemId}×{Count} dropped — GiftWarehouse full.",
                            client.TamerId, rwd.ItemId, rwd.Count);
                }

                if (delivered > 0)
                {
                    await _sender.Send(new UpdateItemsCommand(client.Tamer.GiftWarehouse));
                    _logger.Verbose(
                        "DailyEvent {EventNo} fired for tamer {TamerId} → delivered {Count} reward stack(s) to GiftWarehouse.",
                        current.TableNo, client.TamerId, delivered);
                }
            }

            await _sender.Send(new UpdateTamerTimeRewardCommand(client.Tamer.TimeReward));
            client.Send(new TimeRewardPacket(client.Tamer.TimeReward));
        }
    }
}
