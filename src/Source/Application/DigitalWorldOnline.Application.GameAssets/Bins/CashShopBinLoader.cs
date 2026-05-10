namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>CashShop.bin</c> into the flat product dictionary the server uses
/// for purchase validation.
/// <para>
/// Layout from <c>LibProj/CsFileTable/CashShopMng.cpp:_Load_Pack</c>:
/// </para>
/// <code>
///   int nTableTypeCount                      ; 1 = Default, 2 = Default+Steam
///   for each TableType:
///     int nTableType
///     int nMainCount
///     for each Main:
///       int nMainType
///       wstring wsName
///       int nSubCount
///       for each Sub:
///         int nSubType
///         wstring wsName
///         int nProductGroupCount
///         for each ProductGroup:
///           int  dwItemGroupID
///           int  nProductCount
///           for each Product (sCashshop_Item):
///             wstring wsName
///             wstring wsComment
///             bool   bActive            (1 byte)
///             int    dwProductID
///             char[64] szStartTime
///             char[64] szEndTime
///             int    nPurchaseCashType  (1=Cash, 2=Bonus)
///             int    nStandardSellingPrice
///             int    nRealSellingPrice
///             int    nSalePersent
///             int    nIconID
///             int    nMaskType
///             int    nDispType
///             int    nDispCount
///             int    nPackageItemCount
///             nPackageItemCount × {int s_nItemID; int s_nItemCnt}
///   int nWebDataTableCount
///   for each WebTable:
///     int nTableType
///     int nListCount
///     for each Web entry:
///       string sWebImageFile
///       string sWebLinkUrl
/// </code>
/// <para>
/// Server keeps only the Default (<c>nTableType==0</c>) table — Steam catalog is
/// for the Steam build's pre-purchase flow which this server stubs. WebData is
/// purely UI metadata and is skipped by reading + discarding.
/// </para>
/// </summary>
public sealed class CashShopBinLoader
{
    private const string FileName = "CashShop.bin";
    private const int TableTypeDefault = 0;

    private CashShopTable? _data;

    public CashShopTable Data => _data ?? throw new InvalidOperationException(
        $"{nameof(CashShopBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public CashShopTable Load()
    {
        if (_data != null) return _data;

        var path = Path.Combine(BinPath.ResolveDirectory(), FileName);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        _data = Parse(reader);
        return _data;
    }

    public static CashShopTable Parse(BinaryReader r)
    {
        var products = new Dictionary<uint, CashShopProduct>();

        int tableTypeCount = r.ReadInt32();
        for (int t = 0; t < tableTypeCount; t++)
        {
            int tableType = r.ReadInt32();

            int mainCount = r.ReadInt32();
            for (int m = 0; m < mainCount; m++)
            {
                _ = r.ReadInt32();              // nMainType
                _ = ReadWString(r);             // main category name
                int subCount = r.ReadInt32();
                for (int s = 0; s < subCount; s++)
                {
                    _ = r.ReadInt32();          // nSubType
                    _ = ReadWString(r);         // sub category name
                    int groupCount = r.ReadInt32();
                    for (int g = 0; g < groupCount; g++)
                    {
                        _ = r.ReadInt32();      // dwItemGroupID
                        int productCount = r.ReadInt32();
                        for (int p = 0; p < productCount; p++)
                        {
                            var product = ReadProduct(r);
                            // Only the Default catalog drives server validation —
                            // Steam variants exist for the Steam build's pre-purchase
                            // flow which this server stubs.
                            if (tableType == TableTypeDefault &&
                                product.ProductId != 0 &&
                                !products.ContainsKey(product.ProductId))
                            {
                                products[product.ProductId] = product;
                            }
                        }
                    }
                }
            }
        }

        // WebData section — read + discard. The bin loader has to seek past it cleanly
        // to avoid leaving stale bytes in the stream that would confuse a future second
        // bin reading from the same Pack03 handle.
        if (r.BaseStream.Position < r.BaseStream.Length)
        {
            int webTableCount = r.ReadInt32();
            for (int t = 0; t < webTableCount; t++)
            {
                _ = r.ReadInt32();              // nTableType
                int n = r.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    _ = ReadAString(r);         // sWebImageFile
                    _ = ReadAString(r);         // sWebLinkUrl
                }
            }
        }

        return new CashShopTable(products);
    }

    // ---------------------------------------------------------------- helpers

    private static CashShopProduct ReadProduct(BinaryReader r)
    {
        _ = ReadWString(r);                       // wsName
        _ = ReadWString(r);                       // wsComment

        bool bActive = r.ReadByte() != 0;         // bool = 1 byte (MSVC sizeof(bool)==1)
        uint dwProductID = (uint)r.ReadInt32();

        var startTime = ParseFixedAsciiDate(r.ReadBytes(64), DateTime.MinValue);
        var endTime = ParseFixedAsciiDate(r.ReadBytes(64), DateTime.MaxValue);

        int cashType = r.ReadInt32();
        int standardPrice = r.ReadInt32();
        int realPrice = r.ReadInt32();
        _ = r.ReadInt32();                        // nSalePersent
        _ = r.ReadInt32();                        // nIconID
        _ = r.ReadInt32();                        // nMaskType
        _ = r.ReadInt32();                        // nDispType
        _ = r.ReadInt32();                        // nDispCount

        int packageItemCount = r.ReadInt32();
        var packageItems = new List<CashShopPackageItem>(Math.Max(0, packageItemCount));
        for (int i = 0; i < packageItemCount; i++)
        {
            uint itemId = (uint)r.ReadInt32();
            int itemCnt = r.ReadInt32();
            packageItems.Add(new CashShopPackageItem(itemId, itemCnt));
        }

        return new CashShopProduct(
            dwProductID,
            cashType,
            realPrice,
            standardPrice,
            bActive,
            startTime,
            endTime,
            packageItems);
    }

    /// <summary><c>std::wstring</c> framing per the bin-string-framing convention:
    /// <c>[int charCount][charCount × 2 bytes]</c>. Empty if charCount &lt;= 0.</summary>
    private static string ReadWString(BinaryReader r)
    {
        int charCount = r.ReadInt32();
        if (charCount <= 0) return string.Empty;
        var bytes = r.ReadBytes(charCount * 2);
        return System.Text.Encoding.Unicode.GetString(bytes);
    }

    /// <summary><c>std::string</c> framing per the bin-string-framing convention:
    /// <c>[int byteCount][byteCount bytes]</c>. Empty if byteCount &lt;= 0.</summary>
    private static string ReadAString(BinaryReader r)
    {
        int byteCount = r.ReadInt32();
        if (byteCount <= 0) return string.Empty;
        return System.Text.Encoding.UTF8.GetString(r.ReadBytes(byteCount));
    }

    /// <summary>Fixed-width <c>char[64]</c> ASCII window date. Trims trailing NULs and
    /// invalid sentinels; falls back to the supplied default on parse failure so a
    /// malformed bin doesn't poison the catalog.</summary>
    private static DateTime ParseFixedAsciiDate(byte[] buf, DateTime fallback)
    {
        int end = Array.IndexOf<byte>(buf, 0);
        if (end < 0) end = buf.Length;
        if (end == 0) return fallback;
        var s = System.Text.Encoding.ASCII.GetString(buf, 0, end).Trim();
        if (string.IsNullOrEmpty(s)) return fallback;
        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeLocal, out var dt))
            return dt;
        return fallback;
    }
}
