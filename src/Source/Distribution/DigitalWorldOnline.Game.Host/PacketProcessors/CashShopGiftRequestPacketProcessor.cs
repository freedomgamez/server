using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.GameServer;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pCashShop::Gift</c> (3403). Wire layout from
    /// <c>cCliGameShop.cpp:SendGiftCashItem</c>:
    /// <code>
    ///   n4    nPrice
    ///   n4    nProductIDX
    ///   wstring szPeerTamerName
    ///   WORD  trailing-protocol-dup           ; copy-paste bug in client; consume + ignore
    /// </code>
    /// <para>
    /// The trailing WORD is unintended — client's <c>SendGiftCashItem</c> ends with
    /// <c>p.push(packet.GetProtocol())</c>, pushing the protocol number into the body.
    /// Server consumes it so the next packet's framing isn't disturbed.
    /// </para>
    /// <para>Server flow:</para>
    /// <list type="number">
    /// <item>Resolve peer by <see cref="GameServer.FindByTamerName"/>. Offline-peer
    ///   gifting is not supported (see class header for rationale) — reply
    ///   <c>ResultPeerNotFound</c> if the peer isn't connected.</item>
    /// <item>Reject self-gift to avoid the "free cash" exploit pattern.</item>
    /// <item>Catalog lookup + active/window check (mirrors the MultiBuy validation).</item>
    /// <item>Price validation against the catalog's <c>RealPrice</c>.</item>
    /// <item>Balance check on the SENDER (combined Premium+Silk).</item>
    /// <item>Debit sender, grant package items to the PEER's
    ///   <c>AccountCashWarehouse</c>; refund sender on full-warehouse failure.</item>
    /// <item>Persist sender's debit + peer's warehouse update; reply to sender.</item>
    /// </list>
    /// </summary>
    public class CashShopGiftRequestPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.CashShopGiftRequest;

        private readonly CashShopBinLoader _catalog;
        private readonly AssetsLoader _assets;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public CashShopGiftRequestPacketProcessor(
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
            int nPrice = packet.ReadInt();
            int nProductIDX = packet.ReadInt();
            string peerName = packet.ReadString();
            // Trailing WORD copy-paste-bug from SendGiftCashItem — consume two bytes if present.
            try { _ = packet.ReadShort(); } catch { /* unread is fine */ }

            if (string.IsNullOrWhiteSpace(peerName))
            {
                Reply(client, CashShopGiftResponsePacket.ResultPeerNotFound, peerName, nProductIDX);
                return;
            }
            if (peerName.Equals(client.Tamer?.Name, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warning("CashShop Gift: tamer {TamerId} attempted self-gift; rejecting.", client.TamerId);
                Reply(client, CashShopGiftResponsePacket.ResultPeerNotFound, peerName, nProductIDX);
                return;
            }

            var peer = client.Server.FindByTamerName(peerName);
            if (peer == null)
            {
                _logger.Information(
                    "CashShop Gift: tamer {TamerId} → peer '{Peer}' not online; gift rejected.",
                    client.TamerId, peerName);
                Reply(client, CashShopGiftResponsePacket.ResultPeerNotFound, peerName, nProductIDX);
                return;
            }

            if (!_catalog.Data.ByProductId.TryGetValue((uint)nProductIDX, out var product))
            {
                _logger.Warning("CashShop Gift: tamer {TamerId} requested unknown productID {Pid}.",
                    client.TamerId, nProductIDX);
                Reply(client, CashShopGiftResponsePacket.ResultUnknownProduct, peerName, nProductIDX);
                return;
            }

            // Gate on Active only — same rationale as MultiBuy (v487 bin's
            // StartTime/EndTime windows are stale).
            if (!product.Active)
            {
                Reply(client, CashShopGiftResponsePacket.ResultUnknownProduct, peerName, nProductIDX);
                return;
            }

            if (product.Price != nPrice)
            {
                _logger.Warning(
                    "CashShop Gift: tamer {TamerId} price mismatch — client={Client} catalog={Server}.",
                    client.TamerId, nPrice, product.Price);
                Reply(client, CashShopGiftResponsePacket.ResultPriceMismatch, peerName, nProductIDX);
                return;
            }

            int totalCash = client.Premium + client.Silk;
            if (totalCash < product.Price)
            {
                Reply(client, CashShopGiftResponsePacket.ResultInsufficientCash, peerName, nProductIDX);
                return;
            }

            // Debit sender (Premium first, Silk spillover)
            int needed = product.Price;
            int debitPremium = Math.Min(client.Premium, needed);
            client.AddPremium(-debitPremium);
            needed -= debitPremium;
            if (needed > 0)
                client.AddSilk(-needed);

            // Grant items to peer's AccountCashWarehouse. Pad placeholders + set
            // ItemListId on each grant — same rationale as MultiBuy: a fresh peer
            // account's CashWarehouse loads with Items=[], and ItemDTO inserts need
            // a valid FK to Shared_ItemList.
            var peerWarehouse = peer.Tamer.AccountCashWarehouse;
            EnsurePlaceholderSlots(peerWarehouse);
            var rollback = new List<ItemModel>();
            bool allPlaced = true;
            foreach (var pkg in product.PackageItems)
            {
                if (pkg.ItemId == 0 || pkg.Count == 0) continue;
                var info = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == (int)pkg.ItemId);
                if (info == null)
                {
                    allPlaced = false;
                    break;
                }
                var item = new ItemModel();
                item.SetItemInfo(info);
                item.ItemId = (int)pkg.ItemId;
                item.Amount = pkg.Count;
                item.ItemListId = peerWarehouse.Id;
                if (item.IsTemporary)
                    item.SetRemainingTime((uint)item.ItemInfo.UsageTimeMinutes);
                if (!peerWarehouse.AddItem(item))
                {
                    allPlaced = false;
                    break;
                }
                rollback.Add(item);
            }

            if (!allPlaced)
            {
                // Refund sender + rollback peer warehouse
                client.AddPremium(debitPremium);
                if (product.Price - debitPremium > 0)
                    client.AddSilk(product.Price - debitPremium);
                foreach (var item in rollback)
                    peerWarehouse.RemoveOrReduceItem(item, item.Amount, item.Slot);

                Reply(client, CashShopGiftResponsePacket.ResultUnknownProduct, peerName, nProductIDX);
                return;
            }

            await _sender.Send(new UpdatePremiumAndSilkCommand(client.Premium, client.Silk, client.AccountId));
            await _sender.Send(new UpdateItemsCommand(peerWarehouse));

            _logger.Information(
                "CashShop Gift: tamer {TamerId} gifted product {Pid} ({Price} cash) to '{Peer}'.",
                client.TamerId, nProductIDX, product.Price, peerName);

            Reply(client, CashShopGiftResponsePacket.ResultSuccess, peerName, nProductIDX);
        }

        private static void Reply(GameClient client, ushort result, string peerName, int productId)
        {
            client.Send(new CashShopGiftResponsePacket(
                result, client.Premium, client.Silk, peerName ?? string.Empty, productId));
        }

        private static void EnsurePlaceholderSlots(ItemListModel? list)
        {
            if (list == null) return;
            while (list.Items.Count < list.Size)
            {
                int prevMax = list.Items.Any() ? list.Items.Max(x => x.Slot) : -1;
                var placeholder = new ItemModel(prevMax);
                placeholder.ItemListId = list.Id;
                list.Items.Add(placeholder);
            }
        }
    }
}
