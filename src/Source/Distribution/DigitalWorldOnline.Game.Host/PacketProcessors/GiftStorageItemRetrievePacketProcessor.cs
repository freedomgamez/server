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
    public class GiftStorageItemRetrievePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.GiftStorageItemRetrieve;

        private readonly ILogger _logger;
        private readonly ISender _sender;

        public GiftStorageItemRetrievePacketProcessor(
            ILogger logger,
            ISender sender)
        {
            _logger = logger;
            _sender = sender;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var withdrawType = packet.ReadShort();
            var itemSlot = packet.ReadShort();

            if (withdrawType == 1)
            {
                var targetItem = client.Tamer.GiftWarehouse.GiftFindItemBySlot(itemSlot);
                if (targetItem != null && targetItem.ItemId > 0 && targetItem.Amount > 0)
                {
                    var destinationSlot = client.Tamer.Inventory.FindAvailableSlot(targetItem);
                    if (destinationSlot >= 0 &&
                        client.Tamer.GiftWarehouse.TryMoveAcrossLists(client.Tamer.Inventory, itemSlot, destinationSlot))
                    {
                        client.Send(new LoadGiftStoragePacket(client.Tamer.GiftWarehouse));
                        client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.GiftWarehouse));
                    }
                }

            }
            else
            {
                var sourceSlots = client.Tamer.GiftWarehouse.Items
                    .Where(x => x.ItemId > 0 && x.Amount > 0)
                    .OrderBy(x => x.Slot)
                    .Select(x => x.Slot)
                    .ToList();

                foreach (var sourceSlot in sourceSlots)
                {
                    var sourceItem = client.Tamer.GiftWarehouse.GiftFindItemBySlot(sourceSlot);
                    if (sourceItem != null && sourceItem.ItemId > 0 && sourceItem.Amount > 0)
                    {
                        var destinationSlot = client.Tamer.Inventory.FindAvailableSlot(sourceItem);
                        if (destinationSlot >= 0)
                            client.Tamer.GiftWarehouse.TryMoveAcrossLists(client.Tamer.Inventory, sourceSlot, destinationSlot);
                    }
                }

                client.Send(new LoadGiftStoragePacket(client.Tamer.GiftWarehouse));
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));

                await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                await _sender.Send(new UpdateItemsCommand(client.Tamer.GiftWarehouse));
            }


        }
    }
}
