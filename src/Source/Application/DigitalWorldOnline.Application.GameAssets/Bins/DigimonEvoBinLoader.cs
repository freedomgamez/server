namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses <c>DigimonEvo.bin</c> per <c>CDigimonEvolution::SaveBin</c> in
/// <c>LibProj/CsFileTable/CDigimonEvolution.cpp</c>. Game.Host loads ALL trees (140 in v487);
/// CharacterAssets filters to the 4 starters.
///
///   [int digimonCount = 140]
///   per digimon:
///     CDigimonEvolveInfo::sINFO  (8 bytes: DWORD digimonID, DWORD battleEvoType)
///     [int evoCount]
///     evoCount × CDigimonEvolveObj  (328 bytes each)
///   [int battleEvoCount = 5]
///   5 × sBATTLE_EVO_INFO  (50 bytes each — not consumed)
///
/// CDigimonEvolveObj's relevant fields (offsets within the 328-byte record):
///   0    DWORD  m_dwID
///   4    USHORT m_nEvoSlot
///   6    USHORT pad
///   8    SEvolutionInfo[9]    (72 bytes — outgoing evolution targets indexed by UI slot; client
///                              QuickEvol passes the index back as <c>nEvoIndex</c> on click)
///   88   USHORT m_nEnableSlot
///   90   USHORT m_nOpenQualification
///   92   USHORT m_nOpenLevel
///   94   USHORT m_nOpenQuest
///   100  USHORT m_nUseItem
///   102  USHORT m_nUseItemNum
///   ...rest of fields not used by server DTO and is skipped.
/// </summary>
public sealed class DigimonEvoBinLoader
{
    private const string FileName = "DigimonEvo.bin";
    private const int EvolveObjSize = 328;

    private DigimonEvo? _data;

    public DigimonEvo Data => _data ?? throw new InvalidOperationException(
        $"{nameof(DigimonEvoBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public DigimonEvo Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static DigimonEvo Parse(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DigimonEvoEntry>(count);

        for (int i = 0; i < count; i++)
        {
            uint digimonId = r.ReadUInt32();
            uint battleEvoType = r.ReadUInt32();
            int evoCount = r.ReadInt32();

            var lines = new List<DigimonEvoLine>(evoCount);
            for (int k = 0; k < evoCount; k++)
            {
                var rec = r.ReadBytes(EvolveObjSize);
                lines.Add(ReadEvolveObj(rec));
            }

            // Sort by EvoSlot so consumers see Rookie(1) → Champion(2) → Ultimate(3) → Mega(4)
            // → ... in order. Server's DigimonModel.AddEvolutions auto-unlocks the first
            // entries, so this ordering matters.
            lines.Sort((a, b) => a.EvoSlot.CompareTo(b.EvoSlot));
            map[(int)digimonId] = new DigimonEvoEntry((int)digimonId, (int)battleEvoType, lines);
        }

        // Trailing battle-evo-info section is intentionally not consumed.
        return new DigimonEvo(map);
    }

    private static DigimonEvoLine ReadEvolveObj(ReadOnlySpan<byte> rec)
    {
        uint type = BitConverter.ToUInt32(rec[0..4]);
        ushort evoSlot = BitConverter.ToUInt16(rec[4..6]);
        // 8..80 = SEvolutionInfo[9] — outgoing evolution targets indexed by UI slot.
        var stages = new DigimonEvoStage[9];
        for (int i = 0; i < 9; i++)
        {
            int o = 8 + i * 8;
            int slot = BitConverter.ToInt32(rec[o..(o + 4)]);
            int target = BitConverter.ToInt32(rec[(o + 4)..(o + 8)]);
            stages[i] = new DigimonEvoStage(slot, target);
        }
        ushort enableSlot = BitConverter.ToUInt16(rec[88..90]);
        ushort openQualification = BitConverter.ToUInt16(rec[90..92]);
        ushort openLevel = BitConverter.ToUInt16(rec[92..94]);
        ushort openQuest = BitConverter.ToUInt16(rec[94..96]);
        ushort useItem = BitConverter.ToUInt16(rec[100..102]);
        ushort useItemNum = BitConverter.ToUInt16(rec[102..104]);
        return new DigimonEvoLine((int)type, evoSlot, enableSlot, openQualification, openLevel, openQuest, useItem, useItemNum, stages);
    }
}
