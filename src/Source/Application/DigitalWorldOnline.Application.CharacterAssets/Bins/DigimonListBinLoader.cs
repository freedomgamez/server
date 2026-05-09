namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

/// <summary>
/// Parses v487's <c>Digimon_List.bin</c> — single-section file of 634 × 572-byte records
/// (<c>CsDigimon::sINFO</c> from <c>LibProj/CsFileTable/Digimon.h</c>). Newer client builds add
/// CrossDigimon/Ride/etc. sections after this one, but v487's bin only contains section 1.
///
/// Layout (TCHAR = wchar_t = 2 bytes, MAX_FILENAME = 64, default Visual C++ alignment):
///   off  size  field
///   0    4     DWORD   s_dwDigimonID                  (= server's Type)
///   4    4     DWORD   s_dwModelID                    (= server's Model)
///   8    128   wchar   s_szName[64]
///   136  64    char    s_cSoundDirName[64]
///   200  4     float   s_fSelectScale
///   204  128   wchar   s_szEvoEffectDir[64]
///   332  4     enum    s_eEvolutionType
///   336  4     enum    s_eAttributeType               (server's Attribute)
///   340  12    enum[3] s_eFamilyType[3]               (server's Family1/2/3)
///   352  4     enum    s_eBaseNatureType              (server renamed this to "Element")
///   356  12    enum[3] s_eBaseNatureTypes[3]          (alt-nature list; primary already at 352)
///   368  4     DWORD   s_dwBaseLevel                  (server's Level)
///   372  20    USHORT[10] HP/DS/Def/Eva/MS/Crit/AttPow/AttSpd/AttRng/Hit
///   392  1     BYTE    s_nDigimonType
///   393  1     pad
///   394  2     USHORT  s_dwCharSize                   (server's ScaleType, low byte)
///   396  32    sSKILL[4]                              (DWORD id + int reqLvl per skill)
///   428  4     float   s_fWakkLen
///   432  4     float   s_fRunLen
///   436  4     float   s_fARunLen
///   440  128   wchar   s_szForm[64]
///   568  4     int     s_nDigimonRank
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

    /// <summary>
    /// Load Digimon_List.bin into memory. Pass <paramref name="typeFilter"/> to keep only the
    /// listed digimon types (Character.Host should pass the 4 selectable starter IDs from
    /// CharCreateTable.bin — there's no point holding 634 entries when only 4 are reachable
    /// from the create flow). Pass <c>null</c> to keep all entries.
    /// </summary>
    public DigimonList Load(IReadOnlySet<int>? typeFilter = null)
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        var bytes = File.ReadAllBytes(path);
        _data = Parse(bytes, typeFilter);
        return _data;
    }

    public static DigimonList Parse(ReadOnlySpan<byte> data, IReadOnlySet<int>? typeFilter = null)
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

        var map = new Dictionary<int, DigimonListEntry>(typeFilter?.Count ?? count);
        for (int i = 0; i < count; i++)
        {
            var rec = data.Slice(4 + i * RecordSize, RecordSize);
            int type = BitConverter.ToInt32(rec[0..4]);
            if (typeFilter is not null && !typeFilter.Contains(type)) continue;
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
        // s_cSoundDirName, s_fSelectScale, s_szEvoEffectDir intentionally skipped — not in DTO.
        int evolutionType = BitConverter.ToInt32(rec[332..336]);
        int attribute = BitConverter.ToInt32(rec[336..340]);
        int family1 = BitConverter.ToInt32(rec[340..344]);
        int family2 = BitConverter.ToInt32(rec[344..348]);
        int family3 = BitConverter.ToInt32(rec[348..352]);
        int element = BitConverter.ToInt32(rec[352..356]);   // bin's 's_eBaseNatureType' = server's 'Element'
        // s_eBaseNatureTypes[3] (offsets 356..368) — secondary elements, not in DTO.
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
        // s_nDigimonType (BYTE) at 392 — not in DTO.
        ushort scaleType = BitConverter.ToUInt16(rec[394..396]);
        // skills (396..428), wakk/run/arun lengths (428..440), form (440..568), rank (568..572)
        // are not in the server DTO — skipped.

        return new DigimonListEntry(
            Type: type,
            Model: model,
            Name: name,
            BaseLevel: (byte)baseLevel,
            ScaleType: (byte)scaleType,
            EvolutionType: evolutionType,
            Attribute: attribute,
            Element: element,
            Family1: family1,
            Family2: family2,
            Family3: family3,
            HP: hp, DS: ds, DefPower: def, Evasion: eva, MoveSpeed: ms,
            CriticalRate: crit, AttPower: att, AttSpeed: attSpd, AttRange: attRng, HitRate: hit);
    }

    private static string ReadWStringField(ReadOnlySpan<byte> field)
    {
        // 64-wide-char fixed buffer; trim at first null wchar (both bytes 0).
        int charLen = 0;
        for (int i = 0; i < field.Length; i += 2)
        {
            if (field[i] == 0 && field[i + 1] == 0) break;
            charLen++;
        }
        return System.Text.Encoding.Unicode.GetString(field[..(charLen * 2)]);
    }
}
