using System.Collections.Generic;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models.Asset;

namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>Nature.bin</c> per <c>NatureMng::SaveBin</c> +
/// <c>_SaveBinBaseElement</c> in <c>LibProj/CsFileTable/NatureMng.cpp</c>.
///
/// Two concatenated sections in the file:
/// <list type="bullet">
/// <item>§1 — Element-vs-element percent-delta matrix.
///   <c>[i32 count][count × {USHORT id; short element[11]}]</c>.  v487 has 11 rows
///   (NT_ICE..NT_NONE).  Row's <c>id</c> is the attacker's <c>nsCsDigimonTable::eNATURE_TYPE</c>;
///   each <c>element[j]</c> is the % delta when hitting target type <c>NT_ICE + j</c>.
/// </item>
/// <item>§2 — Attribute (Data/Vaccine/Virus) table.  Nested counts:
///   <c>[i32 cmpTypeCount][per cmpType: i32 cmpType; i32 baseCount; per base: i32 baseType;
///   i32 vecCount; vec × {i32 targetType; i32 point}]</c>.
///   cmpType: 0=Attack roll, 1=Defence roll.  baseType/targetType use
///   <c>nsBaseElement::eElementType</c> (0..4 = No/Da/Va/Vi/Un), which is shifted +1
///   from server's <see cref="DigimonAttributeEnum"/> (1..5).
/// </item>
/// </list>
///
/// Also re-used by <c>NewElementBinLoader</c> via <see cref="LoadFromPath"/> — identical
/// byte layout in v487.
/// </summary>
public sealed class NatureBinLoader
{
    private const string FileName = "Nature.bin";
    private const int NaturesPerRow = 11;             // CsNature::eTYPE::MaxCount in v487
    private const int NatureRecordSize = 2 + NaturesPerRow * 2;   // USHORT + 11 × short = 24 B

    private NatureData? _data;

    public NatureData Data => _data ?? throw new InvalidOperationException(
        $"{nameof(NatureBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public NatureData Load()
    {
        if (_data != null) return _data;
        _data = LoadFromPath(System.IO.Path.Combine(BinPath.ResolveDirectory(), FileName));
        return _data;
    }

    public static NatureData LoadFromPath(string path)
    {
        var bytes = System.IO.File.ReadAllBytes(path);
        return Parse(bytes);
    }

    public static NatureData Parse(ReadOnlySpan<byte> data)
    {
        // ─── §1 Element matrix ───────────────────────────────────────────
        int cursor = 0;
        int natureCount = BitConverter.ToInt32(data.Slice(cursor, 4));
        cursor += 4;

        var elementByAttacker = new Dictionary<DigimonElementEnum, IReadOnlyDictionary<DigimonElementEnum, short>>();
        for (int i = 0; i < natureCount; i++)
        {
            int recOffset = cursor + i * NatureRecordSize;
            ushort rawId = BitConverter.ToUInt16(data.Slice(recOffset, 2));
            var attacker = MapNatureId(rawId);

            var row = new Dictionary<DigimonElementEnum, short>(NaturesPerRow);
            for (int j = 0; j < NaturesPerRow; j++)
            {
                short delta = BitConverter.ToInt16(data.Slice(recOffset + 2 + j * 2, 2));
                // Column index j → target id = NT_ICE + j (NaturesPerRow = NT_ICE..NT_NONE inclusive).
                var target = MapNatureId((ushort)(16 + j));
                row[target] = delta;
            }
            elementByAttacker[attacker] = row;
        }
        cursor += natureCount * NatureRecordSize;

        // ─── §2 BaseElement (attribute compare table) ────────────────────
        int cmpTypeCount = BitConverter.ToInt32(data.Slice(cursor, 4));
        cursor += 4;

        var attributeByCmp = new Dictionary<AttributeCompare, IReadOnlyDictionary<DigimonAttributeEnum, IReadOnlyDictionary<DigimonAttributeEnum, int>>>();
        for (int c = 0; c < cmpTypeCount; c++)
        {
            int cmpType = BitConverter.ToInt32(data.Slice(cursor, 4));
            cursor += 4;
            int baseCount = BitConverter.ToInt32(data.Slice(cursor, 4));
            cursor += 4;

            var bySource = new Dictionary<DigimonAttributeEnum, IReadOnlyDictionary<DigimonAttributeEnum, int>>();
            for (int b = 0; b < baseCount; b++)
            {
                int baseType = BitConverter.ToInt32(data.Slice(cursor, 4));
                cursor += 4;
                int vecCount = BitConverter.ToInt32(data.Slice(cursor, 4));
                cursor += 4;

                var byTarget = new Dictionary<DigimonAttributeEnum, int>(vecCount);
                for (int v = 0; v < vecCount; v++)
                {
                    int targetType = BitConverter.ToInt32(data.Slice(cursor, 4));
                    cursor += 4;
                    int point = BitConverter.ToInt32(data.Slice(cursor, 4));
                    cursor += 4;
                    byTarget[MapBaseAttribute(targetType)] = point;
                }
                bySource[MapBaseAttribute(baseType)] = byTarget;
            }
            attributeByCmp[(AttributeCompare)cmpType] = bySource;
        }

        return new NatureData(elementByAttacker, attributeByCmp);
    }

    /// <summary>
    /// Bin's <c>nsCsDigimonTable::eNATURE_TYPE</c> (NT_ICE=16..NT_NONE=26) → server's
    /// <see cref="DigimonElementEnum"/>.  NT_NONE=26 maps to Neutral=0 since the server enum
    /// doesn't carry a distinct "None" value — the v487 bin's NT_NONE row is all zeros anyway.
    /// </summary>
    private static DigimonElementEnum MapNatureId(ushort raw) => raw switch
    {
        16 => DigimonElementEnum.Ice,
        17 => DigimonElementEnum.Water,
        18 => DigimonElementEnum.Fire,
        19 => DigimonElementEnum.Land,
        20 => DigimonElementEnum.Wind,
        21 => DigimonElementEnum.Wood,
        22 => DigimonElementEnum.Light,
        23 => DigimonElementEnum.Dark,
        24 => DigimonElementEnum.Thunder,
        25 => DigimonElementEnum.Steel,
        _ => DigimonElementEnum.Neutral,    // 0, 26 (NT_NONE), and unexpected values
    };

    /// <summary>
    /// Bin's <c>nsBaseElement::eElementType</c> (eNo=0..eUn=4) → server's
    /// <see cref="DigimonAttributeEnum"/> (None=1..Unknown=5).  Off-by-one shift.
    /// </summary>
    private static DigimonAttributeEnum MapBaseAttribute(int raw) => raw switch
    {
        0 => DigimonAttributeEnum.None,
        1 => DigimonAttributeEnum.Data,
        2 => DigimonAttributeEnum.Vaccine,
        3 => DigimonAttributeEnum.Virus,
        4 => DigimonAttributeEnum.Unknown,
        _ => DigimonAttributeEnum.None,
    };
}
