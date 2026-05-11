namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>Monster.bin</c> per <c>CsMonsterMng::SaveBin</c> +
/// <c>_LoadBin</c> in <c>LibProj/CsFileTable/MonsterMng.cpp:555-672</c>.
///
/// Four concatenated sections (each prefixed with <c>int count</c>):
/// <code>
///   §1  CsMonster::sINFO            (2138 × 396 B)  — mob catalog
///   §2  { int level, int hit }      (100 × 8 B)      — level→hit map
///   §3  CsMonsterSkill::sINFO       (2044 × 144 B)  — skill rows
///   §4  CsMonsterSkillTerms::sINFO  (31 × 12 B)      — AoE shapes
/// </code>
/// Total file size 1,142,172 B in v487; cursor lands at EOF after section 4.
/// <c>CsMonsterDrop</c> declared in <c>Monster.h</c> is NOT written to the bin in v487
/// (commented-out <c>_LoadExcelDrop</c>; no <c>SaveBin</c> entry).
///
/// All record sizes verified by direct python decode against the file before this
/// loader shipped — see <c>project_monster_bin_plan.md §1.5</c>.
/// </summary>
public sealed class MonsterBinLoader
{
    private const string FileName = "Monster.bin";

    // §1 CsMonster::sINFO: DWORD×2 + 2×TCHAR[64] + TCHAR[34] + ... fixed layout.
    // Offsets from project_monster_bin_plan.md §1.1.
    private const int MonsterRecordSize = 396;
    private const int MonsterNameBytes = 64 * 2;        // TCHAR[64], wide
    private const int MonsterPenNameBytes = 34 * 2;
    private const int MonsterTitleBytes = 64 * 2;

    private const int HitRecordSize = 8;                // int level + int hit
    private const int SkillRecordSize = 144;
    private const int TermsRecordSize = 12;

    private const int SkillNoticeNameBytes = 64;        // char[MAX_FILENAME=64] ASCII

    private Monster? _data;

    public Monster Data => _data ?? throw new InvalidOperationException(
        $"{nameof(MonsterBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public Monster Load()
    {
        if (_data != null) return _data;
        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static Monster Parse(BinaryReader r)
    {
        // ─── §1 CsMonster::sINFO ─────────────────────────────────────────
        int monsterCount = r.ReadInt32();
        var byType = new Dictionary<int, MonsterRecord>(monsterCount);
        for (int i = 0; i < monsterCount; i++)
        {
            int type = r.ReadInt32();                                       // s_dwMonsterID
            int modelId = r.ReadInt32();                                    // s_dwModelID
            r.BaseStream.Seek(MonsterNameBytes, SeekOrigin.Current);        // skip s_szName[64]
            r.BaseStream.Seek(MonsterPenNameBytes, SeekOrigin.Current);     // skip s_szPenName[34]
            r.BaseStream.Seek(MonsterTitleBytes, SeekOrigin.Current);       // skip s_szTitle[64]
            ushort level = r.ReadUInt16();
            ushort exp = r.ReadUInt16();
            ushort battle = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);                       // pad → align int s_nHP
            int hp = r.ReadInt32();
            int ds = r.ReadInt32();
            ushort defPower = r.ReadUInt16();
            ushort evasion = r.ReadUInt16();
            ushort moveSpeed = r.ReadUInt16();
            ushort walkSpeed = r.ReadUInt16();
            ushort critRate = r.ReadUInt16();
            ushort attPower = r.ReadUInt16();
            ushort attSpeed = r.ReadUInt16();
            ushort attRange = r.ReadUInt16();
            ushort hitRate = r.ReadUInt16();
            ushort sight = r.ReadUInt16();
            ushort huntRange = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);                       // pad → align float s_fScale
            float scale = r.ReadSingle();
            ushort classFlag = r.ReadUInt16();
            r.BaseStream.Seek(2 * 6, SeekOrigin.Current);                   // skip s_nIcon1..6 (12 B)
            ushort expMin = r.ReadUInt16();
            ushort expMax = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);                       // trailing struct pad → 396

            byType[type] = new MonsterRecord(
                Type: type, ModelId: modelId,
                Level: level, Exp: exp, BattlePower: battle,
                Hp: hp, Ds: ds,
                DefPower: defPower, Evasion: evasion, MoveSpeed: moveSpeed, WalkSpeed: walkSpeed,
                CriticalRate: critRate, AttPower: attPower, AttSpeed: attSpeed, AttRange: attRange, HitRate: hitRate,
                Sight: sight, HuntRange: huntRange,
                Scale: scale, Class: classFlag,
                ExpMin: expMin, ExpMax: expMax);
        }

        // ─── §2 MonsterHit map ───────────────────────────────────────────
        int hitCount = r.ReadInt32();
        var hitByLevel = new Dictionary<int, int>(hitCount);
        for (int i = 0; i < hitCount; i++)
        {
            int lvl = r.ReadInt32();
            int ht = r.ReadInt32();
            hitByLevel[lvl] = ht;
        }

        // ─── §3 CsMonsterSkill::sINFO ───────────────────────────────────
        int skillCount = r.ReadInt32();
        var skillsByIndex = new Dictionary<int, MonsterSkillRecord>(skillCount);
        for (int i = 0; i < skillCount; i++)
        {
            ushort idx = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);                       // pad → align DWORD s_dwMonsterID
            int monsterType = r.ReadInt32();
            uint cooldown = r.ReadUInt32();
            ushort castTime = r.ReadUInt16();
            ushort castCheck = r.ReadUInt16();
            ushort targetCnt = r.ReadUInt16();
            ushort targetMin = r.ReadUInt16();
            ushort targetMax = r.ReadUInt16();
            ushort useTerms = r.ReadUInt16();
            ushort skillType = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);                       // pad → align DWORD s_dwEff_Val_Min
            uint effValMin = r.ReadUInt32();
            uint effValMax = r.ReadUInt32();
            ushort rangeIdx = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);                       // pad → align DWORD s_dwSequenceID
            uint sequenceId = r.ReadUInt32();
            ushort aniDelay = r.ReadUInt16();
            ushort velocity = r.ReadUInt16();
            ushort accel = r.ReadUInt16();
            var effFactor = new ushort[3] { r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16() };
            // cursor now 56 — naturally aligned for DWORD array
            var effFactVal = new uint[3] { r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32() };
            uint talkId = r.ReadUInt32();
            uint activeType = r.ReadUInt32();
            float noticeTime = r.ReadSingle();
            r.BaseStream.Seek(SkillNoticeNameBytes, SeekOrigin.Current);    // skip s_szNoticeEff_Name[64]

            skillsByIndex[idx] = new MonsterSkillRecord(
                SkillIndex: idx, MonsterType: monsterType,
                Cooldown: cooldown, CastTime: castTime, CastCheck: castCheck,
                TargetCount: targetCnt, TargetMin: targetMin, TargetMax: targetMax,
                UseTerms: useTerms, SkillType: skillType,
                EffectValueMin: effValMin, EffectValueMax: effValMax,
                RangeIndex: rangeIdx, SequenceId: sequenceId,
                AnimationDelay: aniDelay, Velocity: velocity, Acceleration: accel,
                EffectFactor: effFactor, EffectFactorValue: effFactVal,
                TalkId: talkId, ActiveType: activeType, NoticeTime: noticeTime);
        }

        // ─── §4 CsMonsterSkillTerms::sINFO ───────────────────────────────
        int termsCount = r.ReadInt32();
        var termsByIndex = new Dictionary<int, MonsterSkillTermsRecord>(termsCount);
        for (int i = 0; i < termsCount; i++)
        {
            ushort idx = r.ReadUInt16();
            ushort direction = r.ReadUInt16();
            uint range = r.ReadUInt32();
            ushort targetingType = r.ReadUInt16();
            ushort refCode = r.ReadUInt16();
            termsByIndex[idx] = new MonsterSkillTermsRecord(
                Index: idx, Direction: direction, Range: range,
                TargetingType: targetingType, RefCode: refCode);
        }

        return new Monster(byType, hitByLevel, skillsByIndex, termsByIndex);
    }
}
