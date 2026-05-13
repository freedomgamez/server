using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.GameHost;
using Serilog;
using System.Net.Sockets;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class TradeAddItemPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.TradeAddItem;

        private readonly MapServer _mapServer;
        private readonly ILogger _logger;


        public TradeAddItemPacketProcessor(
            MapServer mapServer,
            ILogger logger)
        {
            _mapServer = mapServer;
            _logger = logger;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            var inventorySlot = packet.ReadShort();
            var amount = packet.ReadShort();
            var targetClient = _mapServer.FindClientByTamerHandle(client.Tamer.TargetTradeGeneralHandle);
            var sourceItem = client.Tamer.Inventory.FindItemBySlot(inventorySlot);
            if (targetClient == null || sourceItem == null || sourceItem.ItemId == 0 || amount <= 0)
            {
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                return;
            }

            if (!client.Tamer.ReserveTradeItem(inventorySlot, amount))
            {
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                return;
            }

            var tradeSlot = client.Tamer.TradeInventory.GetEmptySlot;
            if (tradeSlot < 0)
            {
                client.Tamer.RemoveReservedTradeItem(inventorySlot, amount);
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                return;
            }

            var tradeItem = new ItemModel(sourceItem.ItemId, amount)
            {
                ItemInfo = sourceItem.ItemInfo,
                Power = sourceItem.Power,
                RerollLeft = sourceItem.RerollLeft,
                FamilyType = sourceItem.FamilyType,
                Duration = sourceItem.Duration,
                EndDate = sourceItem.EndDate,
                FirstExpired = sourceItem.FirstExpired
            };

            client.Tamer.TradeInventory.AddItemWithSlot(tradeItem, tradeSlot);
            client.Tamer.TradeInventory.FindItemBySlot(tradeSlot)?.SetTradeSlot(tradeSlot);
            client.Tamer.MapTradeSlotSource(tradeSlot, inventorySlot);


            targetClient.Send(new TradeInventoryUnlockPacket(client.Tamer.TargetTradeGeneralHandle));
            client.Send(new TradeInventoryUnlockPacket(client.Tamer.TargetTradeGeneralHandle));

            client.Send(new TradeAddItemPacket(client.Tamer.GeneralHandler, tradeItem.ToArray(), (byte)tradeSlot, inventorySlot));
            targetClient.Send(new TradeAddItemPacket(client.Tamer.GeneralHandler, tradeItem.ToArray(), (byte)tradeSlot, inventorySlot));
            client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
            targetClient.Send(new LoadInventoryPacket(targetClient.Tamer.Inventory, InventoryTypeEnum.Inventory));

            //_logger.Verbose($"Character {client.TamerId} and {targetClient.TamerId} inventory unlock "); ;

        }
    }
}
