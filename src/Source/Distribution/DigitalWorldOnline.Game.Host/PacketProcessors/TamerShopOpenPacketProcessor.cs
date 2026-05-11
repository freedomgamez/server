using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Extensions;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.PersonalShop;
using DigitalWorldOnline.GameHost;

using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class TamerShopOpenPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.TamerShopOpen;

        private readonly AssetsLoader _assets;
        private readonly DMBaseBinLoader _dmBase;
        private readonly MapServer _mapServer;
        private readonly ILogger _logger;
        private readonly ISender _sender;

        public TamerShopOpenPacketProcessor(
            AssetsLoader assets,
            DMBaseBinLoader dmBase,
            MapServer mapServer,
            ILogger logger,
            ISender sender)
        {
            _assets = assets;
            _dmBase = dmBase;
            _mapServer = mapServer;
            _logger = logger;
            _sender = sender;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            // DMBase.bin §8 StoreDist: reject opening a personal shop within StoreDist of another
            // open shop (matches v487 client `DataMng.cpp:3696` proximity check, but enforced
            // server-side so a manipulated client can't bypass it).
            var storeDist = _dmBase.Data.PersonStore.StoreDist;
            if (storeDist > 0)
            {
                var map = _mapServer.FindMapByTamer(client.TamerId);
                if (map != null)
                {
                    var myX = client.Tamer.Location.X;
                    var myY = client.Tamer.Location.Y;
                    foreach (var other in map.Clients)
                    {
                        if (other.TamerId == client.TamerId) continue;
                        var c = other.Tamer.CurrentCondition;
                        if ((c & ConditionEnum.TamerShop) == 0 && (c & ConditionEnum.PreparingShop) == 0) continue;

                        var dx = other.Tamer.Location.X - myX;
                        var dy = other.Tamer.Location.Y - myY;
                        var distSq = (long)dx * dx + (long)dy * dy;
                        if (distSq < (long)storeDist * storeDist)
                        {
                            _logger.Information(
                                "Tamer {TamerId} blocked from opening shop: {Other} within {Dist} units (StoreDist={StoreDist}).",
                                client.TamerId, other.TamerId, Math.Sqrt(distSq), storeDist);
                            client.Send(new SystemMessagePacket("There is another shop too close to open here."));
                            return;
                        }
                    }
                }
            }

            var packet = new GamePacketReader(packetData);

            _logger.Debug($"Getting parameters...");
            var shopName = packet.ReadString();
            packet.Skip(1);
            var sellQuantity = packet.ReadInt();

            List<ItemModel> sellList = new(sellQuantity);

            for (int i = 0; i < sellQuantity; i++)
            {
                var sellItem = new ItemModel(packet.ReadInt(), packet.ReadInt());

                packet.Skip(64);
                sellItem.SetSellPrice(packet.ReadInt());

                packet.Skip(12);
                sellList.Add(sellItem);
            }

            _logger.Debug($"{shopName} {sellQuantity}");

            foreach (var item in sellList)
            {
                item.SetItemInfo(_assets.ItemInfo.First(x => x.ItemId == item.ItemId));
                _logger.Debug($"{item.ItemId} {item.Amount} {item.TamerShopSellPrice}");
            }

            _logger.Debug($"Updating tamer shop item list...");
            client.Tamer.TamerShop.AddItems(sellList.Clone());
            await _sender.Send(new UpdateItemsCommand(client.Tamer.TamerShop));

            _logger.Debug($"Updating tamer inventory item list...");
            client.Tamer.Inventory.RemoveOrReduceItems(sellList.Clone());
            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));

            client.Tamer.UpdateCurrentCondition(ConditionEnum.TamerShop);
            client.Tamer.UpdateShopName(shopName);

            //_logger.Debug($"Sending sync in condition packet...");
            //_mapServer.BroadcastForViewsAndSelf(client.TamerId, new SyncConditionPacket(client.Tamer.GeneralHandler, client.Tamer.CurrentCondition, shopName));

            _logger.Debug($"Sending tamer shop open packet...");
            client.Send(new PersonalShopPacket(client.Tamer.ShopItemId));
        }
    }
}