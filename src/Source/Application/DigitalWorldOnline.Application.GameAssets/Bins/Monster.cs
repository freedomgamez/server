using System.Collections.Generic;

namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Decoded v487 <c>Monster.bin</c>.  Four sections:
/// <list type="bullet">
///   <item><see cref="ByType"/> — <c>CsMonster::sINFO</c> catalog rows by Monster ID.</item>
///   <item><see cref="HitByLevel"/> — <c>CsMonsterMng::m_mapMonsterHit</c>: tamer level → mob hit-floor.</item>
///   <item><see cref="SkillsByIndex"/> — <c>CsMonsterSkill::sINFO</c> rows by skill index.</item>
///   <item><see cref="TermsByIndex"/> — <c>CsMonsterSkillTerms::sINFO</c> AoE-shape rows by index.</item>
/// </list>
/// <see cref="SkillsForMob"/> is the per-mob lookup the rotation/cast pipeline uses.
/// </summary>
public sealed class Monster
{
    public IReadOnlyDictionary<int, MonsterRecord> ByType { get; }
    public IReadOnlyDictionary<int, int> HitByLevel { get; }
    public IReadOnlyDictionary<int, MonsterSkillRecord> SkillsByIndex { get; }
    public IReadOnlyDictionary<int, MonsterSkillTermsRecord> TermsByIndex { get; }

    private readonly IReadOnlyDictionary<int, IReadOnlyList<MonsterSkillRecord>> _skillsByMob;

    public Monster(
        IReadOnlyDictionary<int, MonsterRecord> byType,
        IReadOnlyDictionary<int, int> hitByLevel,
        IReadOnlyDictionary<int, MonsterSkillRecord> skillsByIndex,
        IReadOnlyDictionary<int, MonsterSkillTermsRecord> termsByIndex)
    {
        ByType = byType;
        HitByLevel = hitByLevel;
        SkillsByIndex = skillsByIndex;
        TermsByIndex = termsByIndex;

        // Pre-group skills by owner mob so rotation lookups are O(1).
        var grouped = new Dictionary<int, List<MonsterSkillRecord>>(byType.Count);
        foreach (var s in skillsByIndex.Values)
        {
            if (!grouped.TryGetValue(s.MonsterType, out var list))
            {
                list = new List<MonsterSkillRecord>();
                grouped[s.MonsterType] = list;
            }
            list.Add(s);
        }
        var read = new Dictionary<int, IReadOnlyList<MonsterSkillRecord>>(grouped.Count);
        foreach (var kv in grouped) read[kv.Key] = kv.Value;
        _skillsByMob = read;
    }

    /// <summary>All skills owned by <paramref name="monsterType"/>, or an empty list if the mob has none.</summary>
    public IReadOnlyList<MonsterSkillRecord> SkillsForMob(int monsterType) =>
        _skillsByMob.TryGetValue(monsterType, out var list) ? list : System.Array.Empty<MonsterSkillRecord>();
}

/// <summary>
/// One <c>CsMonster::sINFO</c> row (396 B).  Strings + UI icons (s_szName, s_szPenName,
/// s_szTitle, s_nIcon1..6) are intentionally skipped on parse per the no-strings-on-server
/// convention.  s_fScale is kept because it factors into AttRange (see <c>Monster.cpp:425</c>).
/// </summary>
public sealed record MonsterRecord(
    int Type,
    int ModelId,
    ushort Level,
    ushort Exp,
    ushort BattlePower,
    int Hp,
    int Ds,
    ushort DefPower,
    ushort Evasion,
    ushort MoveSpeed,
    ushort WalkSpeed,
    ushort CriticalRate,
    ushort AttPower,
    ushort AttSpeed,
    ushort AttRange,
    ushort HitRate,
    ushort Sight,
    ushort HuntRange,
    float Scale,
    ushort Class,
    ushort ExpMin,
    ushort ExpMax)
{
    /// <summary>
    /// Catalog boss flag per <c>CsMonster::IsBossType()</c> in <c>Monster.h:91</c>:
    /// class ∈ {3 = boss, 4 = elite boss, 6 = raid boss}.  Note this is the *catalog*
    /// convention; the server's runtime DB convention uses <c>Class == 8</c> for raid
    /// rewards — they don't always match.  See <c>project_monster_bin_plan.md §4 #7</c>.
    /// </summary>
    public bool IsBoss => Class == 3 || Class == 4 || Class == 6;
}

/// <summary>
/// One <c>CsMonsterSkill::sINFO</c> row (144 B).  <c>s_szNoticeEff_Name</c> (telegraph
/// effect path) is skipped — client-only VFX field.
/// </summary>
public sealed record MonsterSkillRecord(
    ushort SkillIndex,
    int MonsterType,
    uint Cooldown,                  // ms
    ushort CastTime,                // ms
    ushort CastCheck,               // 0 = can move during cast, 1 = root mob
    ushort TargetCount,
    ushort TargetMin,
    ushort TargetMax,
    ushort UseTerms,                // CsMonsterSkill::eTERM_TYPE — rotation trigger
    ushort SkillType,               // CsMonsterSkill::eEFFECT_TYPE — what the skill does
    uint EffectValueMin,
    uint EffectValueMax,
    ushort RangeIndex,              // FK → MonsterSkillTermsRecord.Index
    uint SequenceId,                // client VFX
    ushort AnimationDelay,
    ushort Velocity,
    ushort Acceleration,
    ushort[] EffectFactor,          // eFACTOR_TYPE × 3
    uint[] EffectFactorValue,       // matching values × 3
    uint TalkId,
    uint ActiveType,                // 0 = self, 1 = target, 2 = coord (AoE origin)
    float NoticeTime);              // ms pre-cast telegraph

/// <summary>
/// One <c>CsMonsterSkillTerms::sINFO</c> row (12 B).  Defines an AoE shape that a
/// CsMonsterSkill row references by <c>RangeIndex</c>.
/// </summary>
public sealed record MonsterSkillTermsRecord(
    ushort Index,
    ushort Direction,
    uint Range,
    ushort TargetingType,
    ushort RefCode);
