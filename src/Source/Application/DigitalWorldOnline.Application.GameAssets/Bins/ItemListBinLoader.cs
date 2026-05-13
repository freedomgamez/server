using System.Text;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums.ClientEnums;

namespace DigitalWorldOnline.Application.GameAssets.Bins;

public sealed class ItemListBinLoader
{
    private const string FileName = "ItemList.bin";
    private const int Utf16Bytes = 2;

    private static readonly int ItemInfoRecordSize = ComputeItemInfoRecordSize();
    private static readonly int ItemTapRecordSize = sizeof(ushort) + (32 * Utf16Bytes);
    private static readonly int ItemCoolTimeRecordSize = Align(sizeof(uint) + sizeof(byte), 8) + sizeof(double);
    private static readonly int MapDispRecordSize = sizeof(uint) + sizeof(uint);
    private static readonly int MapTypeNameRecordSize = sizeof(uint) + (64 * Utf16Bytes);
    private static readonly int RankRecordSize = sizeof(int) + sizeof(ushort) + sizeof(ushort);
    private static readonly int ElementItemRecordSize = sizeof(uint);
    private static readonly int ExchangeRecordSize = sizeof(int) + ComputeExchangeInfoSize();
    private static readonly int AccessoryOptionRecordSize = sizeof(int) + ComputeAccessoryOptionInfoSize();
    private static readonly int AccessoryEnchantRecordSize = sizeof(int) + sizeof(uint) + sizeof(short) + sizeof(short);

    private ItemList? _data;

    public ItemList Data => _data ?? throw new InvalidOperationException(
        $"{nameof(ItemListBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public ItemList Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var fs = File.OpenRead(path);
        using var r = new BinaryReader(fs);
        _data = Parse(r);
        return _data;
    }

    public static ItemList Parse(BinaryReader r)
    {
        int itemCount = r.ReadInt32();
        var items = new List<ItemAssetDTO>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(ReadItemInfo(r));
        }

        var itemTap = ReadItemTapSection(r);
        var coolTime = ReadItemCoolTimeSection(r);
        var mapDisp = ReadMapDispSection(r);
        var mapTypeName = ReadMapTypeNameSection(r);
        var rank = ReadRankSection(r);
        var elementItem1 = ReadElementItemSection(r, "ElementItem1");
        var elementItem2 = ReadElementItemSection(r, "ElementItem2");
        var exchange = ReadExchangeSection(r);
        var accessoryOptions = ReadAccessoryOptionSection(r);
        var accessoryEnchants = ReadAccessoryEnchantSection(r);

        return new ItemList(
            items,
            itemTap,
            coolTime,
            mapDisp,
            mapTypeName,
            rank,
            elementItem1,
            elementItem2,
            exchange,
            accessoryOptions,
            accessoryEnchants,
            new ItemListSectionCounts(
                itemCount,
                itemTap.Count,
                coolTime.Count,
                mapDisp.Count,
                mapTypeName.Count,
                rank.Count,
                elementItem1.Count,
                elementItem2.Count,
                exchange.Count,
                accessoryOptions.Count,
                accessoryEnchants.Count));
    }

    private static ItemAssetDTO ReadItemInfo(BinaryReader r)
    {
        var rec = r.ReadBytes(ItemInfoRecordSize);
        if (rec.Length != ItemInfoRecordSize)
            throw new EndOfStreamException($"Unexpected EOF while reading ItemList ItemInfo record ({ItemInfoRecordSize} bytes).");

        int cursor = 0;
        int itemId = ReadInt32(rec, ref cursor);
        string name = ReadUtf16(rec, ref cursor, 64);
        _ = ReadInt32(rec, ref cursor);
        _ = ReadUtf16(rec, ref cursor, 512);
        _ = ReadAscii(rec, ref cursor, 64);
        int @class = ReadUInt16(rec, ref cursor);
        _ = ReadUtf16(rec, ref cursor, 64);
        _ = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 2);
        int typeL = ReadUInt16(rec, ref cursor);
        int typeS = ReadUInt16(rec, ref cursor);
        int typeValue = ReadInt32(rec, ref cursor);
        int section = ReadInt32(rec, ref cursor);
        int sellType = ReadUInt16(rec, ref cursor);
        _ = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 2);
        _ = ReadUInt16(rec, ref cursor);
        short overlap = (short)ReadUInt16(rec, ref cursor);
        if (overlap <= 0)
            overlap = 1;
        byte tamerMinLevel = ToByte(ReadUInt16(rec, ref cursor));
        byte tamerMaxLevel = ToByte(ReadUInt16(rec, ref cursor));
        byte digimonMinLevel = ToByte(ReadUInt16(rec, ref cursor));
        byte digimonMaxLevel = ToByte(ReadUInt16(rec, ref cursor));
        _ = ReadUInt16(rec, ref cursor);
        _ = ReadUInt16(rec, ref cursor);
        _ = ReadUInt16(rec, ref cursor);
        _ = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 2);
        _ = ReadUInt16(rec, ref cursor);
        cursor = Align(cursor, 4);
        int eventPriceId = ReadInt32(rec, ref cursor);
        int eventPriceAmount = ReadUInt16(rec, ref cursor);
        int digicorePrice = ReadUInt16(rec, ref cursor);
        int scanPrice = ReadInt32(rec, ref cursor);
        int sellPrice = ReadInt32(rec, ref cursor);
        _ = ReadAscii(rec, ref cursor, 64);
        _ = ReadAscii(rec, ref cursor, 64);
        _ = ReadByte(rec, ref cursor);
        _ = ReadByte(rec, ref cursor);
        _ = ReadUInt16(rec, ref cursor);
        long skillCode = ReadInt32(rec, ref cursor);
        short applyValueMax = ReadByte(rec, ref cursor);
        short applyValueMin = ReadByte(rec, ref cursor);
        short applyElement = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 2);
        _ = ReadUInt16(rec, ref cursor);
        _ = ReadUInt16(rec, ref cursor);
        int boundType = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 4);
        _ = ReadInt32(rec, ref cursor);
        _ = ReadInt32(rec, ref cursor);
        _ = ReadInt32(rec, ref cursor);
        _ = ReadByte(rec, ref cursor);
        _ = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 4);
        _ = ReadInt32(rec, ref cursor);
        int useTimeType = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 4);
        int usageTimeMinutes = ReadInt32(rec, ref cursor);
        _ = ReadByte(rec, ref cursor);
        cursor = Align(cursor, 2);
        _ = ReadUInt16(rec, ref cursor);

        return new ItemAssetDTO
        {
            ItemId = itemId,
            Name = name,
            Class = @class,
            Type = typeL,
            TypeN = typeValue,
            ApplyValueMin = applyValueMin,
            ApplyValueMax = applyValueMax,
            ApplyElement = applyElement,
            Section = section,
            SellType = sellType,
            BoundType = boundType,
            UseTimeType = useTimeType,
            SkillCode = skillCode,
            TamerMinLevel = tamerMinLevel,
            TamerMaxLevel = tamerMaxLevel,
            DigimonMinLevel = digimonMinLevel,
            DigimonMaxLevel = digimonMaxLevel,
            SellPrice = sellPrice,
            ScanPrice = scanPrice,
            DigicorePrice = digicorePrice,
            EventPriceId = eventPriceId,
            EventPriceAmount = eventPriceAmount,
            UsageTimeMinutes = usageTimeMinutes,
            Overlap = overlap,
            Target = ResolveConsumeTarget(typeS),
        };
    }

    private static IReadOnlyList<ItemTapRecord> ReadItemTapSection(BinaryReader r)
    {
        int count = ReadSectionCount(r, "ItemTap");
        var records = new List<ItemTapRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, ItemTapRecordSize, "ItemTap");
            int cursor = 0;
            ushort type = ReadUInt16(rec, ref cursor);
            string name = ReadUtf16(rec, ref cursor, 32);
            records.Add(new ItemTapRecord(type, name));
        }
        return records;
    }

    private static IReadOnlyList<ItemCoolTimeRecord> ReadItemCoolTimeSection(BinaryReader r)
    {
        int count = ReadSectionCount(r, "ItemCoolTime");
        var records = new List<ItemCoolTimeRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, ItemCoolTimeRecordSize, "ItemCoolTime");
            int cursor = 0;
            uint itemType = ReadUInt32(rec, ref cursor);
            byte group = ReadByte(rec, ref cursor);
            cursor = Align(cursor, 8);
            double cooldownSeconds = ReadDouble(rec, ref cursor);
            records.Add(new ItemCoolTimeRecord(itemType, group, cooldownSeconds));
        }
        return records;
    }

    private static IReadOnlyList<ItemMapDisplayRecord> ReadMapDispSection(BinaryReader r)
    {
        int count = ReadSectionCount(r, "MapDisp");
        var records = new List<ItemMapDisplayRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, MapDispRecordSize, "MapDisp");
            int cursor = 0;
            uint itemType = ReadUInt32(rec, ref cursor);
            uint mapId = ReadUInt32(rec, ref cursor);
            records.Add(new ItemMapDisplayRecord(itemType, mapId));
        }
        return records;
    }

    private static IReadOnlyList<ItemMapTypeNameRecord> ReadMapTypeNameSection(BinaryReader r)
    {
        int count = ReadSectionCount(r, "MapTypeName");
        var records = new List<ItemMapTypeNameRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, MapTypeNameRecordSize, "MapTypeName");
            int cursor = 0;
            uint type = ReadUInt32(rec, ref cursor);
            string name = ReadUtf16(rec, ref cursor, 64);
            records.Add(new ItemMapTypeNameRecord(type, name));
        }
        return records;
    }

    private static IReadOnlyList<ItemRankRecord> ReadRankSection(BinaryReader r)
    {
        int count = ReadSectionCount(r, "Rank");
        var records = new List<ItemRankRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, RankRecordSize, "Rank");
            int cursor = 0;
            int itemId = ReadInt32(rec, ref cursor);
            ushort min = ReadUInt16(rec, ref cursor);
            ushort max = ReadUInt16(rec, ref cursor);
            records.Add(new ItemRankRecord(itemId, min, max));
        }
        return records;
    }

    private static IReadOnlyList<uint> ReadElementItemSection(BinaryReader r, string sectionName)
    {
        int count = ReadSectionCount(r, sectionName);
        var records = new List<uint>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, ElementItemRecordSize, sectionName);
            int cursor = 0;
            records.Add(ReadUInt32(rec, ref cursor));
        }
        return records;
    }

    private static IReadOnlyList<ItemExchangeRecord> ReadExchangeSection(BinaryReader r)
    {
        int count = ReadSectionCount(r, "Exchange");
        var records = new List<ItemExchangeRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = ReadFixedRecordBytes(r, ExchangeRecordSize, "Exchange");
            int cursor = 0;

            int index = ReadInt32(rec, ref cursor);
            uint itemType = ReadUInt32(rec, ref cursor);
            ushort unknown1 = ReadUInt16(rec, ref cursor);
            cursor = Align(cursor, 4);
            uint unknown2 = ReadUInt32(rec, ref cursor);
            int unknown3 = ReadInt32(rec, ref cursor);
            int unknown4 = ReadInt32(rec, ref cursor);
            int unknown5 = ReadInt32(rec, ref cursor);
            int unknown6 = ReadInt32(rec, ref cursor);
            ushort unknown7 = ReadUInt16(rec, ref cursor);
            ushort unknown8 = ReadUInt16(rec, ref cursor);
            ushort unknown9 = ReadUInt16(rec, ref cursor);
            ushort unknown10 = ReadUInt16(rec, ref cursor);
            ushort unknown11 = ReadUInt16(rec, ref cursor);

            records.Add(new ItemExchangeRecord(
                index,
                itemType,
                unknown1,
                unknown2,
                unknown3,
                unknown4,
                unknown5,
                unknown6,
                unknown7,
                unknown8,
                unknown9,
                unknown10,
                unknown11));
        }
        return records;
    }

    private static int ReadSectionCount(BinaryReader r, string sectionName)
    {
        int count = r.ReadInt32();
        if (count < 0)
            throw new InvalidDataException($"ItemList.bin section {sectionName} has negative count {count}.");

        return count;
    }

    private static byte[] ReadFixedRecordBytes(BinaryReader r, int recordSize, string sectionName)
    {
        var rec = r.ReadBytes(recordSize);
        if (rec.Length != recordSize)
            throw new EndOfStreamException($"Unexpected EOF while reading ItemList {sectionName} record ({recordSize} bytes).");
        return rec;
    }

    private static IReadOnlyList<ItemAccessoryOptionRecord> ReadAccessoryOptionSection(BinaryReader r)
    {
        int count = r.ReadInt32();
        if (count < 0)
            throw new InvalidDataException($"ItemList.bin section AccessoryOption has negative count {count}.");

        var records = new List<ItemAccessoryOptionRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = r.ReadBytes(AccessoryOptionRecordSize);
            if (rec.Length != AccessoryOptionRecordSize)
                throw new EndOfStreamException($"Unexpected EOF while reading ItemList AccessoryOption record ({AccessoryOptionRecordSize} bytes).");

            int cursor = 0;
            int index = ReadInt32(rec, ref cursor);
            uint itemType = ReadUInt32(rec, ref cursor);
            short minValue = ReadInt16(rec, ref cursor);
            short maxValue = ReadInt16(rec, ref cursor);

            var slots = new List<ItemAccessoryOptionSlot>(16);
            for (int slot = 0; slot < 16; slot++)
            {
                short optionType = ReadInt16(rec, ref cursor);
                cursor = Align(cursor, 4);
                uint optionMin = ReadUInt32(rec, ref cursor);
                uint optionMax = ReadUInt32(rec, ref cursor);
                slots.Add(new ItemAccessoryOptionSlot(optionType, optionMin, optionMax));
            }

            records.Add(new ItemAccessoryOptionRecord(index, itemType, minValue, maxValue, slots));
        }

        return records;
    }

    private static IReadOnlyList<ItemAccessoryEnchantRecord> ReadAccessoryEnchantSection(BinaryReader r)
    {
        int count = r.ReadInt32();
        if (count < 0)
            throw new InvalidDataException($"ItemList.bin section AccessoryEnchant has negative count {count}.");

        var records = new List<ItemAccessoryEnchantRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = r.ReadBytes(AccessoryEnchantRecordSize);
            if (rec.Length != AccessoryEnchantRecordSize)
                throw new EndOfStreamException($"Unexpected EOF while reading ItemList AccessoryEnchant record ({AccessoryEnchantRecordSize} bytes).");

            int cursor = 0;
            int index = ReadInt32(rec, ref cursor);
            uint itemType = ReadUInt32(rec, ref cursor);
            short minValue = ReadInt16(rec, ref cursor);
            short maxValue = ReadInt16(rec, ref cursor);
            records.Add(new ItemAccessoryEnchantRecord(index, itemType, minValue, maxValue));
        }

        return records;
    }

    private static ItemConsumeTargetEnum ResolveConsumeTarget(int typeS)
        => Enum.IsDefined(typeof(ItemConsumeTargetEnum), typeS)
            ? (ItemConsumeTargetEnum)typeS
            : ItemConsumeTargetEnum.Unavailable;

    private static byte ToByte(int value) => (byte)Math.Min(value, byte.MaxValue);

    private static int Align(int value, int alignment)
        => (value + (alignment - 1)) & ~(alignment - 1);

    private static int ComputeExchangeInfoSize()
    {
        int size = 0;
        size += sizeof(uint);
        size += sizeof(ushort);
        size = Align(size, 4);
        size += sizeof(uint);
        size += sizeof(int) * 4;
        size += sizeof(ushort) * 5;
        return Align(size, 4);
    }

    private static int ComputeAccessoryOptionInfoSize()
    {
        int size = sizeof(uint) + sizeof(short) + sizeof(short);
        for (int i = 0; i < 16; i++)
        {
            int opt = sizeof(short);
            opt = Align(opt, 4);
            opt += sizeof(uint) + sizeof(uint);
            size += opt;
        }

        return size;
    }

    private static int ComputeItemInfoRecordSize()
    {
        int size = 0;
        size += sizeof(int);               // itemId
        size += 64 * Utf16Bytes;          // name
        size += sizeof(int);              // icon
        size += 512 * Utf16Bytes;         // comment
        size += 64;                       // nif
        size += sizeof(ushort);           // class
        size += 64 * Utf16Bytes;          // type comment
        size += sizeof(byte);             // code tag
        size = Align(size, 2);
        size += sizeof(ushort);           // typeL
        size += sizeof(ushort);           // typeS
        size += sizeof(int);              // typeValue
        size += sizeof(int);              // section
        size += sizeof(ushort);           // sellType
        size += sizeof(byte);             // useMode
        size = Align(size, 2);
        size += sizeof(ushort);           // useTimeGroup
        size += sizeof(ushort);           // overlap
        size += sizeof(ushort) * 4;       // level mins/maxes
        size += sizeof(ushort);           // possess
        size += sizeof(ushort);           // equipSeries
        size += sizeof(ushort);           // useCharacter
        size += sizeof(byte);             // dummy
        size = Align(size, 2);
        size += sizeof(ushort);           // drop
        size = Align(size, 4);
        size += sizeof(int);              // event item type
        size += sizeof(ushort);           // event price
        size += sizeof(ushort);           // digicore price
        size += sizeof(int);              // scan price
        size += sizeof(int);              // sale
        size += 64;                       // model nif
        size += 64;                       // model effect
        size += sizeof(byte);             // model loop
        size += sizeof(byte);             // model shader
        size += sizeof(ushort);           // skill code type
        size += sizeof(int);              // skill code
        size += sizeof(byte);             // apply max
        size += sizeof(byte);             // apply min
        size += sizeof(byte);             // apply element
        size = Align(size, 2);
        size += sizeof(ushort);           // socket count
        size += sizeof(ushort);           // sound id
        size += sizeof(byte);             // belonging
        size = Align(size, 4);
        size += sizeof(int) * 3;          // quests
        size += sizeof(byte);             // digivice skill slot
        size += sizeof(byte);             // digivice chipset slot
        size = Align(size, 4);
        size += sizeof(int);              // quest require
        size += sizeof(byte);             // use time type
        size = Align(size, 4);
        size += sizeof(int);              // use time min
        size += sizeof(byte);             // use battle
        size = Align(size, 2);
        size += sizeof(ushort);           // do not use type
        size += sizeof(byte);             // ITEM_USE_TIME_PASS_FILE: s_bUseTimeType
        return Align(size, 4);
    }

    private static int ReadInt32(byte[] data, ref int cursor)
    {
        int value = BitConverter.ToInt32(data, cursor);
        cursor += sizeof(int);
        return value;
    }

    private static double ReadDouble(byte[] data, ref int cursor)
    {
        double value = BitConverter.ToDouble(data, cursor);
        cursor += sizeof(double);
        return value;
    }

    private static ushort ReadUInt16(byte[] data, ref int cursor)
    {
        ushort value = BitConverter.ToUInt16(data, cursor);
        cursor += sizeof(ushort);
        return value;
    }

    private static short ReadInt16(byte[] data, ref int cursor)
    {
        short value = BitConverter.ToInt16(data, cursor);
        cursor += sizeof(short);
        return value;
    }

    private static uint ReadUInt32(byte[] data, ref int cursor)
    {
        uint value = BitConverter.ToUInt32(data, cursor);
        cursor += sizeof(uint);
        return value;
    }

    private static byte ReadByte(byte[] data, ref int cursor)
    {
        byte value = data[cursor];
        cursor += sizeof(byte);
        return value;
    }

    private static string ReadUtf16(byte[] data, ref int cursor, int chars)
    {
        int bytes = chars * Utf16Bytes;
        ReadOnlySpan<byte> span = data.AsSpan(cursor, bytes);
        cursor += bytes;
        int end = 0;
        while (end + 1 < span.Length)
        {
            if (span[end] == 0 && span[end + 1] == 0) break;
            end += 2;
        }

        return Encoding.Unicode.GetString(span[..end]);
    }

    private static string ReadAscii(byte[] data, ref int cursor, int bytes)
    {
        ReadOnlySpan<byte> span = data.AsSpan(cursor, bytes);
        cursor += bytes;
        int end = span.IndexOf((byte)0);
        if (end < 0) end = span.Length;
        return Encoding.ASCII.GetString(span[..end]);
    }
}
