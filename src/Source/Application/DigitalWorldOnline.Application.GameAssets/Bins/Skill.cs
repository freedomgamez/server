namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// In-memory <c>Skill.bin</c> contents — three sections, all from
/// <c>LibProj/CsFileTable/SkillMng.cpp:_LoadBin</c>.  String fields are skipped per the
/// "server doesn't render UI" convention (see <c>reference_bin_string_framing.md</c>).
/// </summary>
public sealed class Skill
{
    /// <summary>
    /// §1 — <c>CsSkill::sINFO</c> rows keyed by <c>s_dwID</c>.  Covers the universe of
    /// skills (digimon, tamer, monster, item) that any other section references by ID.
    /// </summary>
    public IReadOnlyDictionary<int, SkillRecord> ById { get; }

    /// <summary>
    /// §2 — <c>CsTamerSkill::sTSkill_INFO</c> rows keyed by <c>s_nIndex</c>.
    /// 64 rows in v487; the first six (idx 1-6) are internal tamer-action placeholders
    /// (Standby/Follow/Attack) with <c>s_dwSkillCode == 0</c>.  Idx ≥ 7 are real
    /// usable tamer skills.
    /// </summary>
    public IReadOnlyDictionary<int, TamerSkillRecord> TamerSkillsByIndex { get; }

    /// <summary>
    /// §3 — <c>CsAreaCheck::sArea_Check</c> rows keyed by <c>s_dwIndex</c> (a tamer-skill
    /// index, mirroring §2).  No current server consumer; exposed for future
    /// area-restricted-skill enforcement.
    /// </summary>
    public IReadOnlyDictionary<int, AreaCheckRecord> AreaChecksByIndex { get; }

    public Skill(
        IReadOnlyDictionary<int, SkillRecord> byId,
        IReadOnlyDictionary<int, TamerSkillRecord> tamerSkillsByIndex,
        IReadOnlyDictionary<int, AreaCheckRecord> areaChecksByIndex)
    {
        ById = byId;
        TamerSkillsByIndex = tamerSkillsByIndex;
        AreaChecksByIndex = areaChecksByIndex;
    }

    public SkillRecord? FindSkill(int id) => ById.TryGetValue(id, out var rec) ? rec : null;
    public TamerSkillRecord? FindTamerSkill(int index) => TamerSkillsByIndex.TryGetValue(index, out var rec) ? rec : null;
}

/// <summary>
/// One <c>CsSkill::sINFO</c> entry — 736 bytes on disk.  Bin layout (default Visual C++
/// struct alignment = 4 because of <c>DWORD</c>/<c>float</c> members):
/// <code>
///   0   DWORD   s_dwID
///   4   TCHAR   s_szName[32]                 -- 64 B, skipped
///   68  TCHAR   s_szComment[256]             -- 512 B, skipped
///   580 sAPPLY  s_Apply[3]                   -- 3 × 24 B = 72 B
///   652 USHORT  s_nLevelupPoint
///   654 USHORT  s_nMaxLevel
///   656 USHORT  s_nAttributeType
///   658 USHORT  s_nNatureType
///   660 USHORT  s_nFamilyType
///   662 USHORT  s_nUseHP
///   664 USHORT  s_nUseDS
///   666 USHORT  s_nIcon                       -- UI only, parsed but unused
///   668 USHORT  s_nTarget
///   670 USHORT  s_nAttType
///   672 float   s_fAttRange
///   676 float   s_fAttRange_MinDmg
///   680 float   s_fAttRange_NorDmg
///   684 float   s_fAttRange_MaxDmg
///   688 USHORT  s_nAttSphere
///   690 pad 2
///   692 float   s_fCastingTime
///   696 float   s_fDamageTime
///   700 USHORT  s_nDamageDay
///   702 pad 2
///   704 float   s_nDistanceTime              -- field is named s_n* but typed float in source
///   708 float   s_fCooldownTime              -- in milliseconds (server divides /1000)
///   712 USHORT  s_nCooldownDay
///   714 pad 2
///   716 float   s_fSkill_Velocity
///   720 float   s_fSkill_Accel
///   724 USHORT  s_nSkillType
///   726 USHORT  s_nLimitLevel
///   728 USHORT  s_nSkillGroup
///   730 USHORT  s_nSkillRank
///   732 USHORT  s_nMemorySkill                -- 0 = normal, 1-3 = memory; 5054/5322 are 0
///   734 USHORT  s_nReq_Item
///   736 end
/// </code>
/// </summary>
public sealed record SkillRecord(
    int Id,
    SkillApply Apply0,
    SkillApply Apply1,
    SkillApply Apply2,
    ushort LevelupPoint,
    ushort MaxLevel,
    ushort AttributeType,
    ushort NatureType,
    ushort FamilyType,
    ushort UseHP,
    ushort UseDS,
    ushort Icon,
    ushort Target,
    ushort AttType,
    float AttRange,
    float AttRangeMinDmg,
    float AttRangeNorDmg,
    float AttRangeMaxDmg,
    ushort AttSphere,
    float CastingTime,
    float DamageTime,
    ushort DamageDay,
    float DistanceTime,
    float CooldownTimeMs,
    ushort CooldownDay,
    float SkillVelocity,
    float SkillAccel,
    ushort SkillType,
    ushort LimitLevel,
    ushort SkillGroup,
    ushort SkillRank,
    ushort MemorySkill,
    ushort ReqItem);

/// <summary>
/// One <c>CsSkill::sINFO::sAPPLY</c> entry — 24 bytes on disk (sub-struct of SkillRecord).
/// Skill-modifier slot: each skill carries up to 3 of these for its primary effect
/// triplet.  See <c>LibProj/CsFileTable/Skill.h:24-34</c>.
/// </summary>
public sealed record SkillApply(
    int A,
    int InvokeRate,
    int B,
    int C,
    ushort BuffCode,
    ushort Id,
    short IncreaseBPoint);

/// <summary>
/// One <c>CsTamerSkill::sTSkill_INFO</c> entry — 36 bytes on disk.
/// <code>
///   0   USHORT  s_nIndex                      -- map key (1-based)
///   2   pad 2
///   4   DWORD   s_dwSkillCode                 -- references SkillRecord.Id
///   8   USHORT  s_nType                       -- 1=attack, 2=buff/debuff, 3=script
///   10  pad 2
///   12  DWORD   s_dwFactor1
///   16  DWORD   s_dwFactor2
///   20  DWORD   s_dwTamer_SeqID
///   24  DWORD   s_dwDigimon_SeqID
///   28  USHORT  s_nUseState                   -- 0=always, 1=not in combat
///   30  USHORT  s_nUse_Are_Check              -- 1=area-restricted
///   32  USHORT  s_nAvailable                  -- 1=blacklist, 2=whitelist
///   34  pad 2 (struct-end alignment)
///   36  end
/// </code>
/// </summary>
public sealed record TamerSkillRecord(
    int Index,
    int SkillCode,
    ushort Type,
    int Factor1,
    int Factor2,
    int TamerSeqId,
    int DigimonSeqId,
    ushort UseState,
    ushort UseAreaCheck,
    ushort Available);

/// <summary>
/// One <c>CsAreaCheck::sArea_Check</c> entry — 64 bytes on disk.
/// Maps a tamer-skill index to up to 30 region IDs where the skill is permitted/forbidden
/// (governed by the corresponding <see cref="TamerSkillRecord.Available"/> flag).
/// </summary>
public sealed record AreaCheckRecord(
    int Index,
    ushort[] Areas);
