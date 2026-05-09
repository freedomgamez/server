namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

public sealed class CharCreateTable
{
    public IReadOnlyList<CharCreateTamerSlot> Tamers { get; }
    public IReadOnlyList<CharCreateStarterDigimon> Digimon { get; }

    public CharCreateTable(
        IReadOnlyList<CharCreateTamerSlot> tamers,
        IReadOnlyList<CharCreateStarterDigimon> digimon)
    {
        Tamers = tamers;
        Digimon = digimon;
    }

    /// <summary>
    /// Find a tamer entry by model. The caller still has to check <see cref="CharCreateTamerSlot.Enable"/>
    /// before accepting it for character creation — entries with <c>Show==true, Enable==false</c>
    /// appear in some UIs but are not meant to be picked at create time.
    /// </summary>
    public CharCreateTamerSlot? FindTamer(int model)
    {
        for (int i = 0; i < Tamers.Count; i++)
            if (Tamers[i].Model == model) return Tamers[i];
        return null;
    }

    public CharCreateStarterDigimon? FindStarterDigimon(int model)
    {
        for (int i = 0; i < Digimon.Count; i++)
            if (Digimon[i].Model == model) return Digimon[i];
        return null;
    }
}

/// <summary>
/// One entry of <c>CharCreateTable.bin</c>'s tamer section. Mirrors <c>sTamerCreateInfo</c> in
/// <c>LibProj/CsFileTable/CharacterCreateListInfo.h</c>.
/// </summary>
/// <param name="Model">Tamer model ID (e.g. 80001 for Marcus).</param>
/// <param name="Show">Whether the entry appears in the character-create list UI.</param>
/// <param name="Enable">Whether the entry can actually be selected by the player.
/// In v487 only 4 of 12 tamers are Enable=true (the Savers cast: Marcus/Touma/Yoshi/Ikuto).</param>
/// <param name="SeasonType">UI grouping (1 = Savers cast, 2 = others).</param>
/// <param name="VoiceFile">Voice-clip filename used by the UI when the entry is hovered.</param>
/// <param name="IconIdx">UI icon slot index.</param>
/// <param name="CostumeIds">Costume item IDs the tamer is dressed in by default.</param>
public sealed record CharCreateTamerSlot(
    int Model,
    bool Show,
    bool Enable,
    int SeasonType,
    string VoiceFile,
    int IconIdx,
    IReadOnlyList<int> CostumeIds);

/// <summary>
/// One entry of <c>CharCreateTable.bin</c>'s digimon section. Mirrors <c>sDigimonCreateInfo</c>.
/// </summary>
/// <param name="Model">Digimon model ID (e.g. 31001 for Agumon).</param>
/// <param name="Show">Whether the entry appears in the character-create digimon picker UI.</param>
/// <param name="Enable">Whether the entry can actually be picked. In v487 only 4 of 86 are
/// Enable=true: 31001 (Agumon), 31002 (Lalamon), 31003 (Gaomon), 31004 (Falcomon).</param>
/// <param name="VoiceFile">Voice-clip filename for hover preview.</param>
public sealed record CharCreateStarterDigimon(
    int Model,
    bool Show,
    bool Enable,
    string VoiceFile);
