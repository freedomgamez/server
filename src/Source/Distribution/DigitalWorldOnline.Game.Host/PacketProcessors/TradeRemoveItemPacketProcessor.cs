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
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class TraRemoveItemPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.TradeRemoveItem;

        private readonly MapServer _mapServer;
        private readonly ILogger _logger;


        public TraRemoveItemPacketProcessor(
            MapServer mapServer,
            ILogger logger)
        {
            _mapServer = mapServer;
            _logger = logger;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            var tradeSlot = packet.ReadShort();


            var targetClient = _mapServer.FindClientByTamerHandle(client.Tamer.TargetTradeGeneralHandle);
            if (targetClient == null)
            {
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                return;
            }



            var Item = client.Tamer.TradeInventory.FindItemByTradeSlot(tradeSlot);

            if (Item != null)
            {
                if (client.Tamer.TryGetTradeSlotSource(tradeSlot, out var sourceSlot))
                {
                    client.Tamer.RemoveReservedTradeItem(sourceSlot, Item.Amount);
                    client.Tamer.RemoveTradeSlotSource(tradeSlot);
                }

                client.Tamer.TradeInventory.RemoveOrReduceItem(Item, Item.Amount);
                var remappedSources = new Dictionary<int, int>();
                var nextTradeSlot = 0;
                foreach (var tradeItem in client.Tamer.TradeInventory.EquippedItems.OrderBy(x => x.Slot))
                {
                    if (client.Tamer.TryGetTradeSlotSource(tradeItem.TradeSlot, out var mappedSourceSlot))
                        remappedSources[nextTradeSlot] = mappedSourceSlot;

                    tradeItem.SetTradeSlot(nextTradeSlot);
                    nextTradeSlot++;
                }
                client.Tamer.ReplaceTradeSlotSourceMap(remappedSources);

                client.Send(new TradeRemoveItemPacket(client.Tamer.GeneralHandler, (byte)tradeSlot));
                targetClient.Send(new TradeRemoveItemPacket(client.Tamer.GeneralHandler, (byte)tradeSlot));
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                targetClient.Send(new LoadInventoryPacket(targetClient.Tamer.Inventory, InventoryTypeEnum.Inventory));
            }
            else
            {
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
            }

            //_logger.Verbose($"Character {client.TamerId} and {targetClient.TamerId} inventory unlock "); ;

        }


    }
}
