namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses ALL 11 sections present in v487 <c>DMBase.bin</c> (BaseMng::SaveBin in
/// <c>LibProj/CsFileTable/BaseMng.cpp</c>). The optional <c>#ifdef GUILD_RENEWAL</c>
/// sections (10a + 10b) aren't present in v487 — sections 11/12/13 follow PlayPenalty
/// directly. String fields throughout (evolution-stage names in section 11, guild-skill
/// names/comments in 10b, etc.) are client-only and skipped per
/// <c>reference_bin_string_framing.md</c>.
///
/// Many sections (3 onwards) don't yet have a server consumer — they're parsed and held
/// in memory so future feature work can read them without re-touching this file.
///
/// Section layout summary:
///   1.  [int]                + N × CsBase::sINFO (40 B)        tamer per-level
///   2.  [int]                + N × CsBase::sINFO (40 B)        digimon-rank per-level
///   3.  [int]                + N × CsBaseMapInfo::sINFO (12 B) map config
///   4.  [int]                + N × {u32 itemID, int destCount, destCount × u32}
///   5.  sPARTY (4 B)
///   6.  [int]                + N × sGuild (44 B)
///   7.  sLIMIT (16 B)
///   8.  sBASE_INFO (12 B)    + [int objCount] + objCount × sOBJECT_INFO (144 B)
///   9.  [int]                + N × sPLAY_PANELTY (12 B)
///   --  (10a/b GUILD_RENEWAL absent in v487)
///   11. [int]                + N × {int key, wstring name [skipped], int applyValue}
///   12. [int]                + N × {int key, int skillExpStartLv, int subCount, subCount × int}
///   13. [int]                + N × {int key, int expansionRank, int subCount, subCount × int}
///
/// CsBase::sINFO layout (40 bytes, default Visual C++ alignment, __int64 forces 8-byte align):
///   0    DWORD    s_dwID
///   4    USHORT   s_nLevel
///   6    pad 2
///   8    __int64  s_dwExp
///   16   int      s_nHP
///   20   int      s_nDS
///   24   USHORT   s_nMoveSpeed / Defence / Evasion / Critical / Attack / HitRate (12 B)
///   36   pad 4 (struct trailing)
/// </summary>
public sealed class DMBaseBinLoader
{
    private const string FileName = "DMBase.bin";

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
        var tamerStats = ReadStatSection(r);
        var digimonStats = ReadStatSection(r);
        var mapInfo = ReadMapInfo(r);
        var jumpBuster = ReadJumpBuster(r);
        var party = new DMBasePartyConfig(r.ReadSingle());
        var guildLevels = ReadGuildLevels(r);
        var limit = ReadLimit(r);
        var personStore = ReadPersonStore(r);
        var playPenalty = ReadPlayPenalty(r);
        var evoStageApply = ReadEvolutionStageApply(r);
        var digiEvoMaxLevel = ReadDigimonEvoMaxLevel(r);
        var dskillOpen = ReadDskillOpenExpansion(r);

        return new DMBase(
            tamerStats, digimonStats, mapInfo, jumpBuster,
            party, guildLevels, limit, personStore, playPenalty,
            evoStageApply, digiEvoMaxLevel, dskillOpen);
    }

    private static Dictionary<int, DMBaseStatRecord> ReadStatSection(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBaseStatRecord>(count);
        for (int i = 0; i < count; i++)
        {
            int id = r.ReadInt32();
            ushort level = r.ReadUInt16();
            _ = r.ReadUInt16();           // pad
            long exp = r.ReadInt64();
            int hp = r.ReadInt32();
            int ds = r.ReadInt32();
            ushort ms = r.ReadUInt16();
            ushort def = r.ReadUInt16();
            ushort ev = r.ReadUInt16();
            ushort cr = r.ReadUInt16();
            ushort at = r.ReadUInt16();
            ushort ht = r.ReadUInt16();
            _ = r.ReadInt32();            // trailing pad
            // Tamer-side has known dups in some pack data per BaseMng._LoadFilePack — keep first.
            if (!map.ContainsKey(id))
                map[id] = new DMBaseStatRecord(id, level, exp, hp, ds, ms, def, ev, cr, at, ht);
        }
        return map;
    }

    private static Dictionary<int, DMBaseMapInfo> ReadMapInfo(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBaseMapInfo>(count);
        for (int i = 0; i < count; i++)
        {
            uint mapId = r.ReadUInt32();
            uint shoutSec = r.ReadUInt32();
            byte enableMacroByte = r.ReadByte();
            _ = r.ReadBytes(3);           // pad to 12 (struct alignment)
            map[(int)mapId] = new DMBaseMapInfo((int)mapId, shoutSec, enableMacroByte != 0);
        }
        return map;
    }

    private static Dictionary<int, IReadOnlyList<int>> ReadJumpBuster(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, IReadOnlyList<int>>(count);
        for (int i = 0; i < count; i++)
        {
            int itemId = (int)r.ReadUInt32();
            int destCount = r.ReadInt32();
            var dests = new int[destCount];
            for (int j = 0; j < destCount; j++)
                dests[j] = (int)r.ReadUInt32();
            map[itemId] = dests;
        }
        return map;
    }

    private static Dictionary<int, DMBaseGuildLevelConfig> ReadGuildLevels(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBaseGuildLevelConfig>(count);
        for (int i = 0; i < count; i++)
        {
            int lvl = r.ReadInt32();
            uint fame = r.ReadUInt32();
            int itemNo1 = r.ReadInt32();
            int itemCount1 = r.ReadInt32();
            int itemNo2 = r.ReadInt32();
            int itemCount2 = r.ReadInt32();
            int masterLevel = r.ReadInt32();
            int needPerson = r.ReadInt32();
            int maxGuildPerson = r.ReadInt32();
            int incMember = r.ReadInt32();
            int maxGuild2Master = r.ReadInt32();
            map[lvl] = new DMBaseGuildLevelConfig(
                lvl, fame, itemNo1, itemCount1, itemNo2, itemCount2,
                masterLevel, needPerson, maxGuildPerson, incMember, maxGuild2Master);
        }
        return map;
    }

    private static DMBaseLimit ReadLimit(BinaryReader r)
    {
        short maxTH = r.ReadInt16();
        short maxWH = r.ReadInt16();
        short union_ = r.ReadInt16();
        short maxShare = r.ReadInt16();
        uint consumeXG = r.ReadUInt32();
        uint chargeXG = r.ReadUInt32();
        return new DMBaseLimit(maxTH, maxWH, union_, maxShare, consumeXG, chargeXG);
    }

    private static DMBasePersonStore ReadPersonStore(BinaryReader r)
    {
        float personCharge = r.ReadSingle();
        float emplCharge = r.ReadSingle();
        float storeDist = r.ReadSingle();
        // CONSIGNMENT_CREATE_RESTRICT defined would add s_fNpcDist (4 B) here in newer builds;
        // v487 doesn't have it — sBASE_INFO is 12 bytes total.
        int objCount = r.ReadInt32();
        var objs = new List<DMBasePersonStoreObject>(objCount);
        for (int i = 0; i < objCount; i++)
        {
            int itemId = r.ReadInt32();
            int digimonId = r.ReadInt32();
            float scale = r.ReadSingle();
            int slotCount = r.ReadInt32();
            // CONSIGNMENT_BALLOON_CASH defined — trailing TCHAR fileName[64] = 128 bytes (skipped)
            _ = r.ReadBytes(128);
            objs.Add(new DMBasePersonStoreObject(itemId, digimonId, scale, slotCount));
        }
        return new DMBasePersonStore(personCharge, emplCharge, storeDist, objs);
    }

    private static Dictionary<int, DMBasePlayPenalty> ReadPlayPenalty(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBasePlayPenalty>(count);
        for (int i = 0; i < count; i++)
        {
            int level = r.ReadInt32();
            int exp = r.ReadInt32();
            int drop = r.ReadInt32();
            map[level] = new DMBasePlayPenalty(level, exp, drop);
        }
        return map;
    }

    private static Dictionary<int, int> ReadEvolutionStageApply(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, int>(count);
        for (int i = 0; i < count; i++)
        {
            int key = r.ReadInt32();
            SkipWString(r);            // stage name (e.g. "Digi-Tama", "Rookie") — UI-only
            int applyValue = r.ReadInt32();
            map[key] = applyValue;
        }
        return map;
    }

    private static Dictionary<int, DMBaseDigimonEvoMaxLevel> ReadDigimonEvoMaxLevel(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBaseDigimonEvoMaxLevel>(count);
        for (int i = 0; i < count; i++)
        {
            int key = r.ReadInt32();
            int skillExpStartLv = r.ReadInt32();
            int subCount = r.ReadInt32();
            var subVals = new int[subCount];
            for (int j = 0; j < subCount; j++) subVals[j] = r.ReadInt32();
            map[key] = new DMBaseDigimonEvoMaxLevel(key, skillExpStartLv, subVals);
        }
        return map;
    }

    private static Dictionary<int, DMBaseDskillOpenExpansion> ReadDskillOpenExpansion(BinaryReader r)
    {
        int count = r.ReadInt32();
        var map = new Dictionary<int, DMBaseDskillOpenExpansion>(count);
        for (int i = 0; i < count; i++)
        {
            int key = r.ReadInt32();
            int rank = r.ReadInt32();
            int subCount = r.ReadInt32();
            var allowed = new int[subCount];
            for (int j = 0; j < subCount; j++) allowed[j] = r.ReadInt32();
            map[key] = new DMBaseDskillOpenExpansion(key, rank, allowed);
        }
        return map;
    }

    /// <summary>Skip a <c>std::wstring</c>: read int charCount, advance charCount × 2 bytes.</summary>
    private static void SkipWString(BinaryReader r)
    {
        int charCount = r.ReadInt32();
        if (charCount > 0)
            r.BaseStream.Seek(charCount * 2, SeekOrigin.Current);
    }
}
