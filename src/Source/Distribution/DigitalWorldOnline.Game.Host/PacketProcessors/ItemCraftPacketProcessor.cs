using AutoMapper;
using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemCraftPacketProcessor : IGamePacketProcessor
    {
        private const int ResultSuccess = 0;
        private const int ResultNoMakeItemTable = 11078;
        private const int ResultInventoryFull = 30502;
        private const int ResultNotEnoughMoney = 11071;
        private const int ResultNotEnoughResource = 11040;
        private const int ResultInvalidMakeCount = 11079;

        public GameServerPacketEnum Type => GameServerPacketEnum.ItemCraft;

        private readonly AssetsLoader _assets;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ItemCraftPacketProcessor(
            AssetsLoader assets,
            ISender sender,
            ILogger logger,
            IMapper mapper)
        {
            _assets = assets;
            _sender = sender;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var npcId = packet.ReadInt();
            var sequencialId = packet.ReadInt();
            var requestAmount = (int)(ushort)packet.ReadShort();
            var increaseRateItem = packet.ReadInt();
            var protectItem = packet.ReadInt();

            var craftRecipe = _mapper.Map<ItemCraftAssetModel>(await _sender.Send(new ItemCraftAssetsByFilterQuery(npcId, sequencialId)));

            if (craftRecipe == null)
            {
                client.Send(new CraftItemPacket(
                    ResultNoMakeItemTable,
                    new ItemCraftAssetModel { ItemId = 0, Materials = new List<ItemCraftMaterialAssetModel>() },
                    0,
                    0,
                    client.Tamer.Inventory.Bits,
                    0,
                    0,
                    Array.Empty<(int ItemId, int Amount)>(),
                    0));
                _logger.Warning($"Item craft not found with NPC id {npcId} and id {sequencialId} for tamer {client.TamerId}.");
                return;
            }

            if (requestAmount <= 0)
            {
                client.Send(new CraftItemPacket(
                    ResultInvalidMakeCount,
                    craftRecipe,
                    0,
                    0,
                    client.Tamer.Inventory.Bits,
                    0,
                    0,
                    Array.Empty<(int ItemId, int Amount)>(),
                    0));
                return;
            }

            if (client.Tamer.Inventory.GetEmptySlot < 0)
            {
                client.Send(new CraftItemPacket(
                    ResultInventoryFull,
                    craftRecipe,
                    0,
                    0,
                    client.Tamer.Inventory.Bits,
                    0,
                    0,
                    Array.Empty<(int ItemId, int Amount)>(),
                    0));
                return;
            }

            var totalPrice = requestAmount * craftRecipe.Price;

            if (!client.Tamer.Inventory.RemoveBits(totalPrice))
            {
                client.Send(new CraftItemPacket(
                    ResultNotEnoughMoney,
                    craftRecipe,
                    0,
                    0,
                    client.Tamer.Inventory.Bits,
                    0,
                    0,
                    Array.Empty<(int ItemId, int Amount)>(),
                    0));
                _logger.Warning($"Insuficient bits for item craft NPC id {npcId} and id {sequencialId} for tamer {client.TamerId}.");
                return;
            }

            foreach (var material in craftRecipe.Materials)
            {
                var requiredTotal = material.Amount * requestAmount;
                var inventoryAmount = client.Tamer.Inventory.FindItemsById(material.ItemId).Sum(x => x.Amount);
                if (inventoryAmount < requiredTotal)
                {
                    client.Tamer.Inventory.AddBits(totalPrice);
                    client.Send(new CraftItemPacket(
                        ResultNotEnoughResource,
                        craftRecipe,
                        0,
                        0,
                        client.Tamer.Inventory.Bits,
                        0,
                        0,
                        Array.Empty<(int ItemId, int Amount)>(),
                        0));
                    return;
                }
            }

            if (increaseRateItem > 0)
            {
                var rateItemCount = client.Tamer.Inventory.FindItemsById(increaseRateItem).Sum(x => x.Amount);
                if (rateItemCount < requestAmount)
                {
                    client.Tamer.Inventory.AddBits(totalPrice);
                    client.Send(new CraftItemPacket(
                        ResultNotEnoughResource,
                        craftRecipe,
                        0,
                        0,
                        client.Tamer.Inventory.Bits,
                        0,
                        0,
                        Array.Empty<(int ItemId, int Amount)>(),
                        0));
                    return;
                }
            }

            if (protectItem > 0)
            {
                var protectItemCount = client.Tamer.Inventory.FindItemsById(protectItem).Sum(x => x.Amount);
                if (protectItemCount < requestAmount)
                {
                    client.Tamer.Inventory.AddBits(totalPrice);
                    client.Send(new CraftItemPacket(
                        ResultNotEnoughResource,
                        craftRecipe,
                        0,
                        0,
                        client.Tamer.Inventory.Bits,
                        0,
                        0,
                        Array.Empty<(int ItemId, int Amount)>(),
                        0));
                    return;
                }
            }

            var craftedItem = new ItemModel(craftRecipe.ItemId, craftRecipe.Amount);
            craftedItem.SetItemInfo(_assets.ItemInfo.FirstOrDefault(x => x.ItemId == craftRecipe.ItemId));

            craftedItem.SetDefaultRemainingTime();

            var deletedItems = new Dictionary<int, int>();
            var usedProtectItemId = 0;
            var totalCrafted = 0;
            var tries = requestAmount;

            while (tries > 0)
            {
                var materialConsumed = true;
                foreach (var material in craftRecipe.Materials)
                {
                    var removed = client.Tamer.Inventory.RemoveOrReduceItemsByItemId(material.ItemId, material.Amount);
                    if (!removed)
                    {
                        _logger.Warning(
                            "Character {TamerId} could not consume material {ItemId} x{Amount} for craft {SeqId}.",
                            client.TamerId,
                            material.ItemId,
                            material.Amount,
                            sequencialId);
                        materialConsumed = false;
                        break;
                    }

                    if (deletedItems.TryGetValue(material.ItemId, out var consumedAmount))
                        deletedItems[material.ItemId] = consumedAmount + material.Amount;
                    else
                        deletedItems.Add(material.ItemId, material.Amount);
                }

                if (!materialConsumed)
                    break;

                if (increaseRateItem > 0 && client.Tamer.Inventory.RemoveOrReduceItemsByItemId(increaseRateItem, 1))
                {
                    if (deletedItems.TryGetValue(increaseRateItem, out var consumedRateItem))
                        deletedItems[increaseRateItem] = consumedRateItem + 1;
                    else
                        deletedItems.Add(increaseRateItem, 1);
                }

                var successRate = craftRecipe.SuccessRate;
                if (increaseRateItem > 0)
                {
                    var increaseItemInfo = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == increaseRateItem);
                    if (increaseItemInfo != null)
                    {
                        var bonusRate = Math.Max(0, (int)increaseItemInfo.ApplyValueMax);
                        successRate = (byte)Math.Min(100, successRate + bonusRate);
                    }
                }

                var crafted = successRate >= UtilitiesFunctions.RandomByte(maxValue: 100);
                if (crafted)
                {
                    var tempItem = (ItemModel)craftedItem.Clone();
                    if (!client.Tamer.Inventory.AddItem(tempItem))
                    {
                        _logger.Warning("Character {TamerId} craft add-item failed for item {ItemId}.", client.TamerId, craftRecipe.ItemId);
                        break;
                    }

                    totalCrafted++;
                }
                else if (protectItem > 0 && client.Tamer.Inventory.RemoveOrReduceItemsByItemId(protectItem, 1))
                {
                    usedProtectItemId = protectItem;

                    if (deletedItems.TryGetValue(protectItem, out var consumedProtectItem))
                        deletedItems[protectItem] = consumedProtectItem + 1;
                    else
                        deletedItems.Add(protectItem, 1);
                }

                tries--;
            }

            await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));
            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));

            client.Send(
                UtilitiesFunctions.GroupPackets(
                    new CraftItemPacket(
                        ResultSuccess,
                        craftRecipe,
                        requestAmount,
                        0,
                        client.Tamer.Inventory.Bits,
                        totalCrafted,
                        totalCrafted * craftRecipe.Amount,
                        deletedItems.Select(x => (x.Key, x.Value)).ToList(),
                        usedProtectItemId).Serialize(),
                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                )
            );
        }
    }
}
