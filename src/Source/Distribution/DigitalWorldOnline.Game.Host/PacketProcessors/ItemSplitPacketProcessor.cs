using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemSplitPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.SplitItem;

        private readonly ISender _sender;
        private readonly ILogger _logger;

        public ItemSplitPacketProcessor(
            ISender sender,
            ILogger logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var originSlot = packet.ReadShort();
            var destinationSlot = packet.ReadShort();
            var amountToSplit = packet.ReadShort();

            var itemListMovimentation = UtilitiesFunctions.SwitchItemList(originSlot, destinationSlot);

            _logger.Verbose($"Character {client.TamerId} splited {itemListMovimentation} from slot {originSlot} to {destinationSlot} x{amountToSplit}.");

            switch (itemListMovimentation)
            {
                case ItemListMovimentationEnum.InventoryToInventory:
                    {
                        var success = client.Tamer.Inventory.TrySplitAcrossLists(client.Tamer.Inventory, originSlot, destinationSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                    }
                    break;

                case ItemListMovimentationEnum.InventoryToWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();

                        var success = client.Tamer.Inventory.TrySplitAcrossLists(client.Tamer.Warehouse, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                    }
                    break;

                case ItemListMovimentationEnum.InventoryToAccountWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();

                        var success = client.Tamer.Inventory.TrySplitAcrossLists(client.Tamer.AccountWarehouse, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                    }
                    break;

                case ItemListMovimentationEnum.WarehouseToWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();

                        var success = client.Tamer.Warehouse.TrySplitAcrossLists(client.Tamer.Warehouse, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                    }
                    break;

                case ItemListMovimentationEnum.WarehouseToInventory:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();

                        var success = client.Tamer.Warehouse.TrySplitAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                    }
                    break;

                case ItemListMovimentationEnum.WarehouseToAccountWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();

                        var success = client.Tamer.Warehouse.TrySplitAcrossLists(client.Tamer.AccountWarehouse, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                    }
                    break;

                case ItemListMovimentationEnum.AccountWarehouseToAccountWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();

                        var success = client.Tamer.AccountWarehouse.TrySplitAcrossLists(client.Tamer.AccountWarehouse, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                    }
                    break;

                case ItemListMovimentationEnum.AccountWarehouseToInventory:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();

                        var success = client.Tamer.AccountWarehouse.TrySplitAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                    }
                    break;

                case ItemListMovimentationEnum.AccountWarehouseToWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        var success = client.Tamer.AccountWarehouse.TrySplitAcrossLists(client.Tamer.Warehouse, srcSlot, dstSlot, amountToSplit);
                        client.Send(new SplitItemPacket(originSlot, destinationSlot, success ? amountToSplit : (short)0));

                        await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                    }
                    break;
            }

            SendItemListRefresh(client, itemListMovimentation);

            //_logger.Debug($"Concatting tamer {client.TamerId} items information...");
            //foreach (var item in client.Tamer.ItemList.SelectMany(x => x.Items).Where(x => x.ItemId > 0))
            //    item.SetItemInfo(_assets.ItemInfo.FirstOrDefault(x => x.ItemId == item?.ItemId));
        }

        private static void SendItemListRefresh(GameClient client, ItemListMovimentationEnum movimentation)
        {
            var packets = new List<byte[]>
            {
                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
            };

            if (movimentation is ItemListMovimentationEnum.InventoryToWarehouse or
                ItemListMovimentationEnum.WarehouseToWarehouse or
                ItemListMovimentationEnum.WarehouseToInventory or
                ItemListMovimentationEnum.WarehouseToAccountWarehouse or
                ItemListMovimentationEnum.AccountWarehouseToWarehouse)
            {
                packets.Add(new LoadInventoryPacket(client.Tamer.Warehouse, InventoryTypeEnum.Warehouse).Serialize());
            }

            if (movimentation is ItemListMovimentationEnum.InventoryToAccountWarehouse or
                ItemListMovimentationEnum.WarehouseToAccountWarehouse or
                ItemListMovimentationEnum.AccountWarehouseToAccountWarehouse or
                ItemListMovimentationEnum.AccountWarehouseToInventory or
                ItemListMovimentationEnum.AccountWarehouseToWarehouse)
            {
                packets.Add(new LoadInventoryPacket(client.Tamer.AccountWarehouse, InventoryTypeEnum.AccountWarehouse).Serialize());
            }

            client.Send(UtilitiesFunctions.GroupPackets(packets.ToArray()));
        }
    }
}
