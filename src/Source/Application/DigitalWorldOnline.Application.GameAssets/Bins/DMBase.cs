namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Full v487 DMBase.bin contents: 11 mandatory sections (sections 10a/10b under
/// <c>#ifdef GUILD_RENEWAL</c> are not present in v487). Most sections don't yet have a
/// server-side consumer — they're parsed and held in memory so future feature work can
/// read them without touching the bin format again. String fields throughout the bin
/// are client-only (UI labels) and intentionally not preserved here; see
/// <c>reference_bin_string_framing.md</c> in memory.
/// </summary>
public sealed class DMBase
{
    public IReadOnlyDictionary<int, DMBaseStatRecord> TamerStats { get; }
    public IReadOnlyDictionary<int, DMBaseStatRecord> DigimonStats { get; }
    public IReadOnlyDictionary<int, DMBaseMapInfo> MapInfo { get; }
    public IReadOnlyDictionary<int, IReadOnlyList<int>> JumpBusterDestinations { get; }
    public DMBasePartyConfig Party { get; }
    public IReadOnlyDictionary<int, DMBaseGuildLevelConfig> GuildLevels { get; }
    public DMBaseLimit Limit { get; }
    public DMBasePersonStore PersonStore { get; }
    public IReadOnlyDictionary<int, DMBasePlayPenalty> PlayPenalty { get; }
    public IReadOnlyDictionary<int, int> EvolutionStageApply { get; }
    public IReadOnlyDictionary<int, DMBaseDigimonEvoMaxLevel> DigimonEvoMaxLevel { get; }
    public IReadOnlyDictionary<int, DMBaseDskillOpenExpansion> DskillOpenExpansion { get; }

    public DMBase(
        IReadOnlyDictionary<int, DMBaseStatRecord> tamerStats,
        IReadOnlyDictionary<int, DMBaseStatRecord> digimonStats,
        IReadOnlyDictionary<int, DMBaseMapInfo> mapInfo,
        IReadOnlyDictionary<int, IReadOnlyList<int>> jumpBusterDestinations,
        DMBasePartyConfig party,
        IReadOnlyDictionary<int, DMBaseGuildLevelConfig> guildLevels,
        DMBaseLimit limit,
        DMBasePersonStore personStore,
        IReadOnlyDictionary<int, DMBasePlayPenalty> playPenalty,
        IReadOnlyDictionary<int, int> evolutionStageApply,
        IReadOnlyDictionary<int, DMBaseDigimonEvoMaxLevel> digimonEvoMaxLevel,
        IReadOnlyDictionary<int, DMBaseDskillOpenExpansion> dskillOpenExpansion)
    {
        TamerStats = tamerStats;
        DigimonStats = digimonStats;
        MapInfo = mapInfo;
        JumpBusterDestinations = jumpBusterDestinations;
        Party = party;
        GuildLevels = guildLevels;
        Limit = limit;
        PersonStore = personStore;
        PlayPenalty = playPenalty;
        EvolutionStageApply = evolutionStageApply;
        DigimonEvoMaxLevel = digimonEvoMaxLevel;
        DskillOpenExpansion = dskillOpenExpansion;
    }

    // Section 1 ID encoding: (tamerModel - 80000) * 1000 + level
    public static int TamerKey(int tamerModel, int level) => (tamerModel - 80000) * 1000 + level;

    // Section 2 ID encoding: digimonRank * 1000 + level
    public static int DigimonRankKey(int rank, int level) => rank * 1000 + level;
}

/// <summary>Sections 1 and 2: per-level stat row (CsBase::sINFO, 40 bytes on disk).</summary>
public sealed record DMBaseStatRecord(
    int Id,
    ushort Level,
    long Exp,
    int HP,
    int DS,
    ushort MoveSpeed,
    ushort Defence,
    ushort Evasion,
    ushort Critical,
    ushort Attack,
    ushort HitRate);

/// <summary>Section 3: per-map config (CsBaseMapInfo::sINFO, 12 bytes).</summary>
public sealed record DMBaseMapInfo(int MapId, uint ShoutSec, bool EnableCheckMacro);

/// <summary>Section 5: party config (sPARTY, single struct, 4 bytes).</summary>
public sealed record DMBasePartyConfig(float PartyDist);

/// <summary>Section 6: per-level guild progression (sGuild, 44 bytes).</summary>
public sealed record DMBaseGuildLevelConfig(
    int Level,
    uint Fame,
    int ItemNo1, int ItemCount1,
    int ItemNo2, int ItemCount2,
    int MasterLevel,
    int NeedPerson,
    int MaxGuildPerson,
    int IncMember,
    int MaxGuild2Master);

/// <summary>
/// Section 7: server-wide limits (sLIMIT, 16 bytes in v487 = 4 SHORTs + 2 u32, with
/// NEW_SHARESTASH defined and CROSSWARS_SYSTEM not).
///
/// <b>Naming caveat — <see cref="MaxTacticsHouse"/> is actually the DigimonArchive cap.</b>
/// The "TacticsHouse" feature was the original DMO digimon-storage system; in v487 it was
/// replaced by DigimonArchive (<c>m_pDigimonArchiveAdapt</c> in client) but the bin's field
/// name kept the legacy "MaxTacticsHouse" label. Verified by client `DataMng.cpp:1542`:
/// <c>if(m_pDigimonArchiveAdapt-&gt;GetOpenedArchiveCnt() &gt;= nTHouseCount)</c> where
/// <c>nTHouseCount = bin.MaxTacticsHouse</c>. Server-side, this field caps
/// <c>CharacterDigimonArchiveModel.Slots</c>.
/// </summary>
public sealed record DMBaseLimit(
    short MaxTacticsHouse,
    short MaxWareHouse,
    short UnionStore,
    short MaxShareStash,
    uint ConsumeXG,
    uint ChargeXG);

/// <summary>Section 8: personal-store config (sPERSON_STORE).</summary>
public sealed record DMBasePersonStore(
    float PersonCharge,
    float EmploymentCharge,
    float StoreDist,
    IReadOnlyList<DMBasePersonStoreObject> Objects);

/// <summary>One person-store object (sOBJECT_INFO, 144 bytes — CONSIGNMENT_BALLOON_CASH defined; the trailing 128-byte filename is skipped).</summary>
public sealed record DMBasePersonStoreObject(int ItemId, int DigimonId, float Scale, int SlotCount);

/// <summary>Section 9: per-penalty-level config (sPLAY_PANELTY, 12 bytes).</summary>
public sealed record DMBasePlayPenalty(int PenaltyLevel, int Exp, int Drop);

/// <summary>Section 12: per-rank max-skill-level table for digimon evolution.</summary>
public sealed record DMBaseDigimonEvoMaxLevel(int Key, int SkillExpStartLv, IReadOnlyList<int> SkillMaxLevels);

/// <summary>Section 13: D-Skill expansion-item allowed evolution-types.</summary>
public sealed record DMBaseDskillOpenExpansion(int ItemSubType, int ExpansionRank, IReadOnlyList<int> AllowedEvoTypes);
