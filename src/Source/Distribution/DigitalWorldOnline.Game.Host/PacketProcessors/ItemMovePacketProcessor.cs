using AutoMapper;
using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemMovePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.MoveItem;

        private readonly MapServer _mapServer;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly DungeonsServer _dungeonServer;

        public ItemMovePacketProcessor(
            MapServer mapServer,
            ISender sender,
            ILogger logger,
            DungeonsServer dungeonsServer,
            IMapper mapper)
        {
            _mapServer = mapServer;
            _sender = sender;
            _logger = logger;
            _dungeonServer = dungeonsServer;
            _mapper = mapper;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var originSlot = packet.ReadShort();
            var destinationSlot = packet.ReadShort();

            var itemListMovimentation = UtilitiesFunctions.SwitchItemList(originSlot, destinationSlot);

            var success = SwapItems(client, originSlot, destinationSlot, itemListMovimentation);
            if (!success)
            {
                _logger.Warning(
                    "ItemMove failed for tamer {TamerId}: origin={OriginSlot} destination={DestinationSlot} movement={Movement}",
                    client.TamerId, originSlot, destinationSlot, itemListMovimentation);
            }

            if (success)
            {
                switch (itemListMovimentation)
                {
                    case ItemListMovimentationEnum.InventoryToInventory:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                        }
                        break;

                    case ItemListMovimentationEnum.EquipmentToInventory:
                    case ItemListMovimentationEnum.InventoryToEquipment:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            client.Tamer.Equipment.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Equipment));
                    
                            
                        }
                        break;

                    case ItemListMovimentationEnum.InventoryToDigivice:
                    case ItemListMovimentationEnum.DigiviceToInventory:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            client.Tamer.Digivice.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Digivice));
                        }
                        break;

                    case ItemListMovimentationEnum.InventoryToChipset:
                    case ItemListMovimentationEnum.ChipsetToInventory:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            client.Tamer.ChipSets.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.ChipSets));
                        }
                        break;
                    case ItemListMovimentationEnum.InventoryToJogressChipset:
                    case ItemListMovimentationEnum.JogressChipsetToInventory:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            client.Tamer.JogressChipSet.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.JogressChipSet));
                        }
                        break;

                    case ItemListMovimentationEnum.WarehouseToInventory:
                    case ItemListMovimentationEnum.InventoryToWarehouse:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            client.Tamer.Warehouse.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                        }
                        break;

                    case ItemListMovimentationEnum.AccountWarehouseToInventory:
                    case ItemListMovimentationEnum.InventoryToAccountWarehouse:
                        {
                            client.Tamer.Inventory.CheckEmptyItems();
                            client.Tamer.AccountWarehouse.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                        }
                        break;

                    case ItemListMovimentationEnum.AccountWarehouseToWarehouse:
                    case ItemListMovimentationEnum.WarehouseToAccountWarehouse:
                        {
                            client.Tamer.AccountWarehouse.CheckEmptyItems();
                            client.Tamer.Warehouse.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                        }
                        break;

                    case ItemListMovimentationEnum.AccountWarehouseToAccountWarehouse:
                        {
                            client.Tamer.AccountWarehouse.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountWarehouse));
                        }
                        break;

                    case ItemListMovimentationEnum.WarehouseToWarehouse:
                        {
                            client.Tamer.Warehouse.CheckEmptyItems();
                            await _sender.Send(new UpdateItemsCommand(client.Tamer.Warehouse));
                        }
                        break;
                }

                client.Send(
                    UtilitiesFunctions.GroupPackets(BuildMoveResultPackets(client, itemListMovimentation, true, originSlot, destinationSlot))
                );

                if (originSlot == GeneralSizeEnum.XaiSlot.GetHashCode())
                {
                    client.Tamer.Xai.RemoveXai();
                    client.Send(new XaiInfoPacket());
                    client.Send(new TamerXaiResourcesPacket(0, 0));
                    await _sender.Send(new UpdateCharacterXaiCommand(client.Tamer.Xai));
                }

                if (destinationSlot == GeneralSizeEnum.XaiSlot.GetHashCode())
                {
                    var equippedItem = client.Tamer.Equipment.FindItemBySlot(destinationSlot - 1000);
                    if (equippedItem == null || equippedItem.ItemId <= 0)
                    {
                        _logger.Warning("Character {TamerId} failed XAI equip: empty slot item.", client.TamerId);
                        return;
                    }

                    var itemId = equippedItem.ItemId;
                    var xaiDto = await _sender.Send(new XaiInformationQuery(itemId));
                    if (xaiDto == null)
                    {
                        _logger.Warning("Character {TamerId} failed XAI equip: item {ItemId} has no XAI asset row.", client.TamerId, itemId);
                        return;
                    }

                    var XaiInfo = _mapper.Map<XaiAssetModel>(xaiDto);

                    client.Tamer.Xai.EquipXai(XaiInfo.ItemId, XaiInfo.XGauge, XaiInfo.XCrystals);
                    client.Tamer.ClampXaiResourcesToCap();

                    client.Send(new XaiInfoPacket(client.Tamer.Xai));
                    client.Send(new TamerXaiResourcesPacket(client.Tamer.XGauge, client.Tamer.XCrystals));

                    await _sender.Send(new UpdateCharacterXaiCommand(client.Tamer.Xai));
                }

                if (IsDigimonStatAffectingMove(itemListMovimentation))
                {
                    LogDigimonStatSnapshot(client, originSlot, destinationSlot, itemListMovimentation);
                    AccessoryParitySnapshot.LogEquippedSnapshot(
                        _logger,
                        $"equip-move:{itemListMovimentation}",
                        client.TamerId,
                        client.Tamer.Equipment.EquippedItems);
                }

                _logger.Verbose($"Character {client.TamerId} moved an item from {originSlot} to {destinationSlot}.");
            }
            else
            {
                client.Send(
                    UtilitiesFunctions.GroupPackets(BuildMoveResultPackets(client, itemListMovimentation, false, originSlot, destinationSlot))
                );

                _logger.Warning($"Character {client.TamerId} failled to move item from {originSlot} to {destinationSlot}.");
            }
        }

        private static bool IsDigimonStatAffectingMove(ItemListMovimentationEnum movimentation)
        {
            return movimentation is ItemListMovimentationEnum.EquipmentToInventory
                or ItemListMovimentationEnum.InventoryToEquipment
                or ItemListMovimentationEnum.InventoryToDigivice
                or ItemListMovimentationEnum.DigiviceToInventory
                or ItemListMovimentationEnum.InventoryToChipset
                or ItemListMovimentationEnum.ChipsetToInventory
                or ItemListMovimentationEnum.InventoryToJogressChipset
                or ItemListMovimentationEnum.JogressChipsetToInventory;
        }

        private void LogDigimonStatSnapshot(
            GameClient client,
            short originSlot,
            short destinationSlot,
            ItemListMovimentationEnum movimentation)
        {
            var partner = client.Partner;
            if (partner == null)
            {
                _logger.Warning(
                    "ItemMove stat snapshot skipped: tamer={TamerId} movement={Movement} origin={OriginSlot} destination={DestinationSlot} reason=NoPartner",
                    client.TamerId, movimentation, originSlot, destinationSlot);
                return;
            }

            _logger.Information(
                "ItemMove stat snapshot: tamer={TamerId} partnerId={PartnerId} partnerName={PartnerName} movement={Movement} origin={OriginSlot} destination={DestinationSlot} HP={HP} DS={DS} AT={AT} DE={DE} AS={AS} HT={HT} BL={BL} CT={CT} EV={EV} CD={CD} ATT={ATT} SCD={SCD}",
                client.TamerId,
                partner.Id,
                partner.Name,
                movimentation,
                originSlot,
                destinationSlot,
                partner.HP,
                partner.DS,
                partner.AT,
                partner.DE,
                partner.AS,
                partner.HT,
                partner.BL,
                partner.CC,
                partner.EV,
                partner.CD,
                partner.ATT,
                partner.SCD);
        }

        private static byte[][] BuildMoveResultPackets(
            GameClient client,
            ItemListMovimentationEnum movimentation,
            bool success,
            short originSlot,
            short destinationSlot)
        {
            var packets = new List<byte[]>
            {
                success
                    ? new ItemMoveSuccessPacket(originSlot, destinationSlot).Serialize()
                    : new ItemMoveFailPacket(originSlot, destinationSlot).Serialize(),
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

            return packets.ToArray();
        }

        private bool SwapItems(GameClient client, short originSlot, short destinationSlot, ItemListMovimentationEnum itemListMovimentation)
        {
            switch (itemListMovimentation)
            {
                case ItemListMovimentationEnum.InventoryToInventory:
                    return client.Tamer.Inventory.MoveItem(originSlot, destinationSlot);

                case ItemListMovimentationEnum.InventoryToDigivice:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.DigiviceSlot.GetHashCode();
                        if (!client.Tamer.Inventory.TryMoveAcrossLists(client.Tamer.Digivice, srcSlot, dstSlot))
                            return false;

                        var equippedItem = client.Tamer.Digivice.FindItemBySlot(dstSlot) ?? new ItemModel();
                        BroadcastAppearanceUpdate(client, 13, equippedItem, 1);
                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.ChipsetToInventory:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.ChipsetMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        if (!client.Tamer.ChipSets.TryMoveAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot))
                            return false;

                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.InventoryToChipset:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.ChipsetMinSlot.GetHashCode();
                        if (!client.Tamer.Inventory.TryMoveAcrossLists(client.Tamer.ChipSets, srcSlot, dstSlot))
                            return false;

                        SendStatusAndSpeed(client);
                        return true;
                    }
                case ItemListMovimentationEnum.InventoryToJogressChipset:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        const int dstSlot = 0;
                        if (!client.Tamer.Inventory.TryMoveAcrossLists(client.Tamer.JogressChipSet, srcSlot, dstSlot))
                            return false;

                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.JogressChipsetToInventory:
                    {
                        const int srcSlot = 0;
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        if (!client.Tamer.JogressChipSet.TryMoveAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot))
                            return false;

                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.InventoryToEquipment:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot == GeneralSizeEnum.XaiSlot.GetHashCode() ? 11 : destinationSlot - GeneralSizeEnum.EquipmentMinSlot.GetHashCode();
                        var srcItemBefore = client.Tamer.Inventory.FindItemBySlot(srcSlot);
                        var dstItemBefore = client.Tamer.Equipment.FindItemBySlot(dstSlot);
                        if (!client.Tamer.Inventory.TryMoveAcrossLists(client.Tamer.Equipment, srcSlot, dstSlot))
                        {
                            _logger.Warning(
                                "InventoryToEquipment rejected: tamer={TamerId} srcSlot={SrcSlot} dstSlot={DstSlot} invSize={InvSize} equipSize={EquipSize} srcExists={SrcExists} srcItemId={SrcItemId} srcAmount={SrcAmount} dstExists={DstExists} dstItemId={DstItemId} dstAmount={DstAmount}",
                                client.TamerId,
                                srcSlot,
                                dstSlot,
                                client.Tamer.Inventory.Size,
                                client.Tamer.Equipment.Size,
                                srcItemBefore != null,
                                srcItemBefore?.ItemId ?? 0,
                                srcItemBefore?.Amount ?? 0,
                                dstItemBefore != null,
                                dstItemBefore?.ItemId ?? 0,
                                dstItemBefore?.Amount ?? 0);
                            return false;
                        }

                        var equippedItem = client.Tamer.Equipment.FindItemBySlot(dstSlot) ?? new ItemModel();
                        BroadcastAppearanceUpdate(client, (byte)dstSlot, equippedItem, 1);
                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.InventoryToWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        return client.Tamer.Inventory.TryMoveAcrossLists(client.Tamer.Warehouse, srcSlot, dstSlot);
                    }

                case ItemListMovimentationEnum.InventoryToAccountWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        var sourceItem = client.Tamer.Inventory.FindItemBySlot(srcSlot);
                        var destinationItem = client.Tamer.AccountWarehouse.FindItemBySlot(dstSlot);
                        var moved = client.Tamer.Inventory.TryMoveAcrossLists(client.Tamer.AccountWarehouse, srcSlot, dstSlot);
                        if (!moved)
                        {
                            _logger.Warning(
                                "InventoryToAccountWarehouse rejected: tamer={TamerId} srcSlot={SrcSlot} dstSlot={DstSlot} invSize={InvSize} accSize={AccSize} srcExists={SrcExists} srcItemId={SrcItemId} srcAmount={SrcAmount} dstExists={DstExists} dstItemId={DstItemId} dstAmount={DstAmount}",
                                client.TamerId,
                                srcSlot,
                                dstSlot,
                                client.Tamer.Inventory.Size,
                                client.Tamer.AccountWarehouse.Size,
                                sourceItem != null,
                                sourceItem?.ItemId ?? 0,
                                sourceItem?.Amount ?? 0,
                                destinationItem != null,
                                destinationItem?.ItemId ?? 0,
                                destinationItem?.Amount ?? 0);
                        }

                        return moved;
                    }

                case ItemListMovimentationEnum.EquipmentToInventory:
                    {
                        var srcSlot = originSlot == GeneralSizeEnum.XaiSlot.GetHashCode() ? 11 : originSlot - GeneralSizeEnum.EquipmentMinSlot.GetHashCode();
                        var dstSlot = destinationSlot;
                        var destinationItem = client.Tamer.Inventory.FindItemBySlot(dstSlot);
                        var hadDestinationItem = destinationItem?.ItemId > 0;
                        if (!client.Tamer.Equipment.TryMoveAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot))
                            return false;

                        var equippedItem = client.Tamer.Equipment.FindItemBySlot(srcSlot) ?? new ItemModel();
                        BroadcastAppearanceUpdate(
                            client,
                            (byte)srcSlot,
                            equippedItem.ItemId > 0 ? equippedItem : new ItemModel(),
                            hadDestinationItem ? (byte)1 : (byte)0);
                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.DigiviceToInventory:
                    {
                        var srcSlot = 0;
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        if (!client.Tamer.Digivice.TryMoveAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot))
                            return false;

                        var digiviceItem = client.Tamer.Digivice.FindItemBySlot(srcSlot) ?? new ItemModel();
                        BroadcastAppearanceUpdate(
                            client,
                            13,
                            digiviceItem.ItemId > 0 ? digiviceItem : new ItemModel(),
                            0);
                        SendStatusAndSpeed(client);
                        return true;
                    }

                case ItemListMovimentationEnum.WarehouseToWarehouse:
                    {
                        var orgSlot = (short)(originSlot - (short)GeneralSizeEnum.WarehouseMinSlot);
                        var destSlot = (short)(destinationSlot - (short)GeneralSizeEnum.WarehouseMinSlot);

                        return client.Tamer.Warehouse.MoveItem(orgSlot, destSlot);
                    }

                case ItemListMovimentationEnum.AccountWarehouseToAccountWarehouse:
                    {
                        var orgSlot = (short)(originSlot - (short)GeneralSizeEnum.AccountWarehouseMinSlot);
                        var destSlot = (short)(destinationSlot - (short)GeneralSizeEnum.AccountWarehouseMinSlot);

                        return client.Tamer.AccountWarehouse.MoveItem(orgSlot, destSlot);
                    }

                case ItemListMovimentationEnum.WarehouseToInventory:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        return client.Tamer.Warehouse.TryMoveAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot);
                    }

                case ItemListMovimentationEnum.WarehouseToAccountWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        return client.Tamer.Warehouse.TryMoveAcrossLists(client.Tamer.AccountWarehouse, srcSlot, dstSlot);
                    }

                case ItemListMovimentationEnum.AccountWarehouseToInventory:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.InventoryMinSlot.GetHashCode();
                        return client.Tamer.AccountWarehouse.TryMoveAcrossLists(client.Tamer.Inventory, srcSlot, dstSlot);
                    }

                case ItemListMovimentationEnum.AccountWarehouseToWarehouse:
                    {
                        var srcSlot = originSlot - GeneralSizeEnum.AccountWarehouseMinSlot.GetHashCode();
                        var dstSlot = destinationSlot - GeneralSizeEnum.WarehouseMinSlot.GetHashCode();
                        return client.Tamer.AccountWarehouse.TryMoveAcrossLists(client.Tamer.Warehouse, srcSlot, dstSlot);
                    }
            }

            return false;
        }

        private void BroadcastAppearanceUpdate(GameClient client, byte slot, ItemModel item, byte mode)
        {
            var packet = new UpdateTamerAppearancePacket(
                client.Tamer.GeneralHandler,
                slot,
                item,
                mode).Serialize();

            if (client.DungeonMap)
            {
                _dungeonServer.BroadcastForTamerViewsAndSelf(client.TamerId, packet);
            }
            else
            {
                _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, packet);
            }
        }

        private void SendStatusAndSpeed(GameClient client)
        {
            client.Send(new UpdateStatusPacket(client.Tamer));

            var packet = new UpdateMovementSpeedPacket(client.Tamer).Serialize();
            if (client.DungeonMap)
            {
                _dungeonServer.BroadcastForTamerViewsAndSelf(client.TamerId, packet);
            }
            else
            {
                _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, packet);
            }
        }
    }
}
