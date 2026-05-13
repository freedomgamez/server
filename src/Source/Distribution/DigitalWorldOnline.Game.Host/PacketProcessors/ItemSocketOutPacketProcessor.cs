using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;


namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemSocketOutPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ItemSocketOut;
        private readonly ILogger _logger;
        private readonly ISender _sender;
        private readonly AssetsLoader _assets;
        public ItemSocketOutPacketProcessor(
            ILogger logger,
            ISender sender,
            AssetsLoader assetsLoader)
        {
            _logger = logger;
            _sender = sender;
            _assets = assetsLoader;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            _ = packet.ReadInt();
            int remaining = (packet.Length - 2) - (int)packet.Packet.Position;
            short sourceSlot;
            byte orderSlot;

            if (remaining == 9)
            {
                _ = packet.ReadInt(); // npcId
                sourceSlot = packet.ReadShort();
                _ = packet.ReadShort(); // destinationSlot
                orderSlot = packet.ReadByte();
            }
            else if (remaining == 10)
            {
                _ = packet.ReadByte(); // vip
                _ = packet.ReadInt();  // npcId
                sourceSlot = packet.ReadShort();
                _ = packet.ReadShort(); // destinationSlot
                orderSlot = packet.ReadByte();
            }
            else if (remaining == 13)
            {
                _ = packet.ReadInt();  // portableSlot
                _ = packet.ReadInt();  // npcId
                sourceSlot = packet.ReadShort();
                _ = packet.ReadShort(); // destinationSlot
                orderSlot = packet.ReadByte();
            }
            else
            {
                _ = packet.ReadByte(); // vip
                _ = packet.ReadInt();  // portableSlot
                _ = packet.ReadInt();  // npcId
                sourceSlot = packet.ReadShort();
                _ = packet.ReadShort(); // destinationSlot
                orderSlot = packet.ReadByte();
            }

            var itemInfo = client.Tamer.Inventory.FindItemBySlot(sourceSlot);
         
            if (itemInfo != null)
            {
                var avaliableSocket = itemInfo.SocketStatus.FirstOrDefault(x => x.Slot == orderSlot);
              
                if (avaliableSocket != null)
                {
                    var newItem = new ItemModel();
                    newItem.SetItemInfo(_assets.ItemInfo.FirstOrDefault(x => x.ItemId == avaliableSocket.AttributeId));
                    if (newItem.ItemInfo == null)
                    {
                        client.Send(UtilitiesFunctions.GroupPackets(
                            new ItemSocketOutPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                            new SystemMessagePacket("Invalid socket attribute item.").Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));
                        return;
                    }



                    newItem.ItemId = avaliableSocket.AttributeId;
                    newItem.Amount = 1;

                    if (newItem.IsTemporary)
                        newItem.SetRemainingTime((uint)newItem.ItemInfo.UsageTimeMinutes);

                    newItem.SetPower(itemInfo.Power);
                    newItem.SetReroll(0);

                    var newAvaliableStatus = newItem.AccessoryStatus.FirstOrDefault(x => x.Value == 0);
                    if (newAvaliableStatus == null)
                    {
                        client.Send(UtilitiesFunctions.GroupPackets(
                            new ItemSocketOutPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));
                        return;
                    }

                    newAvaliableStatus.SetType(avaliableSocket.Type);
                    newAvaliableStatus.SetValue(avaliableSocket.Value);

                    var itemClone = (ItemModel)newItem.Clone();

                    if(client.Tamer.Inventory.AddItem(itemClone))
                    {
                        await _sender.Send(new UpdateItemSocketStatusCommand(itemClone));
                        await _sender.Send(new UpdateItemAccessoryStatusCommand(itemClone));
                        avaliableSocket.SetType(0);
                        avaliableSocket.SetAttributeId(0);
                        avaliableSocket.SetValue(0);

                        if (itemInfo.AccessoryStatus != null)
                        {
                            var remainingSocketsWithType = itemInfo.SocketStatus.Count(x => x.Type == newAvaliableStatus.Type && x.AttributeId > 0);
                            var accessoryWithType = itemInfo.AccessoryStatus
                                .Where(x => x.Type == newAvaliableStatus.Type && x.Value > 0)
                                .OrderByDescending(x => x.Slot)
                                .ToList();

                            if (accessoryWithType.Count > remainingSocketsWithType)
                            {
                                var staleAccessory = accessoryWithType.First();
                                staleAccessory.SetType(0);
                                staleAccessory.SetValue(0);
                            }
                        }
                    }
                    else
                    {
                        client.Send(UtilitiesFunctions.GroupPackets(
                            new ItemSocketOutPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                            new PickItemFailPacket(PickItemFailReasonEnum.InventoryFull).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));
                        return;
                    }
                }


                client.Tamer.Inventory.RemoveBits(itemInfo.ItemInfo.ScanPrice / 2 * 3 + 1);

                client.Send(UtilitiesFunctions.GroupPackets(
                    new ItemSocketOutPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));

                await _sender.Send(new UpdateItemSocketStatusCommand(itemInfo));
                await _sender.Send(new UpdateItemAccessoryStatusCommand(itemInfo));

                await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));
            }
            else
            {
                client.Send(UtilitiesFunctions.GroupPackets(
                    new ItemSocketOutPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));
            }
        }
    }
}
