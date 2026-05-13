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
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;

using MediatR;
using Serilog;
using System.Linq;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemRerollPacketProcessor : IGamePacketProcessor
    {
        private static readonly List<int> _normalStone = new()
        {
            45003, 46996, 46998, 47003, 47007
        };

        private static readonly List<int> _advancedStone = new()
        {
            47008 , 47106
        };

        private static readonly List<int> _digitaryStone = new()
        {
            45000, 47000
        };

        private static readonly List<int> _highStone = new()
        {
            46997, 46999
        };

        private static readonly List<int> _shinyStone = new()
        {
            10026, 47004
        };

        private static readonly List<int> _amazingStone = new()
        {
            10259, 47009, 47107
        };

        private static readonly List<int> _optionNumberChange = new()
        {
            10052, 45001, 47001, 47005
        };

        private static readonly List<int> _numberChange = new()
        {
            10053, 45002, 47002, 47006
        };

        public GameServerPacketEnum Type => GameServerPacketEnum.ItemReroll;

        private readonly AssetsLoader _assets;
        private readonly ItemListBinLoader _itemListBinLoader;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public ItemRerollPacketProcessor(
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

            var tamerHandle = packet.ReadInt();
            var consumableSlot = packet.ReadShort();
            var accessorySlot = packet.ReadShort();
            byte changedStatusSlot = 0;
            int remaining = (packet.Length - 2) - (int)packet.Packet.Position;
            if (remaining > 0)
                changedStatusSlot = packet.ReadByte();

            var consumedStone = client.Tamer.Inventory.FindItemBySlot(consumableSlot);
            if (consumedStone == null || consumedStone.ItemId == 0)
            {
                _logger.Warning($"Invalid item at slot {consumableSlot} for tamer {client.TamerId}.");
                var accessorySnapshot = client.Tamer.Inventory.FindItemBySlot(accessorySlot);
                client.Send(
                    UtilitiesFunctions.GroupPackets(
                        accessorySnapshot != null
                            ? new ItemRerollPacket(2, accessorySlot, accessorySnapshot).Serialize()
                            : Array.Empty<byte>(),
                        new SystemMessagePacket($"Invalid stone at slot {consumableSlot}.").Serialize(),
                        new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                    )
                );
                return;
            }

            var targetAccessory = client.Tamer.Inventory.FindItemBySlot(accessorySlot);
            if (targetAccessory == null || targetAccessory.ItemId == 0)
            {
                _logger.Warning($"Invalid item at slot {accessorySlot} for tamer {client.TamerId}.");
                client.Send(
                    UtilitiesFunctions.GroupPackets(
                        new ItemRerollPacket(2, accessorySlot, new Commons.Models.Base.ItemModel()).Serialize(),
                        new SystemMessagePacket($"Invalid accessory at slot {accessorySlot}.").Serialize(),
                        new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                    )
                );
                return;
            }

            byte result = 1; //1 = sucesso | 2 = falha | 3 = mantem

            if (_normalStone.Contains(consumedStone.ItemId))
            {
                if (targetAccessory.RerollLeft >= 20)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot increase item {targetAccessory.ItemId} reroll left with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    result = UtilitiesFunctions.RandomByte(0, 1);

                    targetAccessory.RerollLeft += result;

                    client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );

                    if (result > 0)
                        _logger.Verbose($"Character {client.TamerId} increased item {targetAccessory.ItemId} reroll left by {result} with {consumedStone.ItemId}.");
                    else
                        _logger.Verbose($"Character {client.TamerId} failed to increase item {targetAccessory.ItemId} reroll left with {consumedStone.ItemId}.");
                }
            }

            if (_advancedStone.Contains(consumedStone.ItemId))
            {
                if (targetAccessory.RerollLeft + 5 > 20)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot increase item {targetAccessory.ItemId} reroll left with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    targetAccessory.RerollLeft += 5;

                    client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} increased item {targetAccessory.ItemId} reroll left by 5 with {consumedStone.ItemId}.");
                }
            }

            if (_digitaryStone.Contains(consumedStone.ItemId))
            {
                if (targetAccessory.Power >= 200)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot increase item {targetAccessory.ItemId} power with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    result = UtilitiesFunctions.RandomByte(1, 3);

                    if (result == 1)
                    {
                        targetAccessory.Power += 1;
                        _logger.Verbose($"Character {client.TamerId} increase item {targetAccessory.ItemId} power by 1 with {consumedStone.ItemId}.");
                    }
                    else if (result == 2)
                    {
                        targetAccessory.Power -= 1;
                        _logger.Verbose($"Character {client.TamerId} decreased item {targetAccessory.ItemId} power by 1 with {consumedStone.ItemId}.");
                    }
                    else
                    {
                        _logger.Verbose($"Character {client.TamerId} item {targetAccessory.ItemId} power has no changes with {consumedStone.ItemId}.");
                    }

                    client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                }
            }

            if (_highStone.Contains(consumedStone.ItemId))
            {
                if (targetAccessory.Power + 2 > 200)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot increase item {targetAccessory.ItemId} power with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    result = UtilitiesFunctions.RandomByte(1, 3);

                    if (result == 1)
                    {
                        targetAccessory.Power += 2;
                        _logger.Verbose($"Character {client.TamerId} increase item {targetAccessory.ItemId} power by 2 with {consumedStone.ItemId}.");
                    }
                    else if (result == 2)
                    {
                        targetAccessory.Power -= 2;
                        _logger.Verbose($"Character {client.TamerId} decreased item {targetAccessory.ItemId} power by 2 with {consumedStone.ItemId}.");
                    }
                    else
                    {
                        _logger.Verbose($"Character {client.TamerId} item {targetAccessory.ItemId} power has no changes with {consumedStone.ItemId}.");
                    }

                    client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                }
            }

            if (_shinyStone.Contains(consumedStone.ItemId))
            {
                if (targetAccessory.Power + 3 > 200)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );

                    _logger.Verbose($"Character {client.TamerId} cannot increase item {targetAccessory.ItemId} power with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    targetAccessory.Power += 3;

                    client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} increase item {targetAccessory.ItemId} power by 3 with {consumedStone.ItemId}.");
                }
            }

            if (_amazingStone.Contains(consumedStone.ItemId))
            {
                if (targetAccessory.Power + 10 > 200)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot increase item {targetAccessory.ItemId} power with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    targetAccessory.Power += 10;

                    client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} increase item {targetAccessory.ItemId} power by 10 with {consumedStone.ItemId}.");
                }
            }

            if (_optionNumberChange.Contains(consumedStone.ItemId))
            {
                result = 0;

                if (targetAccessory.RerollLeft == 0)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot reroll item {targetAccessory.ItemId} status with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    targetAccessory.RerollLeft -= 1;

                    var itemInfo = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == targetAccessory.ItemId);
                    if (itemInfo == null)
                    {
                        result = 3;

                        client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                        client.Send(
                            UtilitiesFunctions.GroupPackets(
                                new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize(),
                                new SystemMessagePacket($"Invalid item info for item {targetAccessory.ItemId}.").Serialize()
                            )
                        );
                        _logger.Warning($"Invalid item info for item {targetAccessory.ItemId} and tamer {client.TamerId}.");
                        return;
                    }

                    var optionInfo = ResolveAccessoryOptionInfo(itemInfo.Type);
                    if (optionInfo != null)
                    {
                        targetAccessory.AccessoryStatus = targetAccessory.AccessoryStatus.OrderBy(x => x.Slot).ToList();

                        int statusAmount = Math.Max(0, Math.Min(targetAccessory.AccessoryStatus.Count, optionInfo.MaxStatusCount));
                        for (int i = 0; i < statusAmount; i++)
                        {
                            var possibleStatus = optionInfo.Options
                                .Where(x => targetAccessory.StatusAmount(x.Type) < x.MaxAmount)
                                .ToList();
                            if (!possibleStatus.Any())
                                continue;

                            var selectedIndex = UtilitiesFunctions.RandomInt(0, possibleStatus.Count - 1);
                            var newStatus = possibleStatus[selectedIndex];

                            targetAccessory.AccessoryStatus[i].SetType(newStatus.Type);
                            targetAccessory.AccessoryStatus[i].SetValue(UtilitiesFunctions.RandomShort(newStatus.MinValue, newStatus.MaxValue));
                        }

                        var statusString = targetAccessory.AccessoryStatus.Where(x => x.Value > 0)?.Select(x => $"{x.Type} {x.Value}");
                        _logger.Verbose($"Character {client.TamerId} rerolled status for item {targetAccessory.ItemId} with {consumedStone.ItemId} power {targetAccessory.Power} " +
                            $"reroll {targetAccessory.RerollLeft} and new status {string.Join(',', statusString)}.");


                        client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                        client.Send(
                            UtilitiesFunctions.GroupPackets(
                                new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                            )
                        );
                    }
                    else
                    {
                        result = 3;
                        client.Send(
                            UtilitiesFunctions.GroupPackets(
                                new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize(),
                                new SystemMessagePacket($"Invalid accessory option data for item {targetAccessory.ItemId}.").Serialize()
                            )
                        );
                        _logger.Warning($"Invalid accessory option bin data for item {targetAccessory.ItemId} and tamer {client.TamerId}.");
                        return;
                    }
                }
            }

            if (_numberChange.Contains(consumedStone.ItemId))
            {
                result = 0;

                if (targetAccessory.RerollLeft == 0)
                {
                    result = 3;
                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                    _logger.Verbose($"Character {client.TamerId} cannot reroll item {targetAccessory.ItemId} status with {consumedStone.ItemId}.");
                    return;
                }
                else
                {
                    targetAccessory.RerollLeft -= 1;

                    var itemInfo = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == targetAccessory.ItemId);
                    if (itemInfo == null)
                    {
                        result = 3;

                        client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                        client.Send(
                            UtilitiesFunctions.GroupPackets(
                                new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize(),
                                new SystemMessagePacket($"Invalid item info for item {targetAccessory.ItemId}.").Serialize()
                            )
                        );
                        _logger.Warning($"Invalid item info for item {targetAccessory.ItemId} and tamer {client.TamerId}.");
                        return;
                    }

                    var optionInfo = ResolveAccessoryOptionInfo(itemInfo.Type);
                    if (optionInfo != null)
                    {
                        targetAccessory.AccessoryStatus = targetAccessory.AccessoryStatus.OrderBy(x => x.Slot).ToList();

                        if (changedStatusSlot >= targetAccessory.AccessoryStatus.Count)
                        {
                            result = 3;
                            client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);
                            client.Send(
                                UtilitiesFunctions.GroupPackets(
                                    new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize(),
                                    new SystemMessagePacket($"Invalid accessory status slot {changedStatusSlot}.").Serialize()
                                )
                            );
                            _logger.Warning($"Invalid accessory status slot {changedStatusSlot} for item {targetAccessory.ItemId} and tamer {client.TamerId}.");
                            return;
                        }

                        var selectedType = targetAccessory.AccessoryStatus[changedStatusSlot].Type;
                        var newStatus = optionInfo.Options.FirstOrDefault(x => x.Type == selectedType);

                        if (newStatus == null)
                        {
                            result = 3;

                            client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                            client.Send(
                                UtilitiesFunctions.GroupPackets(
                                    new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize(),
                                    new SystemMessagePacket($"Invalid accessory status reroll info for item {targetAccessory.ItemId}.").Serialize()
                                )
                            );
                            _logger.Warning($"Invalid accessory status for item {targetAccessory.ItemId} while tamer {client.TamerId} reroll.");
                            return;
                        }

                        targetAccessory.AccessoryStatus[changedStatusSlot].SetValue(UtilitiesFunctions.RandomShort(newStatus.MinValue, newStatus.MaxValue));

                        client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                        client.Send(
                            UtilitiesFunctions.GroupPackets(
                                new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                            )
                        );
                    }
                    else
                    {
                        result = 3;

                        client.Tamer.Inventory.RemoveOrReduceItem(consumedStone, 1);

                        client.Send(
                            UtilitiesFunctions.GroupPackets(
                                new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                                new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize(),
                                new SystemMessagePacket($"Invalid accessory option data for item {targetAccessory.ItemId}.").Serialize()
                            )
                        );
                        _logger.Warning($"Invalid accessory option bin data for item {targetAccessory.ItemId} and tamer {client.TamerId}.");

                        return;
                    }

                    var statusString = targetAccessory.AccessoryStatus.Where(x => x.Value > 0)?.Select(x => $"{x.Type} {x.Value}");
                    _logger.Verbose($"Character {client.TamerId} rerolled status number for item {targetAccessory.ItemId} with {consumedStone.ItemId} power {targetAccessory.Power} " +
                        $"reroll {targetAccessory.RerollLeft} and new status {string.Join(',', statusString)}.");
                    AccessoryParitySnapshot.LogItemSnapshot(
                        _logger,
                        "reroll-success",
                        client.TamerId,
                        accessorySlot,
                        targetAccessory);

                    client.Send(
                        UtilitiesFunctions.GroupPackets(
                            new ItemRerollPacket(result, accessorySlot, targetAccessory).Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                        )
                    );
                }
            }


            await _sender.Send(new UpdateItemAccessoryStatusCommand(targetAccessory));
            await _sender.Send(new UpdateItemCommand(consumedStone));
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

        private sealed record AccessoryOptionInfo(int MaxStatusCount, List<AccessoryOptionRange> Options);

        private sealed record AccessoryOptionRange(AccessoryStatusTypeEnum Type, short MinValue, short MaxValue, int MaxAmount);
    }
}
