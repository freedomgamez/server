using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.GameServer.Combat;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class AccountWarehouseItemRetrievePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.RetrivieAccountWarehouseItem;

        private readonly ILogger _logger;
        private readonly ISender _sender;

        public AccountWarehouseItemRetrievePacketProcessor(
            ILogger logger,
            ISender sender)
        {
            _logger = logger;
            _sender = sender;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var itemSlot = packet.ReadShort();
            var sourceList = client.Tamer.AccountCashWarehouse;
            var targetItem = sourceList.FindItemBySlot(itemSlot);

            if (targetItem != null && targetItem.ItemId > 0 && targetItem.Amount > 0)
            {
                var destinationSlot = client.Tamer.Inventory.FindAvailableSlot(targetItem);
                if (destinationSlot < 0)
                {
                    client.Send(new LoadAccountWarehousePacket(client.Tamer.AccountCashWarehouse));
                    client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                    return;
                }

                var canRetrieve = sourceList.TryMoveAcrossLists(client.Tamer.Inventory, itemSlot, destinationSlot);

                if (canRetrieve)
                {
                    sourceList.Sort();
                    var retrievedItem = client.Tamer.Inventory.FindItemBySlot(destinationSlot);
                    if (retrievedItem == null || retrievedItem.ItemId <= 0 || retrievedItem.Amount <= 0)
                        return;

                    client.Send(new AccountWarehouseItemRetrievePacket(retrievedItem, itemSlot));

                    client.Send(new LoadAccountWarehousePacket(client.Tamer.AccountCashWarehouse));
                    client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));

                    await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                    await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountCashWarehouse));
                }
                else
                {
                    client.Send(new LoadAccountWarehousePacket(client.Tamer.AccountCashWarehouse));
                    client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
                }
            }
            else
            {
                client.Send(new LoadAccountWarehousePacket(client.Tamer.AccountCashWarehouse));
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
            }

        }

    }
}
