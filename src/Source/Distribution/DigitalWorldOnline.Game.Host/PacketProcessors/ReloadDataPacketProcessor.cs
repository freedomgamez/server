using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.Items;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ReloadDataPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ReloadData;

        private readonly ILogger _logger;

        public ReloadDataPacketProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task Process(GameClient client, byte[] packetData)
        {
            client.Tamer.Inventory.CheckEmptyItems();
            client.Tamer.Warehouse.CheckEmptyItems();
            client.Tamer.AccountWarehouse.CheckEmptyItems();

            client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory));
            client.Send(new LoadInventoryPacket(client.Tamer.Warehouse, InventoryTypeEnum.Warehouse));
            client.Send(new LoadInventoryPacket(client.Tamer.AccountWarehouse, InventoryTypeEnum.AccountWarehouse));

            _logger.Debug("Processed ReloadData for tamer {TamerId}.", client.TamerId);
            return Task.CompletedTask;
        }
    }
}
