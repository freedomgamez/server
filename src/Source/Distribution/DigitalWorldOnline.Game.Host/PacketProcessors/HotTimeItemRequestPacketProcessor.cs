using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.Game.Managers;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles the v487 client's <c>pEvent::HotTimeItemRequest</c> (3135) — player clicked
    /// the "Claim Item" button on the Hot Time UI panel. Wire format from the client at
    /// <c>cCliGameEvent.cpp:403-411</c> (<c>C2GS_SEND_HOTTIME_EVENT_GET</c>):
    /// just <c>n4 nTableIdx</c> — the <c>EventNo</c> the player wants to claim.
    /// <para>
    /// Server validates: bin entry exists, in-time gate via <c>HotTimeService.IsClaimable</c>,
    /// not already claimed today by this character. On success, grants the entry's
    /// <c>ItemId × ItemCount</c> to the cash warehouse (mirrors the attendance-reward grant
    /// pattern at <c>MapServerTamerOperation.cs:1032</c>) and replies SUCCESS. On failure,
    /// replies with the matching <c>nsHotTimeResult</c> message id so the client toasts
    /// "already claimed" / "not in time".
    /// </para>
    /// </summary>
    public class HotTimeItemRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.HotTimeItemRequest;

        private readonly HotTimeService _hotTime;
        private readonly AssetsLoader _assets;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public HotTimeItemRequestPacketProcessor(
            HotTimeService hotTime,
            AssetsLoader assets,
            ISender sender,
            ILogger logger)
        {
            _hotTime = hotTime;
            _assets = assets;
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            int tableIdx = packet.ReadInt();
            var now = DateTime.UtcNow;

            var entry = _hotTime.FindByEventNo(tableIdx);
            if (entry == null)
            {
                _logger.Warning(
                    "HotTime claim from tamer {TamerId} for unknown table idx {Idx} — Event.bin §5 has no such EventNo.",
                    client.TamerId, tableIdx);
                client.Send(new HotTimeItemGetResultPacket(HotTimeItemGetResultPacket.ResultNotInTime));
                return;
            }

            if (!_hotTime.IsClaimable(entry, now))
            {
                client.Send(new HotTimeItemGetResultPacket(HotTimeItemGetResultPacket.ResultNotInTime));
                return;
            }

            if (_hotTime.HasClaimed(client.TamerId, tableIdx, now))
            {
                client.Send(new HotTimeItemGetResultPacket(HotTimeItemGetResultPacket.ResultAlreadyClaimed));
                return;
            }

            var itemInfo = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == entry.ItemId);
            if (itemInfo == null)
            {
                _logger.Warning(
                    "HotTime entry {Idx} references unknown item {ItemId} — granting nothing, replying NOT_TIME.",
                    tableIdx, entry.ItemId);
                client.Send(new HotTimeItemGetResultPacket(HotTimeItemGetResultPacket.ResultNotInTime));
                return;
            }

            var newItem = new ItemModel();
            newItem.SetItemInfo(itemInfo);
            newItem.ItemId = entry.ItemId;
            newItem.Amount = entry.ItemCount;
            if (newItem.IsTemporary)
                newItem.SetRemainingTime((uint)newItem.ItemInfo.UsageTimeMinutes);

            if (!client.Tamer.AccountCashWarehouse.AddItem(newItem))
            {
                _logger.Warning(
                    "HotTime claim from tamer {TamerId} for entry {Idx} failed — cash warehouse full.",
                    client.TamerId, tableIdx);
                client.Send(new HotTimeItemGetResultPacket(HotTimeItemGetResultPacket.ResultNotInTime));
                return;
            }

            _hotTime.RecordClaim(client.TamerId, tableIdx, now);
            await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountCashWarehouse));

            _logger.Verbose(
                "HotTime claim succeeded — tamer {TamerId} table {Idx} item {ItemId} × {Count}.",
                client.TamerId, tableIdx, entry.ItemId, entry.ItemCount);

            client.Send(new HotTimeItemGetResultPacket(HotTimeItemGetResultPacket.ResultSuccess));

            // Refresh panel so the "Claim" button greys out for the rest of today.
            var snapshot = _hotTime.GetSnapshot(now);
            client.Send(new HotTimeEventInfoPacket(
                (byte)snapshot.CurrentState,
                (byte)(snapshot.Current?.EventNo ?? 0),
                (byte)(snapshot.Next?.EventNo ?? 0),
                _hotTime.HasClaimed(client.TamerId, snapshot.Current?.EventNo ?? 0, now),
                snapshot.StartTimeLeftSec,
                snapshot.EndTimeLeftSec));
        }
    }
}
