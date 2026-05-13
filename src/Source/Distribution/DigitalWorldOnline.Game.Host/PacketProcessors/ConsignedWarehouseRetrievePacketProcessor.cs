using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Extensions;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Packets.PersonalShop;

using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ConsignedWarehouseRetrievePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ConsignedWarehouseRetrieve;

        private readonly ILogger _logger;
        private readonly ISender _sender;

        public ConsignedWarehouseRetrievePacketProcessor(
            ILogger logger,
            ISender sender)
        {
            _logger = logger;
            _sender = sender;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var bits = client.Tamer.ConsignedWarehouse.Bits;

            _logger.Debug($"Updating consigned warehouse...");
            var sourceSlots = client.Tamer.ConsignedWarehouse.Items
                .Where(x => x.ItemId > 0 && x.Amount > 0)
                .OrderBy(x => x.Slot)
                .Select(x => x.Slot)
                .ToList();

            _logger.Debug($"Updating tamer inventory...");
            foreach (var sourceSlot in sourceSlots)
            {
                var sourceItem = client.Tamer.ConsignedWarehouse.FindItemBySlot(sourceSlot);
                if (sourceItem == null || sourceItem.ItemId <= 0 || sourceItem.Amount <= 0)
                    continue;

                var destinationSlot = client.Tamer.Inventory.FindAvailableSlot(sourceItem);
                if (destinationSlot < 0)
                    continue;

                client.Tamer.ConsignedWarehouse.TryMoveAcrossLists(client.Tamer.Inventory, sourceSlot, destinationSlot);
            }

            if (bits > 0 && client.Tamer.Inventory.AddBits(bits))
                client.Tamer.ConsignedWarehouse.RemoveBits(bits);

            _logger.Debug($"Sending load inventory packet...");
            client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));

            _logger.Debug($"Sending load consigned shop warehouse packet...");
            client.Send(new LoadConsignedShopWarehousePacket(client.Tamer.ConsignedWarehouse));

            _logger.Debug($"Sending consigned shop warehouse item retrieve packet...");
            client.Send(new ConsignedShopWarehouseItemRetrievePacket());

            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
            await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));
            await _sender.Send(new UpdateItemsCommand(client.Tamer.ConsignedWarehouse));
            await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.ConsignedWarehouse));
        }
    }
}
