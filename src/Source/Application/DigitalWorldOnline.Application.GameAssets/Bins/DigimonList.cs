namespace DigitalWorldOnline.Application.GameAssets.Bins;

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
/// v487's <c>Digimon_List.bin</c>. The bin's <c>s_eBaseNatureType</c> field — the original
/// DMO term — is what the server's DTOs renamed to <c>Element</c>; the loader maps that
/// directly. See <c>project_bin_static_data.md</c> in memory for the cross-reference.
///
/// <para>
/// Client-only fields (s_szForm, s_cSoundDirName, s_szEvoEffectDir, s_fSelectScale,
/// s_fWakkLen/RunLen/ARunLen, s_dwCharSize) are intentionally not exposed — the server
/// renders nothing and animates nothing. <c>s_eBaseNatureTypes[3]</c> (alt-nature list) is
/// also unread until a consumer requests it; the primary <c>s_eBaseNatureType</c> is
/// already exposed as <see cref="Element"/>.
/// </para>
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
    ushort HitRate,
    IReadOnlyList<DigimonSkillSlot> Skills,
    int Rank);

/// <summary>
/// One slot of <c>CsDigimon::sINFO::s_Skill[4]</c> — the per-digimon skill loadout the
/// server resolves through <c>DigimonSkillAssetsQuery</c>. Slot index 0..3 maps to the
/// client's F1..F4 hotbar (see <c>DigimonSkillAssetDTO.Slot</c>).
/// </summary>
public sealed record DigimonSkillSlot(
    int SkillId,
    int RequiredPrevSkillLevel);
