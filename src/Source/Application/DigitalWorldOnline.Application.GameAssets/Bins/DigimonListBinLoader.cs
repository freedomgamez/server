namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487's <c>Digimon_List.bin</c> — single-section file of 634 × 572-byte records
/// (<c>CsDigimon::sINFO</c> from <c>LibProj/CsFileTable/Digimon.h</c>). Game.Host needs
/// the FULL list (Character.Host's loader filters to the 4 selectable starters).
///
/// Layout (TCHAR = wchar_t = 2 bytes, MAX_FILENAME = 64, default Visual C++ alignment):
///   0    DWORD   s_dwDigimonID                  (= server's Type)
///   4    DWORD   s_dwModelID                    (= server's Model)
///   8    wchar   s_szName[64]                   (128 bytes UTF-16)
///   136  char    s_cSoundDirName[64]
///   200  float   s_fSelectScale
///   204  wchar   s_szEvoEffectDir[64]           (128 bytes)
///   332  enum    s_eEvolutionType
///   336  enum    s_eAttributeType               (server's Attribute)
///   340  enum[3] s_eFamilyType[3]               (server's Family1/2/3)
///   352  enum    s_eBaseNatureType              (server renamed this to "Element")
///   356  enum[3] s_eBaseNatureTypes[3]          (alt-nature list; primary already at 352)
///   368  DWORD   s_dwBaseLevel                  (server's Level)
///   372  USHORT[10] HP/DS/Def/Eva/MS/Crit/AttPow/AttSpd/AttRng/Hit
///   392  BYTE    s_nDigimonType
///   393  pad
///   394  USHORT  s_dwCharSize                   (server's ScaleType, low byte)
///   396  sSKILL[4]                              (DWORD id + int reqLvl per skill, 32 bytes)
///   428  float   s_fWakkLen
///   432  float   s_fRunLen
///   436  float   s_fARunLen
///   440  wchar   s_szForm[64]
///   568  int     s_nDigimonRank
///   = 572 bytes total
/// </summary>
public sealed class DigimonListBinLoader
{
    private const string FileName = "Digimon_List.bin";
    private const int RecordSize = 572;

    private DigimonList? _data;

    public DigimonList Data => _data ?? throw new InvalidOperationException(
        $"{nameof(DigimonListBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public DigimonList Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        var bytes = File.ReadAllBytes(path);
        _data = Parse(bytes);
        return _data;
    }

    public static DigimonList Parse(ReadOnlySpan<byte> data)
    {
        int count = BitConverter.ToInt32(data[..4]);
        int expected = 4 + count * RecordSize;
        if (expected != data.Length)
        {
            throw new InvalidDataException(
                $"Digimon_List.bin size mismatch: header says {count} records × {RecordSize}B " +
                $"+ 4B header = {expected}, but file is {data.Length} bytes. v487 should be a " +
                "single-section file; if you see this, the bin may be from a newer multi-section build.");
        }

        var map = new Dictionary<int, DigimonListEntry>(count);
        for (int i = 0; i < count; i++)
        {
            var rec = data.Slice(4 + i * RecordSize, RecordSize);
            var entry = ReadRecord(rec);
            // First-wins on duplicate Type — paranoia, the bin shouldn't have any.
            map.TryAdd(entry.Type, entry);
        }
        return new DigimonList(map);
    }

    private static DigimonListEntry ReadRecord(ReadOnlySpan<byte> rec)
    {
        int type = BitConverter.ToInt32(rec[0..4]);
        int model = BitConverter.ToInt32(rec[4..8]);
        string name = ReadWStringField(rec[8..(8 + 128)]);
        int evolutionType = BitConverter.ToInt32(rec[332..336]);
        int attribute = BitConverter.ToInt32(rec[336..340]);
        int family1 = BitConverter.ToInt32(rec[340..344]);
        int family2 = BitConverter.ToInt32(rec[344..348]);
        int family3 = BitConverter.ToInt32(rec[348..352]);
        int element = BitConverter.ToInt32(rec[352..356]);   // bin's 's_eBaseNatureType' = server's 'Element'
        uint baseLevel = BitConverter.ToUInt32(rec[368..372]);
        ushort hp = BitConverter.ToUInt16(rec[372..374]);
        ushort ds = BitConverter.ToUInt16(rec[374..376]);
        ushort def = BitConverter.ToUInt16(rec[376..378]);
        ushort eva = BitConverter.ToUInt16(rec[378..380]);
        ushort ms = BitConverter.ToUInt16(rec[380..382]);
        ushort crit = BitConverter.ToUInt16(rec[382..384]);
        ushort att = BitConverter.ToUInt16(rec[384..386]);
        ushort attSpd = BitConverter.ToUInt16(rec[386..388]);
        ushort attRng = BitConverter.ToUInt16(rec[388..390]);
        ushort hit = BitConverter.ToUInt16(rec[390..392]);

        // ScaleType is the *leveling-rank* key into DMBase.bin §2 (digimon-rank-level), NOT
        // the visual scale percentage. The client's `GetDigimonBase(level, digimonType)`
        // computes its lookup key as `level + digimonType * 1000` (`BaseMng.cpp:159`), and
        // 's_nDigimonType' (1..4) is the digimonType used. Server's ExpManager queries
        // `DigimonLevelInfo.ScaleType == digimon.BaseInfo.ScaleType`, so this byte must be
        // the rank (1..4), not s_dwCharSize (visual scale, 14..350). Loading the wrong field
        // here was a latent bug — digimon level-up looked up zero rows.
        byte digimonType = rec[392];
        // s_dwCharSize at offset 394 is intentionally not loaded (visual scale, server unused).

        // s_Skill[4] at offset 396, 4 × 8B = 32B.  Each slot is { DWORD s_dwID; int s_nReqPrevSkillLevel }.
        // Slot index in the array = client F1..F4 hotbar position; this is the per-digimon skill
        // loadout the server's DigimonSkillAssetsQuery resolves through.  Zero-id slots represent
        // unfilled / not-yet-unlocked slots — kept in the list (consumers compare against zero).
        var skills = new DigimonSkillSlot[4];
        for (int s = 0; s < 4; s++)
        {
            int o = 396 + s * 8;
            int skillId = BitConverter.ToInt32(rec[o..(o + 4)]);
            int reqPrev = BitConverter.ToInt32(rec[(o + 4)..(o + 8)]);
            skills[s] = new DigimonSkillSlot(skillId, reqPrev);
        }

        // s_fWakkLen/RunLen/ARunLen at 428/432/436 (animation distances) and s_szForm at 440
        // (UI form string) are intentionally skipped — client rendering only.
        int rank = BitConverter.ToInt32(rec[568..572]);

        return new DigimonListEntry(
            Type: type,
            Model: model,
            Name: name,
            BaseLevel: (byte)baseLevel,
            ScaleType: digimonType,
            EvolutionType: evolutionType,
            Attribute: attribute,
            Element: element,
            Family1: family1,
            Family2: family2,
            Family3: family3,
            HP: hp, DS: ds, DefPower: def, Evasion: eva, MoveSpeed: ms,
            CriticalRate: crit, AttPower: att, AttSpeed: attSpd, AttRange: attRng, HitRate: hit,
            Skills: skills,
            Rank: rank);
    }

    private static string ReadWStringField(ReadOnlySpan<byte> field)
    {
        int charLen = 0;
        for (int i = 0; i < field.Length; i += 2)
        {
            if (field[i] == 0 && field[i + 1] == 0) break;
            charLen++;
        }
        return System.Text.Encoding.Unicode.GetString(field[..(charLen * 2)]);
    }
}
