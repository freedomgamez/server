namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

/// <summary>
/// Subset of DMBase.bin needed by Character.Host: just section 1 (per-tamer per-level stats),
/// further filtered to level=1 since character creation is the only consumer here.
/// The bin's other sections (digimon-rank scaling, map info, jump buster, party, guild, limit,
/// person store, etc.) are Game.Host territory and intentionally not parsed in this loader.
/// </summary>
public sealed class DMBase
{
    public IReadOnlyDictionary<int, DMBaseStatRecord> TamerStats { get; }

    public DMBase(IReadOnlyDictionary<int, DMBaseStatRecord> tamerStats)
    {
        TamerStats = tamerStats;
    }

    // Section 1 ID encoding: (tamerModel - 80000) * 1000 + level
    public static int TamerKey(int tamerModel, int level) => (tamerModel - 80000) * 1000 + level;

    public DMBaseStatRecord? FindTamerStats(int tamerModel, int level)
        => TamerStats.TryGetValue(TamerKey(tamerModel, level), out var r) ? r : null;
}

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
