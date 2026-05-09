namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

/// <summary>
/// Parses ONLY section 1 of <c>DMBase.bin</c> (per-tamer per-level stat table) and stops.
/// Character.Host's only stat-query consumers are <c>TamerBaseStatusQuery</c> and
/// <c>TamerLevelStatusQuery</c> — both at character-create time, both at level=1 — so we
/// filter to <c>level == 1</c> (12 rows out of 1440) and don't bother reading section 2
/// (digimon-rank scaling) or the rest of the file. <c>DigimonBaseInfoQuery</c> is satisfied
/// by Digimon_List.bin instead, and Game.Host has its own loader for the full per-level
/// scaling math.
///
/// Format (from <c>BaseMng::SaveBin</c> in <c>LibProj/CsFileTable/BaseMng.cpp</c>):
///   [u32 tamerCount = 1440] + 1440 × CsBase::sINFO    ← parsed (filtered to level=1)
///   ...everything after section 1 is intentionally ignored...
///
/// CsBase::sINFO layout (40 bytes, default Visual C++ alignment — __int64 forces 8-byte alignment):
///   offset  type     field
///   0       DWORD    s_dwID
///   4       USHORT   s_nLevel
///   6       (2 pad)
///   8       __int64  s_dwExp
///   16      int      s_nHP
///   20      int      s_nDS
///   24      USHORT   s_nMoveSpeed
///   26      USHORT   s_nDefence
///   28      USHORT   s_nEvasion
///   30      USHORT   s_nCritical
///   32      USHORT   s_nAttack
///   34      USHORT   s_nHitRate
///   36      (4 pad to 40)
/// </summary>
public sealed class DMBaseBinLoader
{
    private const string FileName = "DMBase.bin";
    private const int RecordSize = 40;

    private DMBase? _data;

    public DMBase Data => _data ?? throw new InvalidOperationException(
        $"{nameof(DMBaseBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public DMBase Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static DMBase Parse(BinaryReader r)
    {
        var tamer = ReadStatSection(r, level1Only: true);
        // Section 2 (digimon-rank-level scaling) and everything after intentionally not
        // consumed — Character.Host doesn't read those, and DigimonBaseInfoQuery gets its
        // data from Digimon_List.bin instead.
        return new DMBase(tamer);
    }

    private static Dictionary<int, DMBaseStatRecord> ReadStatSection(BinaryReader r, bool level1Only)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBaseStatRecord>();
        for (int i = 0; i < count; i++)
        {
            // Always read the full record — we can't skip ahead in a non-seekable parse cleanly,
            // and we need the stream pointer at the right place when the next section starts.
            var rec = ReadRecord(r);
            if (level1Only && rec.Level != 1) continue;
            // Tamer-side has known dups in some pack data per BaseMng._LoadFilePack — keep first.
            if (!map.ContainsKey(rec.Id))
                map[rec.Id] = rec;
        }
        return map;
    }

    private static DMBaseStatRecord ReadRecord(BinaryReader r)
    {
        int id = r.ReadInt32();
        ushort level = r.ReadUInt16();
        _ = r.ReadUInt16();           // 2 bytes pad before __int64
        long exp = r.ReadInt64();
        int hp = r.ReadInt32();
        int ds = r.ReadInt32();
        ushort ms = r.ReadUInt16();
        ushort def = r.ReadUInt16();
        ushort ev = r.ReadUInt16();
        ushort cr = r.ReadUInt16();
        ushort at = r.ReadUInt16();
        ushort ht = r.ReadUInt16();
        _ = r.ReadInt32();            // 4 bytes trailing pad to 40
        return new DMBaseStatRecord(id, level, exp, hp, ds, ms, def, ev, cr, at, ht);
    }
}
