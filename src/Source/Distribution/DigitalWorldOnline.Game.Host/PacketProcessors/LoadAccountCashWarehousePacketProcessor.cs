using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.GameServer.Combat;
using DigitalWorldOnline.Commons.Packets.Items;
using MediatR;
using Serilog;
using System.Linq;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class LoadAccountCashWarehousePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.LoadAccountWarehouse;

        private readonly ILogger _logger;

        public LoadAccountCashWarehousePacketProcessor(
            ILogger logger)
        {
            _logger = logger;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var enabled = client.Tamer.AccountCashWarehouse.Items.Where(x => x.ItemId > 0).ToList();
            var missingInfo = enabled.Where(x => x.ItemInfo == null).Take(10).ToList();
            if (missingInfo.Any())
            {
                var sample = string.Join(", ", missingInfo.Select(x => $"slot={x.Slot} item={x.ItemId} amt={x.Amount}"));
                _logger.Warning(
                    "LoadAccountCashWarehouse diagnostics: tamer {TamerId} has {MissingCount}/{EnabledCount} cash-warehouse items with null ItemInfo. Sample: {Sample}",
                    client.TamerId, enabled.Count(x => x.ItemInfo == null), enabled.Count, sample);
            }

            client.Send(new LoadAccountWarehousePacket(client.Tamer.AccountCashWarehouse));
            _logger.Debug($"Sending loadaccountcashwarehouse packet for character {client.TamerId}...");
        }
    }
}
