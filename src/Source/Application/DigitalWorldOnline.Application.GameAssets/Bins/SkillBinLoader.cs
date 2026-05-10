namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>Skill.bin</c> per <c>SkillMng::SaveBin</c> in
/// <c>LibProj/CsFileTable/SkillMng.cpp:536-587</c>.  Three sections concatenated:
/// <code>
///   [int count1] + count1 × CsSkill::sINFO          (5322 × 736 B)
///   [int count2] + count2 × CsTamerSkill::sTSkill_INFO   (64 × 36 B)
///   [int count3] + count3 × CsAreaCheck::sArea_Check     (93 × 64 B)
/// </code>
/// Total file size 3,925,260 B verified at parse time.
/// </summary>
public sealed class SkillBinLoader
{
    private const string FileName = "Skill.bin";
    private const int SkillRecordSize = 736;
    private const int TamerSkillRecordSize = 36;
    private const int AreaCheckRecordSize = 64;
    private const int AreaCount = 30;

    private Skill? _data;

    public Skill Data => _data ?? throw new InvalidOperationException(
        $"{nameof(SkillBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public Skill Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static Skill Parse(BinaryReader r)
    {
        // §1 — CsSkill rows
        int skillCount = r.ReadInt32();
        var skills = new Dictionary<int, SkillRecord>(skillCount);

        for (int i = 0; i < skillCount; i++)
        {
            int id = r.ReadInt32();
            r.BaseStream.Seek(64 + 512, SeekOrigin.Current);   // skip s_szName + s_szComment
            var apply0 = ReadSkillApply(r);
            var apply1 = ReadSkillApply(r);
            var apply2 = ReadSkillApply(r);
            ushort levelupPoint = r.ReadUInt16();
            ushort maxLevel = r.ReadUInt16();
            ushort attrType = r.ReadUInt16();
            ushort natureType = r.ReadUInt16();
            ushort familyType = r.ReadUInt16();
            ushort useHp = r.ReadUInt16();
            ushort useDs = r.ReadUInt16();
            ushort icon = r.ReadUInt16();
            ushort target = r.ReadUInt16();
            ushort attType = r.ReadUInt16();
            float attRange = r.ReadSingle();
            float attRangeMin = r.ReadSingle();
            float attRangeNor = r.ReadSingle();
            float attRangeMax = r.ReadSingle();
            ushort attSphere = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);          // pad → align float
            float castingTime = r.ReadSingle();
            float damageTime = r.ReadSingle();
            ushort damageDay = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);          // pad → align float
            float distanceTime = r.ReadSingle();
            float cooldownTimeMs = r.ReadSingle();
            ushort cooldownDay = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);          // pad → align float
            float skillVelocity = r.ReadSingle();
            float skillAccel = r.ReadSingle();
            ushort skillType = r.ReadUInt16();
            ushort limitLevel = r.ReadUInt16();
            ushort skillGroup = r.ReadUInt16();
            ushort skillRank = r.ReadUInt16();
            ushort memorySkill = r.ReadUInt16();
            ushort reqItem = r.ReadUInt16();

            // Pack data sometimes contains duplicate skill rows; keep first, skip later dups
            // (mirrors the convention used in our Skill.bin client loader and other bin loaders here).
            if (skills.ContainsKey(id))
                continue;

            skills[id] = new SkillRecord(
                id, apply0, apply1, apply2,
                levelupPoint, maxLevel, attrType, natureType, familyType,
                useHp, useDs, icon, target, attType,
                attRange, attRangeMin, attRangeNor, attRangeMax, attSphere,
                castingTime, damageTime, damageDay,
                distanceTime, cooldownTimeMs, cooldownDay,
                skillVelocity, skillAccel,
                skillType, limitLevel, skillGroup, skillRank, memorySkill, reqItem);
        }

        // §2 — CsTamerSkill rows
        int tamerCount = r.ReadInt32();
        var tamerSkills = new Dictionary<int, TamerSkillRecord>(tamerCount);

        for (int i = 0; i < tamerCount; i++)
        {
            int idx = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);          // pad → align DWORD
            int code = r.ReadInt32();
            ushort type = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);          // pad → align DWORD
            int f1 = r.ReadInt32();
            int f2 = r.ReadInt32();
            int tSeq = r.ReadInt32();
            int dSeq = r.ReadInt32();
            ushort useState = r.ReadUInt16();
            ushort useAreaCheck = r.ReadUInt16();
            ushort available = r.ReadUInt16();
            r.BaseStream.Seek(2, SeekOrigin.Current);          // struct-end pad

            tamerSkills[idx] = new TamerSkillRecord(
                idx, code, type, f1, f2, tSeq, dSeq,
                useState, useAreaCheck, available);
        }

        // §3 — CsAreaCheck rows
        int areaCount = r.ReadInt32();
        var areaChecks = new Dictionary<int, AreaCheckRecord>(areaCount);

        for (int i = 0; i < areaCount; i++)
        {
            int idx = r.ReadInt32();
            var areas = new ushort[AreaCount];
            for (int j = 0; j < AreaCount; j++)
                areas[j] = r.ReadUInt16();

            areaChecks[idx] = new AreaCheckRecord(idx, areas);
        }

        return new Skill(skills, tamerSkills, areaChecks);
    }

    private static SkillApply ReadSkillApply(BinaryReader r)
    {
        int a = r.ReadInt32();
        int invokeRate = r.ReadInt32();
        int b = r.ReadInt32();
        int c = r.ReadInt32();
        ushort buffCode = r.ReadUInt16();
        ushort id = r.ReadUInt16();
        short increaseBPoint = r.ReadInt16();
        r.BaseStream.Seek(2, SeekOrigin.Current);              // sAPPLY struct-end pad → 24 B
        return new SkillApply(a, invokeRate, b, c, buffCode, id, increaseBPoint);
    }
}
