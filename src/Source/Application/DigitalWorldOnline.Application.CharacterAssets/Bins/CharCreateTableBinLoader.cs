using System.Text;

namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

/// <summary>
/// Parses <c>CharCreateTable.bin</c> using the <i>variable-length</i> layout produced by
/// <c>CsCharCreateTableMng::SaveBin</c> in <c>LibProj/CsFileTable/CharacterCreateListInfo.cpp</c>.
///
/// Section 1 — tamers:
///   [int count]
///   per tamer: [u32 id][u8 bShow][u8 bEnable][int seasonType][string voiceFile]
///              [int iconIdx][int costumeCount][u32 × costumeCount]
///
/// Section 2 — starter digimon:
///   [int count]
///   per digimon: [u32 id][u8 bShow][u8 bEnable][string voiceFile]
///
/// The <c>std::string</c> serialization is <c>[int sizeBytes][sizeBytes raw bytes]</c> — empty
/// strings (the v487 default for these voice files) are 4 zero bytes. Records have no fixed
/// size; we read sequentially via <see cref="BinaryReader"/>.
/// </summary>
public sealed class CharCreateTableBinLoader
{
    private const string FileName = "CharCreateTable.bin";

    private CharCreateTable? _data;

    public CharCreateTable Data => _data ?? throw new InvalidOperationException(
        $"{nameof(CharCreateTableBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public CharCreateTable Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static CharCreateTable Parse(BinaryReader r)
    {
        int tamerCount = r.ReadInt32();
        var tamers = new List<CharCreateTamerSlot>(tamerCount);
        for (int i = 0; i < tamerCount; i++)
        {
            int model = r.ReadInt32();
            bool show = r.ReadByte() != 0;
            bool enable = r.ReadByte() != 0;
            int season = r.ReadInt32();
            string voice = ReadStdString(r);
            int icon = r.ReadInt32();
            int costumeCount = r.ReadInt32();
            var costumes = new int[costumeCount];
            for (int j = 0; j < costumeCount; j++)
                costumes[j] = r.ReadInt32();
            tamers.Add(new CharCreateTamerSlot(model, show, enable, season, voice, icon, costumes));
        }

        int digimonCount = r.ReadInt32();
        var digimon = new List<CharCreateStarterDigimon>(digimonCount);
        for (int i = 0; i < digimonCount; i++)
        {
            int model = r.ReadInt32();
            bool show = r.ReadByte() != 0;
            bool enable = r.ReadByte() != 0;
            string voice = ReadStdString(r);
            digimon.Add(new CharCreateStarterDigimon(model, show, enable, voice));
        }

        return new CharCreateTable(tamers, digimon);
    }

    /// <summary>
    /// Read a <c>std::string</c> as it's serialized by <c>CmUtil::CmBinSave::Save_Value</c>:
    /// <c>[int sizeBytes][sizeBytes char data]</c>. Voice filenames in CharCreateTable.bin are
    /// ASCII filenames (or empty in v487), so UTF-8 decode is safe even though the original
    /// DMO pipeline used cp949 — ASCII content is identical in both.
    /// </summary>
    private static string ReadStdString(BinaryReader r)
    {
        int size = r.ReadInt32();
        if (size <= 0) return string.Empty;
        var bytes = r.ReadBytes(size);
        return Encoding.UTF8.GetString(bytes);
    }
}
