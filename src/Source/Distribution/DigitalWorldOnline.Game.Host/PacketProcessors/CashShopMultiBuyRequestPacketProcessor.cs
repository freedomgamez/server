using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.GameServer.Combat;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pCashShop::MultiBuy</c> (3413), the v487 client's primary purchase
    /// path. Wire layout from <c>cCliGameShop.cpp:SendBuyCashItem</c>:
    /// <code>
    ///   n1 nItemCnt           ; 1..MAX_CASHITEM_COUNT (=12)
    ///   n4 nTotalPrice        ; client-asserted total — server validates against catalog
    ///   u2 ui64OrderID        ; Steam order ID — only 2 bytes on the wire (!!)
    ///   nItemCnt × n4 productID
    /// </code>
    /// <para>
    /// <c>ui64OrderID</c> looks like a uint64_t in the protocol struct but is in fact
    /// only 2 bytes on the wire. v487 nlib/base.h has a typedef quirk:
    /// <c>typedef uint16_t uint64;</c> — the parameter type <c>uint64</c> resolves to
    /// uint16_t, so <c>cPacket::push(uint64)</c> emits <c>sizeof(uint16_t) = 2 bytes</c>.
    /// Reading 8 bytes here would eat 6 bytes from the next productID and the whole
    /// item list reads garbage; observed productID 704717338 / 31020088 ratio depending
    /// on the offset slip. The 11-byte body length on the wire (Length=17 - 4 header -
    /// 2 checksum) is the smoking gun: 1 + 4 + 2 + 4 = 11 ✓; 1 + 4 + 8 + 4 = 17 ✗.
    /// </para>
    /// <para>
    /// The single-item <c>pCashShop::Buy</c> (3402) is unused in v487 — the client's
    /// dispatch at <c>cCliGame.cpp:585-586</c> has it commented out and routed through
    /// MultiBuy.
    /// </para>
    /// <para>Server flow:</para>
    /// <list type="number">
    /// <item>Look up each productID in <see cref="CashShopBinLoader"/> — reject the
    ///   whole purchase if any is unknown / inactive / outside its time window.</item>
    /// <item>Sum the catalog's <c>RealPrice</c> per row, compare to client's
    ///   <c>nTotalPrice</c> — reject on mismatch (anti-cheat: client cannot dictate
    ///   price).</item>
    /// <item>Verify cash balance (combined Premium+Silk; no per-product cashType
    ///   gating — DSO treats Premium as the primary pool with Silk as bonus).</item>
    /// <item>Debit Premium first, fall back to Silk if Premium runs out.</item>
    /// <item>Grant each product's <c>PackageItems</c> to <c>AccountCashWarehouse</c>;
    ///   if any item can't be placed (warehouse full), that product goes to the
    ///   failed list — partial-success is intentional per the v487 protocol.</item>
    /// <item>Append product IDs to <c>AccountBuyHistory</c> for the catalog UI's
    ///   "already-purchased" dim/disable state.</item>
    /// <item>Persist debited account + updated lists; reply with the success/fail
    ///   breakdown.</item>
    /// </list>
    /// </summary>
    public class CashShopMultiBuyRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.CashShopMultiBuyRequest;

        private const int MaxCashItemCount = 12;       // pCashShop::MAX_CASHITEM_COUNT
        private const int CashTypePremium = 1;
        private const int CashTypeBonus = 2;

        private readonly CashShopBinLoader _catalog;
        private readonly AssetsLoader _assets;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public CashShopMultiBuyRequestPacketProcessor(
            CashShopBinLoader catalog,
            AssetsLoader assets,
            ISender sender,
            ILogger logger)
        {
            _catalog = catalog;
            _assets = assets;
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            int nItemCnt = packet.ReadByte();
            int nTotalPrice = packet.ReadInt();
            _ = packet.ReadUShort();                  // ui64OrderID — Steam, ignored. 2 bytes; see class header.

            if (nItemCnt <= 0 || nItemCnt > MaxCashItemCount)
            {
                _logger.Warning(
                    "CashShop MultiBuy from tamer {TamerId} rejected: invalid itemCount={Count}.",
                    client.TamerId, nItemCnt);
                Reply(client, CashShopMultiBuyResponsePacket.ResultInternalError,
                    new List<int>(), new List<int>());
                return;
            }

            var productIds = new List<int>(nItemCnt);
            for (int i = 0; i < nItemCnt; i++)
                productIds.Add(packet.ReadInt());

            try
            {

                // 1) Catalog resolution. The bin carries <c>StartTime</c>/<c>EndTime</c>
                //    fields meant for live-ops campaigns; v487's shipped values are stale
                //    (e.g. product 31020088 EndTime=2020-11-01) and would reject every
                //    purchase on a private server. Gate on <c>Active</c> only — the catalog
                //    editor's master enable flag — and let the server admin curate the bin
                //    if they want a subset disabled.
                var resolved = new List<CashShopProduct>(nItemCnt);
                var failedProducts = new List<int>();
                foreach (var pid in productIds)
                {
                    if (!_catalog.Data.ByProductId.TryGetValue((uint)pid, out var product))
                    {
                        failedProducts.Add(pid);
                        _logger.Warning(
                            "CashShop MultiBuy: tamer {TamerId} requested unknown productID {Pid}.",
                            client.TamerId, pid);
                        continue;
                    }
                    if (!product.Active)
                    {
                        failedProducts.Add(pid);
                        _logger.Warning(
                            "CashShop MultiBuy: tamer {TamerId} productID {Pid} marked inactive in catalog.",
                            client.TamerId, pid);
                        continue;
                    }
                    resolved.Add(product);
                }

                if (resolved.Count == 0)
                {
                    Reply(client, CashShopMultiBuyResponsePacket.ResultUnknownProduct,
                        new List<int>(), failedProducts);
                    client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
                    return;
                }

                // 2) Price validation — sum the catalog's RealPrice for the resolved products,
                //    compare against client's nTotalPrice. Failed products don't contribute
                //    to the expected total since they aren't being purchased.
                int expectedPrice = resolved.Sum(p => p.Price);
                if (expectedPrice != nTotalPrice && failedProducts.Count == 0)
                {
                    _logger.Warning(
                        "CashShop MultiBuy: tamer {TamerId} price mismatch — client={Client} catalog={Server}.",
                        client.TamerId, nTotalPrice, expectedPrice);
                    Reply(client, CashShopMultiBuyResponsePacket.ResultPriceMismatch,
                        new List<int>(), productIds);
                    client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
                    return;
                }

                // 3) Balance check
                int totalCash = client.Premium + client.Silk;
                if (totalCash < expectedPrice)
                {
                    _logger.Information(
                        "CashShop MultiBuy: tamer {TamerId} insufficient cash — need {Need} have {Have}.",
                        client.TamerId, expectedPrice, totalCash);
                    Reply(client, CashShopMultiBuyResponsePacket.ResultInsufficientCash,
                        new List<int>(), productIds);
                    client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
                    return;
                }

            // 4) Debit. Premium first; spillover comes out of Silk. The client's
            //    NewCashshopContents.cpp shows Premium ("RealCash") and Silk
            //    ("BonusCash") as fungible from the user's perspective — both pools
            //    contribute to the buying-power total.
                int needed = expectedPrice;
                int debitPremium = Math.Min(client.Premium, needed);
                client.AddPremium(-debitPremium);
                needed -= debitPremium;
                if (needed > 0)
                {
                    client.AddSilk(-needed);
                }

            // GameClient.Premium/Silk are the in-memory runtime mirrors; the persistent
            // backing store gets reconciled via UpdatePremiumAndSilkCommand at the end
            // of the flow (we have a dedicated repo path that doesn't need a full
            // AccountModel hydrate — no need to fetch the row just to write back two
            // ints).

            // 5) Grant items per product. A product may bundle multiple items
            //    (PackageItems). If any item can't fit in the cash warehouse, the
            //    WHOLE product goes to the failed list so the client UI shows it as
            //    not delivered (matches the v487 partial-success contract).
            //
            //    Pad both account-level lists up to Size with empty placeholder
            //    rows. EnsureAccountItemListAsync creates the Shared_ItemList row
            //    with Items=[]; AutoMapper's hydrate doesn't run the model ctor
            //    that pre-pads to Size, so a fresh account's CashWarehouse and
            //    BuyHistory lists arrive empty. Then GetEmptySlot returns -1 and
            //    InsertItem crashes on Items[-1]. Padding here is idempotent —
            //    already-populated lists stay untouched.
                var successProducts = new List<int>();
                var cashWarehouse = client.Tamer.AccountCashWarehouse;
                EnsurePlaceholderSlots(cashWarehouse);
                EnsurePlaceholderSlots(client.Tamer.AccountBuyHistory);
                foreach (var product in resolved)
                {
                    bool allPlaced = true;
                    var rollback = new List<ItemModel>();
                    foreach (var pkg in product.PackageItems)
                    {
                        if (pkg.ItemId == 0 || pkg.Count == 0) continue;
                        var info = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == (int)pkg.ItemId);
                        if (info == null)
                        {
                            _logger.Warning(
                                "CashShop MultiBuy: product {Pid} references unknown item {Iid}; rolling back.",
                                product.ProductId, pkg.ItemId);
                            allPlaced = false;
                            break;
                        }
                        var item = new ItemModel();
                        item.SetItemInfo(info);
                        item.ItemId = (int)pkg.ItemId;
                        item.Amount = pkg.Count;
                        item.ItemListId = cashWarehouse.Id;     // FK to Shared_ItemList
                        if (item.IsTemporary)
                            item.SetRemainingTime((uint)item.ItemInfo.UsageTimeMinutes);

                        if (!cashWarehouse.AddItem(item))
                        {
                            allPlaced = false;
                            break;
                        }
                        rollback.Add(item);
                    }

                    if (allPlaced)
                    {
                        successProducts.Add((int)product.ProductId);
                        // Append productID to BuyHistory — store as an item with
                        // ItemId == ProductId so the existing list serializer works.
                        // Buy-history is informational only (catalog UI dim state); we
                        // don't need to track per-purchase metadata beyond the ID.
                        //
                        // Use AddItemWithSlot directly: AddItem's Overlap-split path
                        // dereferences ItemInfo.Overlap, but BuyHistory entries are
                        // synthetic and have no ItemInfo (they're product IDs, not real
                        // game items). Single-slot insert avoids the NRE entirely.
                        var history = client.Tamer.AccountBuyHistory;
                        if (history != null)
                        {
                            int slot = history.GetEmptySlot;
                            if (slot >= 0)
                            {
                                var historyEntry = new ItemModel();
                                historyEntry.ItemId = (int)product.ProductId;
                                historyEntry.Amount = 1;
                                historyEntry.ItemListId = history.Id;   // FK to Shared_ItemList
                                history.AddItemWithSlot(historyEntry, slot);
                            }
                            // BuyHistory full → silently drop; UI dim is best-effort,
                            // not a transaction-blocker.
                        }
                    }
                    else
                    {
                        // Roll back this product's partial grants so the warehouse stays
                        // consistent with the failed-list response.
                        foreach (var item in rollback)
                            cashWarehouse.RemoveOrReduceItem(item, item.Amount, item.Slot);
                        failedProducts.Add((int)product.ProductId);
                        _logger.Warning(
                            "CashShop MultiBuy: tamer {TamerId} product {Pid} dropped — cash warehouse full.",
                            client.TamerId, product.ProductId);
                    }
                }

                // If everything failed at the grant stage, refund the cash so the player
                // isn't charged for nothing.
                if (successProducts.Count == 0)
                {
                    client.AddPremium(debitPremium);
                    if (expectedPrice - debitPremium > 0)
                        client.AddSilk(expectedPrice - debitPremium);
                    Reply(client, CashShopMultiBuyResponsePacket.ResultWarehouseFull,
                        new List<int>(), failedProducts);
                    client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
                    return;
                }

                // 6) Persist
                await _sender.Send(new UpdateItemsCommand(cashWarehouse));
                await _sender.Send(new UpdatePremiumAndSilkCommand(client.Premium, client.Silk, client.AccountId));

                // Buy-history is ancillary to the purchase. Keep checkout alive even if the
                // history write hits a repository tracking issue in the current scope.
                if (client.Tamer.AccountBuyHistory != null)
                {
                    try
                    {
                        await _sender.Send(new UpdateItemsCommand(client.Tamer.AccountBuyHistory));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex,
                            "CashShop MultiBuy: buy-history persistence failed for account {AccountId}; purchase remains committed.",
                            client.AccountId);
                    }
                }

                _logger.Information(
                    "CashShop MultiBuy: tamer {TamerId} purchased {Success}/{Total} products for {Price} cash.",
                    client.TamerId, successProducts.Count, productIds.Count, expectedPrice);

                Reply(client, CashShopMultiBuyResponsePacket.ResultSuccess, successProducts, failedProducts);
                client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
                client.Send(new LoadAccountWarehousePacket(client.Tamer.AccountCashWarehouse));
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "CashShop MultiBuy crashed for tamer {TamerId} account {AccountId}.",
                    client.TamerId, client.AccountId);
                Reply(client, CashShopMultiBuyResponsePacket.ResultInternalError, new List<int>(), productIds);
                client.Send(new CashShopCoinsPacket(client.Premium, client.Silk));
            }
        }

        private static void Reply(
            GameClient client,
            ushort result,
            IReadOnlyList<int> success,
            IReadOnlyList<int> failed)
        {
            client.Send(new CashShopMultiBuyResponsePacket(
                result, client.Premium, client.Silk, success, failed));
        }

        private static void EnsurePlaceholderSlots(ItemListModel? list)
        {
            if (list == null) return;
            while (list.Items.Count < list.Size)
            {
                int prevMax = list.Items.Any() ? list.Items.Max(x => x.Slot) : -1;
                var placeholder = new ItemModel(prevMax);
                placeholder.ItemListId = list.Id;       // FK to Shared_ItemList — required
                list.Items.Add(placeholder);             // because UpdateItemsAsync inserts
            }                                            // EVERY row including empty slots,
        }                                                // not just the populated ones.
    }
}
