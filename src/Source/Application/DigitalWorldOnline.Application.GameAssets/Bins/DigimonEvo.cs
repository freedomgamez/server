namespace DigitalWorldOnline.Application.GameAssets.Bins;

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
public sealed record DigimonEvoEntry(
    int Type,
    int BattleEvoType,
    IReadOnlyList<DigimonEvoLine> Lines);

/// <summary>
/// One evolution form ("evolveObj"). Mirrors fields of <c>CDigimonEvolveObj</c>.
/// <para>
/// <see cref="EnableSlot"/>: when 0, the slot is closed (<c>eClosed</c>) and unavailable —
/// the client skips it entirely (`DigimonUser.cpp:2355`, `:2821`). Server should refuse
/// evolution-unlock requests for such slots.
/// </para>
/// <para>
/// <see cref="OpenQualification"/>: NEED_QUALITICATION enum — 0=NONE, 1=PARTNERMON,
/// 2=ROYAL_KNIGHT, 3=XAI_SYSTEM (last only with SDM_TAMER_XGUAGE_20180628). Drives which
/// unlock UI / requirements path applies. Server may enforce or merely log.
/// </para>
/// </summary>
public sealed record DigimonEvoLine(
    int Type,
    ushort EvoSlot,
    ushort EnableSlot,
    ushort OpenQualification,
    ushort OpenLevel,
    ushort OpenQuest,
    ushort UseItem,
    ushort UseItemNum,
    IReadOnlyList<DigimonEvoStage> Stages);

/// <summary>
/// One outgoing evolution target on a form's <c>m_nEvolutionList[9]</c>. The client's
/// QuickEvol UI iterates this list by position (0..8) — when the player clicks slot
/// <c>i</c>, the client sends <c>SendEvolution(uid, i)</c> and the server uses <c>i</c>
/// as the index into this same list to resolve the target form. Empty entries
/// (<c>nSlot == INVAIDE</c>, sentinel) keep the index alignment.
/// </summary>
public sealed record DigimonEvoStage(int Slot, int TargetType);
