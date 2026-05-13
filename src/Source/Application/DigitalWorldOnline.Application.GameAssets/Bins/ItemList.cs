using DigitalWorldOnline.Commons.DTOs.Assets;

namespace DigitalWorldOnline.Application.GameAssets.Bins;

public sealed class ItemList
{
    public IReadOnlyList<ItemAssetDTO> Items { get; }
    public IReadOnlyList<ItemTapRecord> ItemTap { get; }
    public IReadOnlyList<ItemCoolTimeRecord> ItemCoolTime { get; }
    public IReadOnlyList<ItemMapDisplayRecord> MapDisp { get; }
    public IReadOnlyList<ItemMapTypeNameRecord> MapTypeName { get; }
    public IReadOnlyList<ItemRankRecord> Rank { get; }
    public IReadOnlyList<uint> ElementItem1 { get; }
    public IReadOnlyList<uint> ElementItem2 { get; }
    public IReadOnlyList<ItemExchangeRecord> Exchange { get; }
    public IReadOnlyList<ItemAccessoryOptionRecord> AccessoryOptions { get; }
    public IReadOnlyList<ItemAccessoryEnchantRecord> AccessoryEnchants { get; }
    public ItemListSectionCounts Sections { get; }

    public ItemList(
        IReadOnlyList<ItemAssetDTO> items,
        IReadOnlyList<ItemTapRecord> itemTap,
        IReadOnlyList<ItemCoolTimeRecord> itemCoolTime,
        IReadOnlyList<ItemMapDisplayRecord> mapDisp,
        IReadOnlyList<ItemMapTypeNameRecord> mapTypeName,
        IReadOnlyList<ItemRankRecord> rank,
        IReadOnlyList<uint> elementItem1,
        IReadOnlyList<uint> elementItem2,
        IReadOnlyList<ItemExchangeRecord> exchange,
        IReadOnlyList<ItemAccessoryOptionRecord> accessoryOptions,
        IReadOnlyList<ItemAccessoryEnchantRecord> accessoryEnchants,
        ItemListSectionCounts sections)
    {
        Items = items;
        ItemTap = itemTap;
        ItemCoolTime = itemCoolTime;
        MapDisp = mapDisp;
        MapTypeName = mapTypeName;
        Rank = rank;
        ElementItem1 = elementItem1;
        ElementItem2 = elementItem2;
        Exchange = exchange;
        AccessoryOptions = accessoryOptions;
        AccessoryEnchants = accessoryEnchants;
        Sections = sections;
    }
}

public sealed record ItemTapRecord(
    ushort Type,
    string Name);

public sealed record ItemCoolTimeRecord(
    uint ItemType,
    byte Group,
    double CooldownSeconds);

public sealed record ItemMapDisplayRecord(
    uint ItemType,
    uint MapId);

public sealed record ItemMapTypeNameRecord(
    uint Type,
    string Name);

public sealed record ItemRankRecord(
    int ItemId,
    ushort Min,
    ushort Max);

public sealed record ItemExchangeRecord(
    int Index,
    uint ItemType,
    ushort Unknown1,
    uint Unknown2,
    int Unknown3,
    int Unknown4,
    int Unknown5,
    int Unknown6,
    ushort Unknown7,
    ushort Unknown8,
    ushort Unknown9,
    ushort Unknown10,
    ushort Unknown11);

public sealed record ItemAccessoryOptionRecord(
    int Index,
    uint ItemType,
    short MinValue,
    short MaxValue,
    IReadOnlyList<ItemAccessoryOptionSlot> Options);

public sealed record ItemAccessoryOptionSlot(
    short OptionType,
    uint MinValue,
    uint MaxValue);

public sealed record ItemAccessoryEnchantRecord(
    int Index,
    uint ItemType,
    short MinValue,
    short MaxValue);

public sealed record ItemListSectionCounts(
    int Items,
    int ItemTap,
    int CoolTime,
    int MapDisp,
    int MapTypeName,
    int Rank,
    int ElementItem1,
    int ElementItem2,
    int Exchange,
    int AccessoryOption,
    int AccessoryEnchant);
