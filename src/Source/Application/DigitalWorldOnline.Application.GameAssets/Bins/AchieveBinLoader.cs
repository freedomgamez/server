namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>Achieve.bin</c> per <c>AchieveMng::SaveBin</c>
/// (<c>LibProj/CsFileTable/AchieveMng.cpp:236-259</c>) and the recursive
/// <c>sTYPE::Save</c> tree (<c>AchieveMng.cpp:17-30</c>).
///
/// File layout:
///   §A   recursive sTYPE root tree
///         per node: TCHAR[32] name (64 B) + int childCount + childCount × node
///         (consumed at load — pure UI category labels, dropped)
///   §B   [int count] + count × CsAchieve::sINFO (796 B per record)
///
/// CsAchieve::sINFO (default Visual C++ alignment, 4-byte struct align — no 8-byte members):
///   0    DWORD     s_nQuestID
///   4    DWORD     s_nIcon
///   8    USHORT    s_nPoint
///   10   bool      s_bDisplay (1 B)
///   11   pad 1                                -- align next TCHAR[] to 2
///   12   TCHAR     s_szName[64]               -- 128 B, skipped
///   140  TCHAR     s_szComment[256]           -- 512 B, skipped
///   652  TCHAR     s_szTitle[64]              -- 128 B, skipped
///   780  int       s_nGroup
///   784  int       s_nSubGroup
///   788  int       s_nType
///   792  int       s_nBuffCode
///   796  end (no trailing pad — 796 % 4 == 0)
/// </summary>
public sealed class AchieveBinLoader
{
    private const string FileName = "Achieve.bin";
    private const int RecordSize = 796;

    private Achieve? _data;

    public Achieve Data => _data ?? throw new InvalidOperationException(
        $"{nameof(AchieveBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public Achieve Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static Achieve Parse(BinaryReader r)
    {
        SkipTypeNode(r);   // §A — recursive sTYPE tree, dropped

        int count = r.ReadInt32();
        var list = new List<AchieveRecord>(count);

        for (int i = 0; i < count; i++)
        {
            uint questId = r.ReadUInt32();
            uint icon = r.ReadUInt32();
            ushort point = r.ReadUInt16();
            bool displayHidden = r.ReadByte() != 0;
            _ = r.ReadByte();                                   // pad 1
            r.BaseStream.Seek(128 + 512 + 128, SeekOrigin.Current);  // skip 3 strings
            int group = r.ReadInt32();
            int subGroup = r.ReadInt32();
            int type = r.ReadInt32();
            int buffCode = r.ReadInt32();

            list.Add(new AchieveRecord(
                (int)questId, icon, point, displayHidden,
                group, subGroup, type, buffCode));
        }

        return new Achieve(list);
    }

    /// <summary>Recursive walker — drops the entire sTYPE tree.</summary>
    private static void SkipTypeNode(BinaryReader r)
    {
        r.BaseStream.Seek(64, SeekOrigin.Current);             // TCHAR[32] s_szType
        int childCount = r.ReadInt32();
        for (int i = 0; i < childCount; i++) SkipTypeNode(r);
    }
}
