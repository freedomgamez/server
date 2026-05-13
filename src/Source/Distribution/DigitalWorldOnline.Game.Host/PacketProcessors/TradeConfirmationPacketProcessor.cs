using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Extensions;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class TradeConfirmationPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.TradeConfirmation;

        private readonly MapServer _mapServer;
        private readonly ILogger _logger;
        private readonly ISender _sender;

        public TradeConfirmationPacketProcessor(
            MapServer mapServer,
            ILogger logger,
            ISender sender)
        {
            _mapServer = mapServer;
            _logger = logger;
            _sender = sender;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var targetClient = _mapServer.FindClientByTamerHandle(client.Tamer.TargetTradeGeneralHandle);
            if (targetClient == null)
                return;


            client.Send(new TradeConfirmationPacket(client.Tamer.GeneralHandler));
            targetClient.Send(new TradeConfirmationPacket(client.Tamer.GeneralHandler));
            client.Tamer.SetTradeConfirm(true);

            if (client.Tamer.TradeConfirm && targetClient.Tamer.TradeConfirm)
            {
                if (!CanCommitTrade(client, targetClient))
                {
                    InvalidTrade(client, targetClient);
                    return;
                }

                var firstTamerItems = client.Tamer.TradeInventory.EquippedItems.Select(x => $"{x.ItemId} x{x.Amount}");
                var secondTamerItems = targetClient.Tamer.TradeInventory.EquippedItems.Select(x => $"{x.ItemId} x{x.Amount}");
                var firstTamerBits = client.Tamer.TradeReservedBits;
                var secondTamerBits = targetClient.Tamer.TradeReservedBits;
              
                _logger.Verbose($"Character {client.TamerId} traded {string.Join('|', firstTamerItems)} and {firstTamerBits} with {targetClient.TamerId}.");
                _logger.Verbose($"Character {targetClient.TamerId} traded {string.Join('|', secondTamerItems)} and {secondTamerBits} with {client.TamerId}.");
               
                if (!CommitTradeItems(client, targetClient))
                {
                    InvalidTrade(client, targetClient);
                    return;
                }

                targetClient.Tamer.Inventory.AddBits(client.Tamer.TradeReservedBits);
                client.Tamer.Inventory.RemoveBits(client.Tamer.TradeReservedBits);
                client.Tamer.Inventory.AddBits(targetClient.Tamer.TradeReservedBits);
                targetClient.Tamer.Inventory.RemoveBits(targetClient.Tamer.TradeReservedBits);

                await _sender.Send(new UpdateItemsCommand(targetClient.Tamer.Inventory));
                await _sender.Send(new UpdateItemListBitsCommand(targetClient.Tamer.Inventory));

                await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));

                targetClient.Tamer.ClearTrade();
                client.Tamer.ClearTrade();

                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                targetClient.Send(new LoadInventoryPacket(targetClient.Tamer.Inventory, InventoryTypeEnum.Inventory));

                client.Send(new TradeFinalConfirmationPacket(client.Tamer.GeneralHandler));
                targetClient.Send(new TradeFinalConfirmationPacket(client.Tamer.GeneralHandler));
            }
        }

        private static void InvalidTrade(GameClient client, GameClient? targetClient)
        {
            client.Send(new TradeCancelPacket(targetClient.Tamer.GeneralHandler));
            client.Send(new PickItemFailPacket(PickItemFailReasonEnum.InventoryFull));
            targetClient.Send(new TradeCancelPacket(targetClient.Tamer.GeneralHandler));
            client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
            targetClient.Send(new LoadInventoryPacket(targetClient.Tamer.Inventory, InventoryTypeEnum.Inventory));

            targetClient.Tamer.ClearTrade();
            client.Tamer.ClearTrade();
        }

        private static bool CanCommitTrade(GameClient firstClient, GameClient secondClient)
        {
            return CanReceiveAllTradeItems(firstClient.Tamer, secondClient.Tamer) &&
                   CanReceiveAllTradeItems(secondClient.Tamer, firstClient.Tamer);
        }

        private static bool CanReceiveAllTradeItems(Commons.Models.Character.CharacterModel receiver, Commons.Models.Character.CharacterModel sender)
        {
            var groupedItems = sender.TradeInventory.EquippedItems
                .GroupBy(item => item.ItemId)
                .ToDictionary(group => group.Key, group => group.Sum(value => value.Amount));

            foreach (var groupedItem in groupedItems)
            {
                var incomingAmount = groupedItem.Value;
                var stackedAmount = receiver.Inventory.FindItemsById(groupedItem.Key)
                    .Where(item => item.ItemInfo != null && item.ItemInfo.Overlap > item.Amount)
                    .Sum(item => item.GetMaxIncreaseCapacity());

                incomingAmount -= stackedAmount;
                if (incomingAmount <= 0)
                    continue;

                var overlap = receiver.Inventory.FindItemById(groupedItem.Key, true)?.ItemInfo?.Overlap ?? (short)1;
                var emptySlotsNeeded = (int)Math.Ceiling(incomingAmount / (double)overlap);
                if (receiver.Inventory.TotalEmptySlots < emptySlotsNeeded)
                    return false;
            }

            return true;
        }

        private static bool CommitTradeItems(GameClient firstClient, GameClient secondClient)
        {
            var firstToSecond = BuildTradeTransfers(firstClient, secondClient);
            if (firstToSecond == null)
                return false;

            var secondToFirst = BuildTradeTransfers(secondClient, firstClient);
            if (secondToFirst == null)
                return false;

            if (!CanApplyTransfers(firstToSecond))
                return false;

            if (!CanApplyTransfers(secondToFirst))
                return false;

            return ApplyTransfers(firstToSecond) && ApplyTransfers(secondToFirst);
        }

        private static List<TradeTransfer>? BuildTradeTransfers(GameClient sourceClient, GameClient destinationClient)
        {
            var transfers = new List<TradeTransfer>();
            foreach (var tradeItem in sourceClient.Tamer.TradeInventory.EquippedItems.OrderBy(item => item.TradeSlot))
            {
                if (!sourceClient.Tamer.TryGetTradeSlotSource(tradeItem.TradeSlot, out var sourceSlot))
                    return null;

                var sourceItem = sourceClient.Tamer.Inventory.FindItemBySlot(sourceSlot);
                if (sourceItem == null || sourceItem.ItemId != tradeItem.ItemId || sourceItem.Amount < tradeItem.Amount)
                    return null;

                transfers.Add(new TradeTransfer(sourceClient, destinationClient, sourceSlot, tradeItem));
            }

            return transfers;
        }

        private static bool CanApplyTransfers(List<TradeTransfer> transfers)
        {
            var capacityByItem = new Dictionary<int, int>();
            var receiver = transfers.FirstOrDefault()?.DestinationClient.Tamer.Inventory;
            if (receiver == null)
                return true;

            foreach (var item in receiver.Items.Where(x => x.ItemId > 0 && x.Amount > 0))
            {
                var remaining = item.GetMaxIncreaseCapacity();
                if (remaining <= 0)
                    continue;

                if (!capacityByItem.TryAdd(item.ItemId, remaining))
                    capacityByItem[item.ItemId] += remaining;
            }

            var emptySlots = (int)receiver.TotalEmptySlots;
            foreach (var transfer in transfers)
            {
                var requiredAmount = transfer.Amount;
                if (capacityByItem.TryGetValue(transfer.ItemId, out var stackCapacity) && stackCapacity > 0)
                {
                    var absorbed = Math.Min(requiredAmount, stackCapacity);
                    requiredAmount -= absorbed;
                    capacityByItem[transfer.ItemId] = stackCapacity - absorbed;
                }

                if (requiredAmount <= 0)
                    continue;

                var overlap = transfer.Overlap > 0 ? transfer.Overlap : 1;
                var slotsNeeded = (int)Math.Ceiling(requiredAmount / (double)overlap);
                if (slotsNeeded > emptySlots)
                    return false;

                emptySlots -= slotsNeeded;
            }

            return true;
        }

        private static bool ApplyTransfers(List<TradeTransfer> transfers)
        {
            foreach (var transfer in transfers)
            {
                var sourceItem = transfer.SourceClient.Tamer.Inventory.FindItemBySlot(transfer.SourceSlot);
                if (sourceItem == null || sourceItem.ItemId != transfer.ItemId || sourceItem.Amount < transfer.Amount)
                    return false;

                var accepted = transfer.DestinationClient.Tamer.Inventory.AddItem(new ItemModel(transfer.ItemId, transfer.Amount)
                {
                    ItemInfo = transfer.Template.ItemInfo,
                    Power = transfer.Template.Power,
                    RerollLeft = transfer.Template.RerollLeft,
                    FamilyType = transfer.Template.FamilyType,
                    Duration = transfer.Template.Duration,
                    EndDate = transfer.Template.EndDate,
                    FirstExpired = transfer.Template.FirstExpired
                });

                if (!accepted)
                    return false;

                if (!transfer.SourceClient.Tamer.Inventory.RemoveOrReduceItem(sourceItem, transfer.Amount, transfer.SourceSlot))
                    return false;
            }

            return true;
        }

        private sealed record TradeTransfer(
            GameClient SourceClient,
            GameClient DestinationClient,
            int SourceSlot,
            ItemModel Template)
        {
            public int ItemId => Template.ItemId;
            public int Amount => Template.Amount;
            public int Overlap => Template.ItemInfo?.Overlap ?? 1;
        }
    }

}
