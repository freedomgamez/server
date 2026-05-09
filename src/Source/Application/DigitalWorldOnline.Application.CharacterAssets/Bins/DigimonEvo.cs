namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

public sealed class DigimonEvo
{
    public IReadOnlyDictionary<int, DigimonEvoEntry> ByType { get; }

    public DigimonEvo(IReadOnlyDictionary<int, DigimonEvoEntry> byType)
    {
        ByType = byType;
    }

    public DigimonEvoEntry? FindByType(int type)
        => ByType.TryGetValue(type, out var e) ? e : null;
}

/// <summary>
/// One digimon's evolution tree, as stored in <c>DigimonEvo.bin</c>. Mirrors a
/// <c>CDigimonEvolveInfo</c> + its inner <c>m_Map</c> of <c>CDigimonEvolveObj</c> entries
/// from <c>LibProj/CsFileTable/CDigimonEvolution.h</c>.
/// </summary>
/// <param name="Type">The base digimon type (e.g. 31001 for Agumon).</param>
/// <param name="BattleEvoType">Battle-evolution group reference (1 for the Savers cast).</param>
/// <param name="Lines">All evolution forms belonging to this digimon's tree, sorted by
/// <c>EvoSlot</c> so slot=1 (Rookie/base) comes first, slot=2 (Champion) next, etc.</param>
public sealed record DigimonEvoEntry(
    int Type,
    int BattleEvoType,
    IReadOnlyList<DigimonEvoLine> Lines);

/// <summary>
/// One evolution form ("evolveObj"). The DMO server's <c>DigimonModel.AddEvolutions</c>
/// only reads <see cref="Type"/>; the other fields are exposed for completeness so the
/// handler can populate the rest of the DTO if a future caller wants unlock metadata.
/// </summary>
/// <param name="Type">The digimon type this slot evolves you into.</param>
/// <param name="EvoSlot">Slot number (1 = base/Rookie, 2 = Champion, etc).</param>
/// <param name="EnableSlot">0 = closed/unavailable; non-zero = enabled. Server should refuse unlocks for closed slots.</param>
/// <param name="OpenQualification">NEED_QUALITICATION enum: 0=NONE, 1=PARTNERMON, 2=ROYAL_KNIGHT, 3=XAI_SYSTEM.</param>
/// <param name="OpenLevel">Partner level required to unlock this slot.</param>
/// <param name="OpenQuest">Quest required to unlock.</param>
/// <param name="UseItem">Item consumed to evolve.</param>
/// <param name="UseItemNum">Item count consumed.</param>
public sealed record DigimonEvoLine(
    int Type,
    ushort EvoSlot,
    ushort EnableSlot,
    ushort OpenQualification,
    ushort OpenLevel,
    ushort OpenQuest,
    ushort UseItem,
    ushort UseItemNum);
