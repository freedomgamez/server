namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>Buff.bin</c> per <c>BuffMng::SaveBin</c> in
/// <c>LibProj/CsFileTable/BuffMng.cpp:180-198</c>.
///
///   [int count] + count × CsBuff::sINFO (476 B per record)
///
/// Layout (default Visual C++ alignment, struct align = 4 from DWORD members; v487 has
/// <c>VERSION_USA</c> so <c>BUFF_NAME_LEN = 64</c>):
///   0    USHORT   s_dwID
///   2    TCHAR    s_szName[64]               -- 128 B, skipped
///   130  TCHAR    s_szComment[128]           -- 256 B, skipped
///   386  USHORT   s_nBuffIcon                -- UI only, loaded but unused
///   388  USHORT   s_nBuffType
///   390  USHORT   s_nBuffLifeType
///   392  USHORT   s_nBuffTimeType
///   394  USHORT   s_nMinLv
///   396  USHORT   s_nBuffClass
///   398  pad 2                                -- align to 4 for next DWORD
///   400  DWORD    s_dwSkillCode
///   404  DWORD    s_dwDigimonSkillCode
///   408  bool     s_bDelete                   -- 1 B; record dropped if true
///   409  char     s_szEffectFile[64]          -- 64 B (ASCII), skipped
///   473  pad 1                                -- align to 2 for next USHORT
///   474  USHORT   s_nConditionLv
///   476  end (no trailing pad — multiple of 4)
///
/// Records with <c>s_bDelete == true</c> are dropped at load (mirrors
/// <c>BuffMng.cpp:172</c> — client logs and discards them too).
/// </summary>
public sealed class BuffBinLoader
{
    private const string FileName = "Buff.bin";
    private const int RecordSize = 476;

    private Buff? _data;

    public Buff Data => _data ?? throw new InvalidOperationException(
        $"{nameof(BuffBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public Buff Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static Buff Parse(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, BuffRecord>(count);

        for (int i = 0; i < count; i++)
        {
            ushort id = r.ReadUInt16();
            r.BaseStream.Seek(128 + 256, SeekOrigin.Current);   // skip s_szName + s_szComment
            _ = r.ReadUInt16();                                 // s_nBuffIcon — UI only
            ushort buffType = r.ReadUInt16();
            ushort lifeType = r.ReadUInt16();
            ushort timeType = r.ReadUInt16();
            ushort minLv = r.ReadUInt16();
            ushort buffClass = r.ReadUInt16();
            _ = r.ReadUInt16();                                 // pad: align to 4 for DWORD
            uint skillCode = r.ReadUInt32();
            uint digimonSkillCode = r.ReadUInt32();
            bool isDeleted = r.ReadByte() != 0;
            r.BaseStream.Seek(64, SeekOrigin.Current);          // skip s_szEffectFile
            _ = r.ReadByte();                                   // pad: align to 2 for USHORT
            ushort conditionLv = r.ReadUInt16();

            // Keep deleted records — many memory-skill buffs (skillCode 9000xxx) are
            // flagged s_bDelete in v487 (regional-disable convention in original DMO).
            // The active-only filter is now enforced at query time
            // (BuffInfoAssetsQueryHandler), so consumers that need the full set —
            // notably MemorySkillUsePacketProcessor — can still find them via the
            // raw BuffBinLoader API.
            map[id] = new BuffRecord(
                id, buffType, lifeType, timeType,
                minLv, buffClass,
                skillCode, digimonSkillCode,
                conditionLv,
                isDeleted);
        }

        return new Buff(map);
    }
}
