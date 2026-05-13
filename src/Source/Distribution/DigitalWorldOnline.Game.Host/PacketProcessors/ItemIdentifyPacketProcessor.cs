using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using MediatR;
using Serilog;
using System;
using System.Linq;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemIdentifyPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ItemIdentify;

        private readonly AssetsLoader _assets;
        private readonly ItemListBinLoader _itemListBinLoader;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public ItemIdentifyPacketProcessor(
            AssetsLoader assets,
            ItemListBinLoader itemListBinLoader,
            ISender sender,
            ILogger logger)
        {
            _assets = assets;
            _itemListBinLoader = itemListBinLoader;
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var tamerhandle = packet.ReadInt();
            var slot = packet.ReadShort();
            _logger.Information("ItemIdentify request: tamer={TamerId} handle={Handle} slot={Slot}", client.TamerId, tamerhandle, slot);

            var identifiedItem = client.Tamer.Inventory.FindItemBySlot(slot);

            if (identifiedItem != null)
            {
                var itemInfo = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == identifiedItem.ItemId);
                if (itemInfo == null)
                {
                    _logger.Error($"Invalid item info for item {identifiedItem.ItemId} and tamer {client.TamerId}.");
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new SystemMessagePacket($"Invalid item info for item {identifiedItem.ItemId}.").Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    return;
                }

                var optionInfo = ResolveAccessoryOptionInfo(itemInfo.Type);
                var enchantInfo = ResolveAccessoryEnchantInfo(itemInfo.Type);
                if (optionInfo != null && enchantInfo != null)
                {
                    int statusAmount = Math.Max(0, Math.Min(identifiedItem.AccessoryStatus.Count, optionInfo.MaxStatusCount));
                    _logger.Information("ItemIdentify processing: tamer={TamerId} slot={Slot} item={ItemId} statusAmount={StatusAmount}", client.TamerId, slot, identifiedItem.ItemId, statusAmount);
                    identifiedItem.AccessoryStatus = identifiedItem.AccessoryStatus.OrderBy(x => x.Slot).ToList();

                    for (int i = 0; i < statusAmount; i++)
                    {
                        var possibleStatus = optionInfo.Options
                            .Where(x => identifiedItem.StatusAmount(x.Type) < x.MaxAmount)
                            .ToList();
                        if (!possibleStatus.Any())
                            continue;

                        var selectedIndex = UtilitiesFunctions.RandomInt(0, possibleStatus.Count - 1);
                        var newStatus = possibleStatus[selectedIndex];

                        identifiedItem.AccessoryStatus[i].SetType(newStatus.Type);
                        identifiedItem.AccessoryStatus[i].SetValue(UtilitiesFunctions.RandomShort(newStatus.MinValue, newStatus.MaxValue));
                    }

                    var rankRange = _itemListBinLoader.Data.Rank.FirstOrDefault(x => x.ItemId == identifiedItem.ItemId);
                    if (rankRange != null)
                    {
                        var min = (byte)Math.Clamp(rankRange.Min, byte.MinValue, byte.MaxValue);
                        var max = (byte)Math.Clamp(rankRange.Max, min, byte.MaxValue);
                        identifiedItem.SetPower(UtilitiesFunctions.RandomByte(min, max));
                    }
                    else
                    {
                        identifiedItem.SetPower(UtilitiesFunctions.RandomByte(95, 102));
                    }
                    identifiedItem.SetReroll(UtilitiesFunctions.RandomByte((byte)enchantInfo.MinReroll, (byte)enchantInfo.MaxReroll));

                    await _sender.Send(new UpdateItemAccessoryStatusCommand(identifiedItem));

                    var statusString = identifiedItem.AccessoryStatus.Where(x => x.Value > 0)?.Select(x => $"{x.Type} {x.Value}");
                    _logger.Verbose($"Character {client.TamerId} identified item {identifiedItem.ItemId} with power {identifiedItem.Power} " +
                        $"reroll {identifiedItem.RerollLeft} and status {string.Join(',', statusString)}.");
                    AccessoryParitySnapshot.LogItemSnapshot(
                        _logger,
                        "identify-success",
                        client.TamerId,
                        slot,
                        identifiedItem);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemIdentifyPacket(slot, identifiedItem).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Information("ItemIdentify success: tamer={TamerId} slot={Slot} item={ItemId} power={Power} reroll={Reroll}", client.TamerId, slot, identifiedItem.ItemId, identifiedItem.Power, identifiedItem.RerollLeft);
                }
                else
                {
                    _logger.Error($"Invalid accessory option/enchant bin data with item id {identifiedItem.ItemId} for tamer {client.TamerId}.");
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemIdentifyPacket(slot, identifiedItem).Serialize(),
                            new SystemMessagePacket($"Invalid accessory data.").Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Warning("ItemIdentify accessory-data miss fallback: tamer={TamerId} slot={Slot} item={ItemId}", client.TamerId, slot, identifiedItem.ItemId);
                }
            }
            else
            {
                _logger.Error($"Invalid item for accessory identify at slot {slot} for tamer {client.TamerId}.");
                client.Send(
                    UtilitiesFunctions.GroupPackets(
                        new ItemIdentifyPacket(slot, new ItemModel()).Serialize(),
                        new SystemMessagePacket($"Invalid item.").Serialize(),
                        new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                    )
                );
                _logger.Warning("ItemIdentify invalid-item fallback: tamer={TamerId} slot={Slot}", client.TamerId, slot);
            }
        }

        private AccessoryOptionInfo? ResolveAccessoryOptionInfo(int accessoryType)
        {
            var record = _itemListBinLoader.Data.AccessoryOptions.FirstOrDefault(x => x.ItemType == (uint)accessoryType);
            if (record == null)
                return null;

            var grouped = record.Options
                .Where(x => Enum.IsDefined(typeof(AccessoryStatusTypeEnum), (int)x.OptionType))
                .GroupBy(x => (AccessoryStatusTypeEnum)x.OptionType)
                .Select(group =>
                {
                    short min = (short)Math.Clamp(group.Min(x => (long)x.MinValue), short.MinValue, short.MaxValue);
                    short max = (short)Math.Clamp(group.Max(x => (long)x.MaxValue), short.MinValue, short.MaxValue);
                    return new AccessoryOptionRange(group.Key, min, max, group.Count());
                })
                .ToList();

            if (!grouped.Any())
                return null;

            int maxStatusCount = Math.Max(0, Math.Min(record.MaxValue, grouped.Sum(x => x.MaxAmount)));
            return new AccessoryOptionInfo(maxStatusCount, grouped);
        }

        private AccessoryEnchantInfo? ResolveAccessoryEnchantInfo(int accessoryType)
        {
            var record = _itemListBinLoader.Data.AccessoryEnchants.FirstOrDefault(x => x.ItemType == (uint)accessoryType);
            if (record == null)
                return null;

            int min = Math.Max(0, (int)record.MinValue);
            int max = Math.Max(min, (int)record.MaxValue);
            max = Math.Min(max, byte.MaxValue);
            return new AccessoryEnchantInfo(min, max);
        }

        private sealed record AccessoryOptionInfo(int MaxStatusCount, List<AccessoryOptionRange> Options);

        private sealed record AccessoryOptionRange(AccessoryStatusTypeEnum Type, short MinValue, short MaxValue, int MaxAmount);

        private sealed record AccessoryEnchantInfo(int MinReroll, int MaxReroll);
    }
}
