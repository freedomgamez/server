namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// In-memory <c>CashShop.bin</c> contents — flat product catalog keyed by
/// <c>dwProductID</c>. Server uses this as the authoritative source for purchase
/// validation: client-asserted <c>(productIDX, totalPrice)</c> pairs are looked
/// up here and rejected if unknown / inactive / outside window / price mismatch.
/// <para>
/// Source layout (<c>LibProj/CsFileTable/CashShopMng.cpp:_Load_Pack</c>) is a
/// nested MainCategory → SubCategory → ProductGroup → Product hierarchy plus a
/// trailing WebData blob. The hierarchy carries display-only categorization
/// metadata; the server flattens it to a dictionary because catalog lookup is
/// the only operation it performs.
/// </para>
/// </summary>
public sealed class CashShopTable
{
    public IReadOnlyDictionary<uint, CashShopProduct> ByProductId { get; }

    public CashShopTable(IReadOnlyDictionary<uint, CashShopProduct> byProductId)
    {
        ByProductId = byProductId;
    }
}

/// <summary>One purchasable cash-shop product (<c>sCashshop_Item</c>).</summary>
/// <param name="ProductId">Wire key — client sends this in MultiBuy/Gift/etc.</param>
/// <param name="CashType"><c>1 = Cash</c> (debits Premium first), <c>2 = Bonus</c> (debits Silk first).</param>
/// <param name="Price"><c>nRealSellingPrice</c> after discount — what the server compares to the client's totalPrice.</param>
/// <param name="StandardPrice"><c>nStandardSellingPrice</c> — pre-discount; informational only.</param>
/// <param name="Active">Master enable flag set in the catalog editor.</param>
/// <param name="StartTime">Active-window start. Empty / unparseable → <c>DateTime.MinValue</c>.</param>
/// <param name="EndTime">Active-window end. Empty / unparseable → <c>DateTime.MaxValue</c>.</param>
/// <param name="PackageItems">Items granted on purchase.</param>
public sealed record CashShopProduct(
    uint ProductId,
    int CashType,
    int Price,
    int StandardPrice,
    bool Active,
    DateTime StartTime,
    DateTime EndTime,
    IReadOnlyList<CashShopPackageItem> PackageItems);

/// <summary><c>sPackageItem</c> — one item to grant per purchase.</summary>
public sealed record CashShopPackageItem(uint ItemId, int Count);
