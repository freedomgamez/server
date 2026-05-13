using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.DTOs.Base;
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
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;
using System.Net.Mime;


namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemScanPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ItemScan;

        private readonly AssetsLoader _assets;
        private readonly MapServer _mapServer;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public ItemScanPacketProcessor(
            AssetsLoader assets,
            MapServer mapServer,
            ISender sender,
            ILogger logger)
        {
            _assets = assets;
            _mapServer = mapServer;
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            int remaining = (packet.Length - 2) - (int)packet.Packet.Position;
            byte vipEnabled = 0;
            int portableIdx = 0;
            int npcId;
            int slotToScan;
            short amountToScan;

            if (remaining == 14)
            {
                portableIdx = packet.ReadInt();
                npcId = packet.ReadInt();
                slotToScan = packet.ReadInt();
                amountToScan = packet.ReadShort();
            }
            else if (remaining == 15)
            {
                vipEnabled = packet.ReadByte();
                portableIdx = packet.ReadInt();
                npcId = packet.ReadInt();
                slotToScan = packet.ReadInt();
                amountToScan = packet.ReadShort();
            }
            else if (remaining == 10)
            {
                npcId = packet.ReadInt();
                slotToScan = packet.ReadInt();
                amountToScan = packet.ReadShort();
            }
            else if (remaining == 11)
            {
                vipEnabled = packet.ReadByte();
                npcId = packet.ReadInt();
                slotToScan = packet.ReadInt();
                amountToScan = packet.ReadShort();
            }
            else
            {
                client.Send(
                    UtilitiesFunctions.GroupPackets(
                        new ItemScanFailPacket(client.Tamer.Inventory.Bits, 0, 0).Serialize(),
                        new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                    )
                );
                _logger.Warning("Scan: invalid payload length {Length} for tamer {TamerId}.", remaining, client.TamerId);
                return;
            }

            var scannedItem = client.Tamer.Inventory.FindItemBySlot(slotToScan);
            if (scannedItem == null || scannedItem.ItemId == 0 || scannedItem.ItemInfo == null)
            {
                client.Send(
                    UtilitiesFunctions.GroupPackets(
                        new SystemMessagePacket($"Invalid item at slot {slotToScan}.").Serialize(),
                        new ItemScanFailPacket(client.Tamer.Inventory.Bits, slotToScan, scannedItem?.ItemId ?? 0).Serialize(),
                        new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                    )
                );
                var invSlots = string.Join(",", client.Tamer.Inventory.Items
                    .Where(i => i.ItemId > 0)
                    .Select(i => $"slot={i.Slot}/item={i.ItemId}"));
                _logger.Warning(
                    "Scan: client sent slot={Slot} vip={Vip} portable={Portable} npc={Npc} count={Count}; tamer {TamerId} inventory items: [{Inv}]",
                    slotToScan, vipEnabled, portableIdx, npcId, amountToScan, client.TamerId, invSlots);
                return;
            }

            var scanAsset = _assets.ScanDetail.FirstOrDefault(x => x.ItemId == scannedItem.ItemId);

            if (scanAsset == null)
            {
                client.Send(
                    UtilitiesFunctions.GroupPackets(
                        new SystemMessagePacket($"No scan configuration for item id {scannedItem.ItemId}.").Serialize(),
                        new ItemScanFailPacket(client.Tamer.Inventory.Bits, slotToScan, scannedItem.ItemId).Serialize(),
                        new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                    )
                );
                _logger.Warning($"No scan configuration for item id {scannedItem.ItemId}");
                return;
            }

            var receivedRewards = new Dictionary<int, ItemModel>();
            short scannedItens = 0;
            long cost = 0;
            var error = false;

            while (scannedItens < amountToScan && !error)
            {
                if (!scanAsset.Rewards.Any())
                {
                    _logger.Warning($"Scan config for item {scanAsset.ItemId} has incorrect rewards configuration.");
                    client.Send(new SystemMessagePacket($"Scan config for item {scanAsset.ItemId} has incorrect rewards configuration."));
                    break;
                }

                var possibleRewards = scanAsset.Rewards.OrderBy(x => Guid.NewGuid()).ToList();
                foreach (var possibleReward in possibleRewards)
                {
                    if (cost + scannedItem.ItemInfo.ScanPrice > client.Tamer.Inventory.Bits)
                    {
                        _logger.Warning($"No more bits after start scanning for tamer {client.TamerId}.");
                        error = true;
                        break;
                    }

                    if (possibleReward.Chance >= UtilitiesFunctions.RandomDouble())
                    {
                        var itemRewardAmount = UtilitiesFunctions.RandomInt(possibleReward.MinAmount, possibleReward.MaxAmount);

                        var contentItem = new ItemModel();
                        contentItem.SetItemId(possibleReward.ItemId);
                        contentItem.SetAmount(itemRewardAmount);
                        contentItem.SetItemInfo(_assets.ItemInfo.FirstOrDefault(x => x.ItemId == possibleReward.ItemId));
    
                        if (contentItem.ItemInfo == null)
                        {
                            _logger.Warning($"Invalid item info for item {possibleReward.ItemId} in tamer {client.TamerId} scan.");
                            client.Send(new SystemMessagePacket($"Invalid item info for item {possibleReward.ItemId}."));
                            error = true;
                            break;
                        }

                        if (contentItem.ItemInfo.Section == 5200)
                        {
                            _ = ApplyValuesChipset(contentItem);
                        }


                        if (contentItem.IsTemporary)
                            contentItem.SetRemainingTime((uint)contentItem.ItemInfo.UsageTimeMinutes);

                        var targetSlot = client.Tamer.Inventory.FindAvailableSlot(contentItem);

                        if (targetSlot != client.Tamer.Inventory.GetEmptySlot)
                        {
                            var inventoryItem = client.Tamer.Inventory.FindItemBySlot(targetSlot);
                            if (inventoryItem == null)
                            {
                                error = true;
                                break;
                            }

                            var tempItem = new ItemModel(inventoryItem.ItemId, inventoryItem.Amount + contentItem.Amount)
                            {
                                ItemInfo = inventoryItem.ItemInfo,
                                Power = inventoryItem.Power,
                                RerollLeft = inventoryItem.RerollLeft,
                                FamilyType = inventoryItem.FamilyType,
                                Duration = inventoryItem.Duration,
                                EndDate = inventoryItem.EndDate,
                                FirstExpired = inventoryItem.FirstExpired
                            };

                            if (!receivedRewards.ContainsKey(targetSlot))
                                receivedRewards.Add(targetSlot, tempItem);
                            else
                                receivedRewards[targetSlot].IncreaseAmount(contentItem.Amount);
                        }
                        else
                        {
                            var tempItem = new ItemModel(contentItem.ItemId, contentItem.Amount)
                            {
                                ItemInfo = contentItem.ItemInfo,
                                Power = contentItem.Power,
                                RerollLeft = contentItem.RerollLeft,
                                FamilyType = contentItem.FamilyType,
                                Duration = contentItem.Duration,
                                EndDate = contentItem.EndDate,
                                FirstExpired = contentItem.FirstExpired
                            };

                            if (!receivedRewards.ContainsKey(targetSlot))
                                receivedRewards.Add(targetSlot, tempItem);
                            else
                                receivedRewards[targetSlot].IncreaseAmount(contentItem.Amount);
                        }

                        if (client.Tamer.Inventory.AddItem(contentItem))
                        {

                            if (possibleReward.Rare)
                                _mapServer.BroadcastForChannel(client.Tamer.Channel, new NeonMessagePacket(NeonMessageTypeEnum.Item, client.Tamer.Name, scanAsset.ItemId, possibleReward.ItemId).Serialize());

                            cost += scannedItem.ItemInfo.ScanPrice;
                            scannedItens++;
                        }
                        else
                        {
                            _logger.Warning($"No more space after start scanning for tamer {client.TamerId}.");
                            error = true;
                            break;
                        }
                    }

                    if (scannedItens >= amountToScan || error)
                        break;
                }
            }

            var dropList = string.Join(',', receivedRewards.Select(x => $"{x.Value.ItemId} x{x.Value.Amount}"));

            if (vipEnabled == 1)
            {
                _logger.Verbose($"Character {client.TamerId} scanned {scannedItem.ItemId} x{scannedItens} with VIP and obtained {dropList}");
            }
            else
            {
                _logger.Verbose($"Character {client.TamerId} scanned {scannedItem.ItemId} x{scannedItens} at {client.TamerLocation} with NPC {npcId} and obtained {dropList}");
            }

            client.Tamer.Inventory.RemoveBits(cost);
            client.Tamer.Inventory.RemoveOrReduceItem(scannedItem, scannedItens, slotToScan);

            var scanQuest = client.Tamer.Progress.InProgressQuestData.FirstOrDefault(x => x.QuestId == 4021);
            if (scanQuest != null && scanAsset.ItemId == 9071)
            {
                scanQuest.UpdateCondition(0, 1);
                client.Send(new QuestGoalUpdatePacket(4021, 0, 1));
                var questToUpdate = client.Tamer.Progress.InProgressQuestData.FirstOrDefault(x => x.QuestId == 4021);
                _sender.Send(new UpdateCharacterInProgressCommand(questToUpdate));
            }

            await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));
            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
            client.Send(
                UtilitiesFunctions.GroupPackets(
                    new ItemScanSuccessPacket(
                        cost,
                        client.Tamer.Inventory.Bits,
                        slotToScan,
                        scannedItem.ItemId,
                        scannedItens,
                        receivedRewards).Serialize(),
                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()
                )
            );
        }
        private ItemModel? ApplyValuesChipset(ItemModel newItem)
        {
            var skillCodeInfo = _assets.SkillCodeInfo.FirstOrDefault(x => x.SkillCode == newItem.ItemInfo.SkillCode);
            var chipsetInfo = skillCodeInfo?.Apply?.FirstOrDefault(x => x.Type > 0);
            var skillInfo = _assets.SkillInfo.FirstOrDefault(x => x.SkillId == newItem.ItemInfo.SkillCode);
            if (chipsetInfo == null || skillInfo == null)
                return newItem;

            var chipsetSkill = skillInfo.FamilyType;
            // Definindo o valor mínimo e máximo para o RNG

            Random random = new Random();
            int ApplyRate = random.Next(newItem.ItemInfo.ApplyValueMin, newItem.ItemInfo.ApplyValueMax);
            var nValue = chipsetInfo.Value + (newItem.ItemInfo.TypeN) * chipsetInfo.AdditionalValue;

            int valorAleatorio = (int)((double)ApplyRate * nValue / 100);

            newItem.AccessoryStatus = newItem.AccessoryStatus.OrderBy(x => x.Slot).ToList();

            if (!TryMapAccessoryStatusType(chipsetInfo.Attribute, out var possibleStatus))
                return newItem;

            newItem.AccessoryStatus[0].SetType(possibleStatus);
            newItem.AccessoryStatus[0].SetValue((short)valorAleatorio);

            newItem.SetPower((byte)ApplyRate); //TODO: externalizar
            newItem.SetReroll((byte)100);
            newItem.SetFamilyType(chipsetSkill);
            return newItem;
        }

        private static bool TryMapAccessoryStatusType(
            SkillCodeApplyAttributeEnum attribute,
            out AccessoryStatusTypeEnum mappedType)
        {
            mappedType = attribute switch
            {
                SkillCodeApplyAttributeEnum.AT => AccessoryStatusTypeEnum.AT,
                SkillCodeApplyAttributeEnum.DP => AccessoryStatusTypeEnum.DE,
                SkillCodeApplyAttributeEnum.HP => AccessoryStatusTypeEnum.HP,
                SkillCodeApplyAttributeEnum.DS => AccessoryStatusTypeEnum.DS,
                SkillCodeApplyAttributeEnum.SCD => AccessoryStatusTypeEnum.SCD,
                SkillCodeApplyAttributeEnum.SkillDamageByAttribute => AccessoryStatusTypeEnum.ATT,
                SkillCodeApplyAttributeEnum.CA => AccessoryStatusTypeEnum.CT,
                SkillCodeApplyAttributeEnum.ER => AccessoryStatusTypeEnum.CD,
                SkillCodeApplyAttributeEnum.AS => AccessoryStatusTypeEnum.AS,
                SkillCodeApplyAttributeEnum.EV => AccessoryStatusTypeEnum.EV,
                SkillCodeApplyAttributeEnum.BL => AccessoryStatusTypeEnum.BL,
                SkillCodeApplyAttributeEnum.HT => AccessoryStatusTypeEnum.HT,
                _ => default
            };

            return mappedType != default;
        }

    }
}
