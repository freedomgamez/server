using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Game.Managers;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles the v487 client's <c>pEvent::Attendance</c> (3107) — player clicked the
    /// "claim attendance" button on the BG sprite (<c>BGSprite.cpp:696</c>). No payload.
    /// <para>
    /// Validates against (a) <c>AttendanceService</c>'s configured window, (b)
    /// <c>AttendanceReward.LastRewardDate</c> already-claimed-today gate. On success,
    /// advances <c>TotalDays</c>, picks the bin's monthly reward for the new day from
    /// <c>_assets.MonthlyEvents</c> (Event.bin §4 via the C5 swap), grants to
    /// <c>GiftWarehouse</c> (event mail per the user's preference), persists, and replies
    /// with <c>nResCode = 100</c>.
    /// </para>
    /// </summary>
    public class AttendanceRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.AttendanceRequest;

        private readonly AttendanceService _attendance;
        private readonly AssetsLoader _assets;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public AttendanceRequestPacketProcessor(
            AttendanceService attendance,
            AssetsLoader assets,
            ISender sender,
            ILogger logger)
        {
            _attendance = attendance;
            _assets = assets;
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var now = DateTime.UtcNow;
            var reward = client.Tamer.AttendanceReward;

            if (!_attendance.IsActive(now))
            {
                _logger.Debug(
                    "Attendance claim from tamer {TamerId} rejected — outside configured window {Start} .. {End}.",
                    client.TamerId, _attendance.ConfiguredStart, _attendance.ConfiguredEnd);
                client.Send(new AttendanceResponsePacket(AttendanceResponsePacket.ResultNotStarted));
                return;
            }

            if (reward.LastRewardDate.Date >= now.Date)
            {
                client.Send(new AttendanceResponsePacket(AttendanceResponsePacket.ResultAlreadyClaimedToday));
                return;
            }

            // Advance streak — fold this in BEFORE looking up the bin so CurrentDay maps
            // to the day being claimed (1-indexed in the bin).
            reward.IncreaseTotalDays();
            reward.SetLastRewardDate();

            var rewardInfo = _assets.MonthlyEvents.FirstOrDefault(x => x.CurrentDay == reward.TotalDays);
            uint giveItemNo = 32; // sentinel "no item granted" per cCliGameEvent.cpp:62

            if (rewardInfo != null && rewardInfo.ItemId > 0)
            {
                var info = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == rewardInfo.ItemId);
                if (info == null)
                {
                    _logger.Warning(
                        "Attendance day {Day} for tamer {TamerId} references unknown item {ItemId}; skipping grant.",
                        reward.TotalDays, client.TamerId, rewardInfo.ItemId);
                }
                else
                {
                    var item = new ItemModel();
                    item.SetItemInfo(info);
                    item.ItemId = rewardInfo.ItemId;
                    item.Amount = rewardInfo.ItemCount;
                    // Gift-box claim window — see DailyEventService for the full rationale.
                    // 14 days from grant; ItemModelBehavior.GiftToArray reads EndDate directly.
                    item.EndDate = DateTime.UtcNow.AddDays(14);
                    if (item.IsTemporary)
                        item.SetRemainingTime((uint)item.ItemInfo.UsageTimeMinutes);

                    if (client.Tamer.GiftWarehouse.AddGiftItem(item))
                    {
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.GiftWarehouse));
                        // Bin index is 0-based on the wire; TotalDays just incremented to N
                        // means the just-claimed day is row index (N - 1).
                        giveItemNo = (uint)(reward.TotalDays - 1);
                        _logger.Verbose(
                            "Attendance day {Day} for tamer {TamerId} → granted item {ItemId} × {Count} to GiftWarehouse.",
                            reward.TotalDays, client.TamerId, rewardInfo.ItemId, rewardInfo.ItemCount);
                    }
                    else
                    {
                        _logger.Warning(
                            "Attendance day {Day} for tamer {TamerId} grant failed — GiftWarehouse full.",
                            reward.TotalDays, client.TamerId);
                    }
                }
            }

            await _sender.Send(new UpdateTamerAttendanceRewardCommand(reward));

            client.Send(new AttendanceResponsePacket(
                giveItemNo,
                AttendanceService.DerivedWorkDayHistory(reward.TotalDays)));

            // Refresh the panel-info packet so the day count updates client-side.
            client.Send(new TamerAttendancePacket(reward));
        }
    }
}
