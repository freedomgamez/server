using System.Collections.Generic;
using DigitalWorldOnline.Commons.Enums.ClientEnums;

namespace DigitalWorldOnline.Commons.Models.Asset;

/// <summary>
/// Parsed Nature.bin (also New_Element.bin — identical layout).  Two tables:
/// <list type="bullet">
/// <item><see cref="ElementDeltaPercent"/>: signed percent delta applied to the
///   base damage when an attacker of NatureType X hits a target of NatureType Y.
///   v487 v487 bin values are -25 / 0 / +25.  Formula: <c>dmg = base * (100 + delta) / 100</c>.</item>
/// <item><see cref="AttributePoint"/>: absolute percentage (e.g. 90/100/110) for the
///   Data/Vaccine/Virus triangle, split by attack-roll vs defence-roll.</item>
/// </list>
/// </summary>
public sealed class NatureData
{
    /// <summary>
    /// [attacker NatureType] × [target NatureType] → percent delta (-25, 0, +25 in v487).
    /// Indexed by <see cref="DigimonElementEnum"/> directly.
    /// </summary>
    public IReadOnlyDictionary<DigimonElementEnum, IReadOnlyDictionary<DigimonElementEnum, short>> ElementDeltaPercent { get; }

    /// <summary>
    /// [eAtt / eDef] × [attacker attribute] × [target attribute] → absolute percent
    /// multiplier (90 / 100 / 110 in v487 — keyed against <c>ELEMENT_EQAUL = 100</c>).
    /// </summary>
    public IReadOnlyDictionary<AttributeCompare, IReadOnlyDictionary<DigimonAttributeEnum, IReadOnlyDictionary<DigimonAttributeEnum, int>>> AttributePoint { get; }

    public NatureData(
        IReadOnlyDictionary<DigimonElementEnum, IReadOnlyDictionary<DigimonElementEnum, short>> elementDelta,
        IReadOnlyDictionary<AttributeCompare, IReadOnlyDictionary<DigimonAttributeEnum, IReadOnlyDictionary<DigimonAttributeEnum, int>>> attributePoint)
    {
        ElementDeltaPercent = elementDelta;
        AttributePoint = attributePoint;
    }

    /// <summary>
    /// Element percent delta for one (attacker, target) pair.  Returns 0 (no change) if
    /// either side is Neutral or the pair isn't in the matrix.
    /// </summary>
    public short GetElementDelta(DigimonElementEnum attacker, DigimonElementEnum target)
    {
        if (!ElementDeltaPercent.TryGetValue(attacker, out var row)) return 0;
        return row.TryGetValue(target, out var v) ? v : (short)0;
    }

    /// <summary>
    /// Attribute multiplier (absolute percent, default 100 = no change) for one
    /// (attacker, target) attribute pair in the selected damage roll.
    /// </summary>
    public int GetAttributePoint(AttributeCompare cmp, DigimonAttributeEnum src, DigimonAttributeEnum target)
    {
        if (!AttributePoint.TryGetValue(cmp, out var byCmp)) return 100;
        if (!byCmp.TryGetValue(src, out var bySrc)) return 100;
        return bySrc.TryGetValue(target, out var v) ? v : 100;
    }
}

/// <summary>Which damage roll the attribute table is being applied to.</summary>
public enum AttributeCompare
{
    /// <summary>Attacker's roll (eAtt in BaseElement.h).</summary>
    Attack = 0,
    /// <summary>Defender's roll (eDef in BaseElement.h).</summary>
    Defence = 1,
}
