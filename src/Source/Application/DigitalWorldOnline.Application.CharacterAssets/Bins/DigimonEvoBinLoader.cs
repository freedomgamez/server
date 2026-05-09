namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

/// <summary>
/// Parses <c>DigimonEvo.bin</c> per <c>CDigimonEvolution::SaveBin</c> in
/// <c>LibProj/CsFileTable/CDigimonEvolution.cpp</c>:
///
///   [int digimonCount]
///   per digimon:
///     CDigimonEvolveInfo::sINFO  (8 bytes: DWORD digimonID, DWORD battleEvoType)
///     [int evoCount]
///     evoCount × CDigimonEvolveObj  (328 bytes each — measured by file probe; default
///         Visual C++ alignment, contains a <c>double</c> so struct alignment is 8)
///   [int battleEvoCount = 5]
///   5 × sBATTLE_EVO_INFO  (50 bytes each)
///
/// CDigimonEvolveObj's important fields for our purpose (offsets relative to the 328-byte
/// record):
///   0    DWORD  m_dwID                 (the digimon type this slot evolves you into)
///   4    USHORT m_nEvoSlot             (1 = Rookie, 2 = Champion, ...)
///   6    USHORT pad
///   8    SEvolutionInfo[9]             (72 bytes; outgoing evolution targets — not needed
///                                       since each target appears as its own evolveObj
///                                       with that slot)
///   88   USHORT m_nEnableSlot
///   90   USHORT m_nOpenQualification
///   92   USHORT m_nOpenLevel           (partner level to unlock)
///   94   USHORT m_nOpenQuest
///   96   USHORT m_nOpenItemTypeS
///   98   USHORT m_nOpenItemNum
///   100  USHORT m_nUseItem
///   102  USHORT m_nUseItemNum
///   ...everything after that (intimacy, crest, evo cards, hatch animation params,
///   effect strings, jogress fields, etc.) is not used by the server DTO and is skipped.
/// </summary>
public sealed class DigimonEvoBinLoader
{
    private const string FileName = "DigimonEvo.bin";
    private const int EvolveObjSize = 328;

    private DigimonEvo? _data;

    public DigimonEvo Data => _data ?? throw new InvalidOperationException(
        $"{nameof(DigimonEvoBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    /// <summary>
    /// Load DigimonEvo.bin. Pass <paramref name="typeFilter"/> to keep only the listed
    /// base digimon types (Character.Host should pass the 4 selectable starter IDs from
    /// CharCreateTable.bin).
    /// </summary>
    public DigimonEvo Load(IReadOnlySet<int>? typeFilter = null)
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader, typeFilter);
        return _data;
    }

    public static DigimonEvo Parse(BinaryReader r, IReadOnlySet<int>? typeFilter = null)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DigimonEvoEntry>(typeFilter?.Count ?? count);

        for (int i = 0; i < count; i++)
        {
            uint digimonId = r.ReadUInt32();
            uint battleEvoType = r.ReadUInt32();
            int evoCount = r.ReadInt32();

            bool keep = typeFilter is null || typeFilter.Contains((int)digimonId);

            if (!keep)
            {
                // Skip evoCount × 328 bytes without allocating.
                r.BaseStream.Seek(evoCount * EvolveObjSize, SeekOrigin.Current);
                continue;
            }

            var lines = new List<DigimonEvoLine>(evoCount);
            for (int k = 0; k < evoCount; k++)
            {
                var rec = r.ReadBytes(EvolveObjSize);
                lines.Add(ReadEvolveObj(rec));
            }

            // Sort by EvoSlot so consumers see Rookie(1) → Champion(2) → Ultimate(3) → Mega(4)
            // → ... in that order. The server's DigimonModel.AddEvolutions auto-unlocks the
            // first 3 entries, so this ordering matters for which evolutions are immediately
            // available at character creation.
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
        // skip 2 bytes of pad + 72 bytes of m_nEvolutionList[9]
        ushort openLevel = BitConverter.ToUInt16(rec[92..94]);
        ushort openQuest = BitConverter.ToUInt16(rec[94..96]);
        ushort useItem = BitConverter.ToUInt16(rec[100..102]);
        ushort useItemNum = BitConverter.ToUInt16(rec[102..104]);
        return new DigimonEvoLine((int)type, evoSlot, openLevel, openQuest, useItem, useItemNum);
    }
}
