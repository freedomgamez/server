namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

public sealed class DigimonList
{
    public IReadOnlyDictionary<int, DigimonListEntry> ByType { get; }

    public DigimonList(IReadOnlyDictionary<int, DigimonListEntry> byType)
    {
        ByType = byType;
    }

    public DigimonListEntry? FindByType(int type)
        => ByType.TryGetValue(type, out var e) ? e : null;
}

/// <summary>
/// One digimon's static descriptive + base-stat info, as stored in the single section of
/// v487's <c>Digimon_List.bin</c>. Field names here follow the server's DTO conventions —
/// note that the bin's <c>s_eBaseNatureType</c> (the original DMO term) is what the server
/// renamed to <c>Element</c>; see <c>project_bin_static_data.md</c> for the cross-reference.
/// </summary>
public sealed record DigimonListEntry(
    int Type,
    int Model,
    string Name,
    byte BaseLevel,
    byte ScaleType,
    int EvolutionType,
    int Attribute,
    int Element,
    int Family1,
    int Family2,
    int Family3,
    ushort HP,
    ushort DS,
    ushort DefPower,
    ushort Evasion,
    ushort MoveSpeed,
    ushort CriticalRate,
    ushort AttPower,
    ushort AttSpeed,
    ushort AttRange,
    ushort HitRate);
