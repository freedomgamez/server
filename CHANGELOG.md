# CHANGELOG

Notable patches applied during the v487-client compatibility work. Grouped by area, not strictly chronological.

## ItemModelBehavior.ToArray — pack ItemId+Amount into the cItemData bitfield

The v487 client's `RecvInvenResult` (`cCliGameReceive.cpp:8505`) handles inventory + warehouse + sharestash through one path that memcpys each entry as `cItemData` — where `m_nType : 17 | m_nCount : 15` share a single 32-bit field (`m_nAll`). The pre-existing `ItemModelBehavior.ToArray` was writing `ItemId` and `Amount` as TWO separate `u4`s, so the client decoded `m_nType` correctly but `m_nCount` came out as 0 and the icon renderer fell back to "1" everywhere — gift CLAIM into inventory, cash-shop multi-buy quantity, normal shop buys with stack size > 1, etc. The same bug existed (and was already fixed) in `GiftToArray` for the gift-box path; this commit applies the same pack to `ToArray`. Bytes 0-3 now carry `((Amount & 0x7FFF) << 17) | (ItemId & 0x1FFFF)`; bytes 4-7 are zeroed where the old `Amount` u4 used to live; everything past byte 8 is unchanged (accessory / socket / expiry / power / level fields all stay at their previous offsets per the prior "don't reshape the rest of the struct" guidance).

Verified in-game: gift box claim transfers the bin's count correctly to inventory, cash-shop multi-buy with quantity-dropdown delivers the right stack size, existing inventory items already show their real counts on next reload.

## Phase 4 (start): Cash shop transaction pipeline

End-to-end purchase flow for the v487 cash shop window. The server is now authoritative on the catalog (item IDs, prices, what each product grants) so client-asserted prices are validated rather than trusted. Verified in-game: balance + buy-history populate on cash shop open, MultiBuy purchase debits cash, grants the package items to the cash warehouse, appends product ID to buy history, and the items survive a server restart.

### `CashShopBinLoader` — server-authoritative catalog

`Application.GameAssets/Bins/CashShopBinLoader.cs` parses the v487 `CashShop.bin` (5.16 MB). Two-section layout: nested catalog (TableType → MainCategory → SubCategory → ProductGroup → Product) and a trailing WebData blob. v487's `nlib/base.h` `typedef uint16_t uint64` quirk is documented in the loader so future protocol decoders that hit the same gotcha have a reference. The loader flattens to `Dictionary<uint productId, CashShopProduct>` keyed by `dwProductID`; only the Default table (TableType=0, 4016 products / 805 active) is indexed — Steam table (TableType=1) is parsed-and-discarded since this server stubs Steam pre-purchase. WebData section is read + discarded (UI-only).

### Packet processors (5 new + 1 enriched)

Wire formats verified against `common_vs2019/Protocol/CashShop_Protocol.h` and `cCliGameShop.cpp`:

- **`CashShopBalanceRequestPacketProcessor` (3404)** — empty C→S body; replies `CashShopCoinsPacket(Premium, Silk)`. The same packet type was already pushed unsolicited at login from `ComplementarInformationPacketProcessor`; this handles the client-initiated refresh on cash shop open + post-purchase.
- **`CashShopBuyHistoryRequestPacketProcessor` (3412)** — empty C→S; replies `CashShopBuyHistoryPacket(productIds)` with format `n1 result · n2 count · count×n4 productID`. Server projects `ItemId` from `client.Tamer.AccountBuyHistory.Items` rows.
- **`CashShopBuyRequestPacketProcessor` (3401)** — Steam pre-purchase handshake stub. Replies `(error=0, totalCash=Premium+Silk)` so the v487 client (`VERSION_USA`-built) falls into the standard MultiBuy purchase path on non-Steam servers.
- **`CashShopMultiBuyRequestPacketProcessor` (3413)** — primary purchase path. Wire layout confirmed by hex-dump verification: `n1 itemCnt · n4 totalPrice · u2 ui64OrderID · itemCnt×n4 productID` (11 bytes body for 1 product, NOT the 17 the protocol struct would suggest — see "uint64 typedef quirk" below). Flow: catalog resolution → Active flag check (window dates ignored — v487's bin values are stale 2017-2020) → price validation against `RealPrice` → balance check (combined Premium+Silk) → debit Premium first with Silk spillover → grant `PackageItems[]` to `AccountCashWarehouse` (per-product allPlaced/rollback) → append productIDs to `AccountBuyHistory` → persist. Reply: `u2 result · n4 realCash · n4 bonusCash · n1 successCnt + n4×successCnt · n1 failedCnt + n4×failedCnt`.
- **`CashShopGiftRequestPacketProcessor` (3403)** — gift to peer tamer. Wire: `n4 price · n4 productIDX · wstring peerTamerName · WORD trailingProtocolDup` (the trailing WORD is a copy-paste bug in the client's `SendGiftCashItem` that pushes the protocol number into the body; server consumes + ignores). Online-peer-only delivery — offline-peer gift routing is deferred. Same validation chain as MultiBuy; sender's debit + peer's warehouse grant. Reply echoes peer name + product ID.
- **`LoadAccountCashWarehousePacketProcessor` (3930)** — pre-existing handler enriched with the comment `pItem::CashShop` triggered by the cash warehouse refresh path.

### Wire-format gotchas captured

Two v487 quirks worth a permanent comment in the relevant processor headers:

- **`uint64` is 2 bytes on the wire**, not 8. `nlib/base.h:50` aliases `typedef uint16_t uint64;` (vs. `typedef uint64_t u8;` at line 43). Any protocol struct field declared `uint64` resolves to `uint16_t` at `cPacket::push` template instantiation, so `ui64OrderID` etc. emit 2 bytes. Fixed in `CashShopMultiBuyRequestPacketProcessor.Process` (`ReadUShort` not `ReadInt64`); diagnostic trick is to validate `Length` from the packet header against `4 (header) + body + 2 (checksum)` — if body math comes out 6 bytes short of what the struct would imply, suspect a `uint64` typedef hit.
- **Account-level `Shared_ItemList` rows created via `EnsureAccountItemListAsync` start with `Items=[]`** (the constructor that pads to `Size` with placeholder `ItemModel` rows runs only on the model-side ctor, not on AutoMapper hydrate from DB). For the cash warehouse + buy history, this means a fresh account's in-memory list has zero rows even though the parent `Shared_ItemList` row exists with a valid `Id`. `EnsurePlaceholderSlots` is run before `AddItem` to pad the list to `Size`, and each placeholder gets `ItemListId = list.Id` so the persistence layer's INSERT path (which inserts EVERY row including empty slots, not just populated ones) doesn't FK-violate. Also documented: only `Active` flag gates purchases server-side — the bin's `StartTime`/`EndTime` window fields are stale (e.g. product 31020088 EndTime=2020-11-01) and would reject every purchase if enforced.

### Catalog editor + transaction transparency

Bin-driven so the server admin can disable products by editing the bin's `Active` flag. No DB schema changes — `Account.Premium`/`Account.Silk` already existed, `AccountCashWarehouse` (`ItemListEnum.CashWarehouse=32`) and `AccountBuyHistory` (`ItemListEnum.BuyHistory=33`) already wired through `EnsureAccountItemListAsync`.

### Skipped (intentional)

- `pCashShop::Cart` (3405) / `CartSave` (3406) — explicitly disabled in the v487 client (`assert_csm(false, "장바구니 사용 안됨")`); never sent.
- `pCashShop::VIPAutoPayment` (3414) — separate VIP-membership subsystem, out of scope.

### Known gap (carry-over for next pass)

`UpdatePremiumAndSilkCommand` and `UpdateItemsCommand(cashWarehouse)` currently run as separate MediatR sends. If the items-persist fails (FK or otherwise) after the cash debit succeeds, cash is debited without items granted — observed during the FK debugging where Premium dropped 5000 → 4860 across two failed attempts before the placeholder/FK fix landed. Wrapping both in a transaction is queued for the next commit.

## Phase 3 follow-ups: TimeReward in-session timer, gift-box count fix, EF schema hygiene

Bug-fixes uncovered while testing the Phase 3 click-to-claim systems end-to-end against the live client.

### TimeReward timer no longer counts down while offline

The original DSO scaffolding stored only an absolute `StartTime` and computed remaining time as `StartTime - Now`, so the wall clock kept ticking the player toward their next reward while logged out. Replaced with an online-only model:

- New `TimeReward.RemainingSeconds` (DB column added in migration `20260510012518_AddTimeRewardRemainingSeconds`, default 1800 = First-tier 30 min) plus an in-memory `LastTickTime` (transient, `[NotMapped]`).
- New `TimeReward.Tick(now)` decrements `RemainingSeconds` by `(now - LastTickTime)` only when the player is in-session. First tick after model load (when `LastTickTime == DateTime.MinValue`) just seeds the timestamp without subtracting — that's how the timer pauses across the offline gap.
- `DailyEventService.TickAsync` now drives off `reward.Tick(now)` instead of comparing against absolute `StartTime`. `UpdateRewardIndex` resets `RemainingSeconds` to the next tier's duration on advance.
- `GameServer.cs` disconnect handler persists `RemainingSeconds` via `UpdateTamerTimeRewardCommand` so sub-threshold session progress survives logout. (Hard-crash mid-session still loses progress between last advance and crash; periodic save deferred until it becomes an issue at scale.)
- Legacy `StartTime` column kept for AutoMapper / DB schema survival; no longer drives any logic.

### Gift box count display: `cItemData` bitfield packing

The v487 client's gift-box recv (`cCliGameShop::RecvGiftShop`) memcpys a `cItemData` array directly off the wire (`common_vs2019/cItemData.h`). `cItemData` packs item type and count into a SINGLE 32-bit bitfield: `u4 m_nType : 17; u4 m_nCount : 15` (sharing the `m_nAll` union under MSVC `pragma pack(4)`). The pre-existing `ItemModelBehavior.GiftToArray` was writing ItemId and Amount as TWO separate `u4`s (8 bytes), so the client decoded `m_nType` correctly but `m_nCount` came out as 0 — daily-event and attendance gifts displayed as "1" (the icon renderer's 0/1 fallback). Fix: pack `((Amount & 0x7FFF) << 17) | (ItemId & 0x1FFFF)` into bytes 0-3 and zero bytes 4-7 where Amount used to live. Layout past byte 8 unchanged from the legacy DSO format — accessory/socket/expiry/rate/level fields are byte-for-byte identical to before.

### Other gift-box fixes
- New `ItemListModelBehavior.AddGiftItem` helper that wraps `AddItemWithSlot` so a gift item lands in a single empty slot at its full bin count, instead of being routed through `AddItem`'s overlap-split path that would fragment a 10× stack into ten 1× slots.
- `ItemModelBehavior.GiftToArray` derives the gift-box "remaining minutes" directly from `EndDate` instead of `RemainingMinutes()`. The latter gates by `ItemInfo.TemporaryItem` and returned 0 for non-temp items, making every gift display "expires now". `EndDate = Now + 14 days` at grant time gives the standard 14-day claim window; an `EndDate` set but in the past serializes to the legacy `0xFFFFFFFF` "expired" sentinel.

### EF schema hygiene: `Asset_EvolutionLine` keeps no bin-derived columns

`EvolutionLineAssetConfiguration` now `Ignore`s `SkillMaxLevels`. The field is bin-driven runtime data populated by `DigimonEvolutionAssetsQueryHandler` from `DMBase.bin §12`; it belongs to the in-memory DTO/Model only. `Asset_EvolutionLine` is DB-resident static data already staged for retirement once the `DigimonEvo.bin` migration completes, so adding a new column to it would defeat the static-data-off-MariaDB plan. The `EF.Ignore` drops it from the schema mapping; the property remains usable on the DTO surface that bin handlers populate.

## Phase 3 features: Hot Time, Daily Play-Time, Attendance click-to-claim

The bin loaders from the prior partial-Phase-3 commit unblocked the actual feature work driven by `Event.bin` §1, §2, and §5. All three click-to-claim or auto-grant systems landed in this commit, all wired to deliver into the player's gift box (`GiftWarehouse` — the v487 client's "event mail" surface) where appropriate. Verified end-to-end in-game.

### C9 — Hot Time (`pEvent::HotTimeEvent` 3134, `pEvent::HotTimeItemRequest` 3135)

Click-to-claim daily reward during a campaign + intra-day time-of-day window. Server picks today's entry (matching `DayOfWeek`) from `Event.bin` §5, sends `HotTimeEventInfoPacket` at login describing `(state, currentEventNo, nextEventNo, alreadyClaimed, startTimeLeftSec, endTimeLeftSec)`, and processes `HotTimeItemRequest` (handled by `HotTimeItemRequestPacketProcessor`) — validates the bin's window + intra-day gate + already-claimed-today, grants the bin's `(ItemId × Count)` to the cash warehouse, replies with the matching `nsHotTimeResult` code (0=success, 30597=already-claimed, 30598=not-time). v487 client at `EventContents.cpp:686` passes the int code straight to `cPrintMsg::PrintMsg` for the toast. Wire format verified against `GS2C_NTF_HOTTIME_EVENT_INFO` and `GS2C_RECV_HOTTIME_GET_RESULT` in `common_vs2019/Protocol/Event_Protocol.h`. Per-character claim ledger is in-memory only (`ConcurrentDictionary<(charId, eventNo), DateOnly>`) — survives map enters / channel switches but not server restart. Same posture as the tradeoff captured in `HotTimeService.cs`'s class header; persistence is queued to ride along with future per-character event-state schema work.

### C7 — Daily Play-Time (`pEvent::DailyEventInfo` 3106)

Server-driven auto-grant tier system: play 30/30/60/60 min today → four reward stacks delivered to `GiftWarehouse`. The dormant `TimeReward` scaffolding (`Event_TimeReward` table, EF mapping, `CharacterModelBehavior.UpdateTimeReward()`, `TimeRewardPacket`) was wired up — most of the infrastructure already existed in DSO but was never invoked. Specific fixes:

- `TimeRewardPacket.cs` wire format had three pre-existing bugs masking the panel display: was sending raw `RewardIndex (0..3)` as `nEventNo` so the client's `GetMap(ET_DAILY, 0)` lookup never resolved; was duplicating `RemainingTime` in place of `nTotalTime`; was hardcoding `nWeek = 1`. Fixed to send `(0..3 offset, max(0, RemainingTime), CurrentTotalSeconds, today's DayOfWeek)`. The offset detail matters: the client at `Event.cpp:981-984` keys `m_mapEvent` by `nType + nNO` with `ET_DAILY = 10000` as the type, so the server must send the offset within the daily-event group, NOT the full `TableNo`.
- `TimeReward` constructor was setting `StartTime = DateTime.Now`, making `RemainingTime ≈ 0` immediately at character creation (the panel would have shown "expired" forever). Now sets `StartTime = Now + First-tier duration` (30 min).
- Added `CurrentEventNo` and `CurrentTotalSeconds` getters on the model so `TimeRewardPacket` reads from one source of truth.
- New `DailyEventService` exposes `FindByOffset(0..N-1)` against `EventTableBinLoader.Data.Daily` (records sorted by `TableNo`) and `TickAsync(client)` for the per-tick advance check. Hooked into both `MapServerTamerOperation` and `DungeonsServerTamerOperation` per-tamer loops next to `CheckMonthlyReward`. When the threshold is reached: grants the bin's `EventDailyRecord.Rewards[]` to `GiftWarehouse` (event mail), advances the index, persists via the new `UpdateTamerTimeRewardCommand`, and re-broadcasts the panel.
- Login push uncommented at `ComplementarInformationPacketProcessor.cs:113-114` (was pre-existing TODO).

Verified in-game: server logs `DailyEvent 10001 fired for tamer 101298 → delivered 3 reward stack(s) to GiftWarehouse` on the first tick, items appear in the gift box.

### C6 — Attendance click-to-claim (`pEvent::Attendance` 3107)

Player clicks the in-world attendance button (`BGSprite.cpp:691-696`); server processes the request, advances `AttendanceReward.TotalDays`, looks up the bin's monthly reward for that day, delivers to `GiftWarehouse`, and replies with the `n4 nResCode + u4 nGiveItemNo + n4 nWorkDayHistory` payload the client unpacks at `cCliGameEvent.cpp:18-65`.

- New `AttendanceService` — singleton, holds the active-window dates from `appsettings:Attendance:Start`/`:End`. v487's bin window is stale 2017-03-15..2017-04-26 so config-driven is the right knob; `Attendance:Start`/`End` in `appsettings.Development.Example.json` left empty default to "always-open" for dev. The bin's value is loaded for visibility but not enforced server-side.
- New `AttendanceRequestPacketProcessor` — handles incoming 3107. Validates window + LastRewardDate-already-today, increments `TotalDays`, picks the day's reward via the bin-backed `MonthlyEventAssetsQuery`, grants to `GiftWarehouse`, persists.
- New `AttendanceResponsePacket` — error reply (just `nResCode`) and success reply (with `nGiveItemNo + nWorkDayHistory`). `nWorkDayHistory` derived as `(1 << TotalDays) - 1` for consecutive streaks; non-consecutive day tracking deferred until a real bitmap column is added.

Note: the v487 client gates `Send_Attendance` on `g_pDataMng->GetAttendance()->IsEnableAttendance()`, which reads the bin's `EventTable.Attendance` window — so the click button is grey until the bin's window covers today. End-to-end click flow verification will wait until a bin tool exists; server-side the wire format and validation paths are complete.

### Item-list zero-count serialization filter (defense-in-depth)

The v487 client's `cIcon::RenderCount` (`Icon.cpp:235`) asserts on `nCount != 0` when rendering item icons. Half-state rows in any `ItemListModel` (`ItemId > 0` but `Amount == 0` — typically from an incomplete grant flow or a manual DB row) would slip through to the wire and pop up `CsAssert` mid-render. Hardened the serialization layer:

- `ItemListModelBehavior.Count` — was `Items.Count(x => x.ItemId != 0)`; now also requires `Amount > 0`.
- `ItemListModelBehavior.GiftToArray` — same predicate tightening on the per-item filter.
- `ItemModelBehavior.ToArray(simplified)` — `if (ItemId > 0)` → `if (ItemId > 0 && Amount > 0)`. Half-state rows fall to the else-branch and serialize as the canonical empty-slot byte block.
- `ItemModelBehavior.GiftToArray` — added `Amount <= 0` early-return.

Belt-and-braces; the right long-term fix is preventing zero-amount rows from being persisted in the first place, but the serialization filter ensures no client-side render asserts even if the storage layer drops one.

### Phase 3 deferrals (intentional, not lost)

- **C8 friend recommend (sRECOMMENDE)** — v487 client has no `Send-Recommend` path (`RecommendEvent_Contents.cpp` only RECEIVES, never INITIATES). Server can parse the bin section but cannot trigger; deferred to Phase 8.
- **C10 daily-check streak (sDAILY_CHECK_EVENT)** — gated by `LJW_DAILYCHECKEVENT_191030` (which depends on `SERVER_KSW_DAILYCHECKEVENT_191014`) and `COMMON_LIB_FIXED`. Neither macro is defined anywhere in v487. The 100-day calendar bin data is still loaded for parity, but `EventContents::MakeWorldData` never sends the request packet, the client's UI never opens. Server has no work to do for v487; deferred.

## Phase 3 (partial): Buff.bin + Achieve.bin + Event.bin migration

Three more static-data bins ported from MariaDB to in-memory Pack03 bin loaders, mirroring the Phase 2 DMBase/Digimon_List/DigimonEvo work. Continues the static-data-off-DB plan documented in memory `project_bin_static_data.md`. Loader patterns mirror `DMBaseBinLoader` exactly: POCO + `*BinLoader.cs` under `Application.GameAssets/Bins/`, DI-singleton in `Game.Host/Program.cs`, eager-loaded at boot with a count-line in the startup log.

**What landed:**

- **`Buff.bin`** (302 KB, single section, 636 records → 280 surviving after `s_bDelete` tombstones dropped). Backs `BuffInfoAssetsQuery`. v487 layout decoded from `LibProj/CsFileTable/Buff.h` and `BuffMng.cpp:180-198`. All 9 server-relevant fields (`BuffId`, `Type`, `LifeType`, `TimeType`, `MinLevel`, `Class`, `SkillCode`, `DigimonSkillCode`, `ConditionLevel`) wire 1:1 to `BuffAssetDTO`. Strings (`s_szName`, `s_szComment`, `s_szEffectFile`) skipped per the bin-string-framing convention. The ~356 deleted records (sample IDs 40101..40105 — vintage tombstones kept for ID stability) are dropped at load.
- **`Achieve.bin`** (264 KB, recursive sTYPE category tree + 330 detail records). Backs both `AchievementAssetsQuery` and `AllTitleStatusAssetsQuery`. Loader walks the sTYPE root tree (UI labels — discarded), then reads 330 × 796 B records. The category/icon/group/subgroup/display fields are loaded but not exposed to a server query yet. Of the 330 achievements, 93 grant a title (non-zero `BuffCode`).
- **`Event.bin`** (54 KB, 6 sequential sections). Backs `MonthlyEventAssetsQuery`. Loader walks all 6 sections cleanly to byte 53640: §1 attendance window (CRT `tm` × 2 — v487's value is stale 2017-03-15..2017-04-26, treat as informational), §2 daily play-time events (44 records, `s_nMinute` threshold + 6 reward slots), §3 friend-recommend (12 records — v487 client has no Send-Recommend path so deferred to Phase 8), §4 monthly attendance (1 record × 28 daily reward slots — currently the only consumed section), §5 hot-time XP/drop windows (7 weekly entries), §6 daily-check streak calendars (1 group × 100 reward days). HotTime/DailyCheck wstrings parsed into `DateTime`/`TimeSpan` via standard `TryParse`. Five sections sit in memory unconsumed pending Phase 3 follow-up commits — no DTO/Model retired, just future-proofed.
- **`MonthlyEventAssetsQueryHandler`** swap: handler now flattens each `EventMonthlyRecord`'s 28-element reward array into 28 day-keyed `MonthlyEventAssetDTO` rows (matching the existing consumer shape — `MapServerTamerOperation:1010`'s `CurrentDay == AttendanceReward.TotalDays` filter still works unchanged). Empty reward slots dropped to keep the list compact.
- **`AllTitleStatusAssetsQueryHandler`** swap: emits derived rows for every achievement with `BuffCode > 0`, with `Id == AchievementId == QuestId`. The 23 stat fields (8 base stats + 12 element resistances + 3 secondary stats) inherited from `StatusDTO` are left at default zero — **implicit retirement of the per-title flat-stat-bonus path** that the v487 client never had. Title effects come exclusively through the achievement's `BuffCode → Buff.bin → Skill.bin` apply chain, not through this stat block. `DigimonModelBehavior.GetTitleStatus(StatusTypeEnum)` continues to be called from 8 stat getters, but every lookup now returns 0. Documentation block added on the function explaining the implicit retirement; method signature kept so callers compile without per-site comment noise.

**SetTitle correctness fixes (separate from the bin migration; uncovered while auditing C4b):**

- `SetTitlePacketProcessor.cs` had three pre-existing bugs that would have masqueraded as bin-migration bugs once the title-stat path was retired. Fixed in this commit:
  1. **Tamer's old title buff was never removed.** Title buffs whose `SkillCode` targets the tamer (e.g., cast-speed bonuses) were applied to `client.Tamer.BuffList` but never `ForceExpired/Remove`'d when the title changed. Now mirrors the partner-side cleanup.
  2. **Stash digimons (non-active partner) silently dropped the buff but never sent a `RemoveBuffPacket`** — leaving stale icons in the client's digimon-list UI. Self-only `Send` added (the broadcast is unnecessary; only the tamer is a viewer of stash digimons).
  3. **Tamer never received the new title's buff.** Original code added the buff only to all owned digimons. Title buffs gated by `DigimonSkillCode == 0 && SkillCode > 0` now also land on `client.Tamer.BuffList` with the standard `AddBuffPacket` broadcast.

**Buff overlap rule investigated and intentionally not implemented server-side.** The audit's first pass suggested adding a `BuffClass`/`MinLevel` overlap-reject guard to `BuffListModelBehavior.Add`. Going back to the v487 client at `DataMng.cpp:3895-3897` showed two reasons not to: (1) the rule's comparison is `existing.MinLv <= incoming.MinLv → reject incoming` (the audit had it flipped); (2) the rule is gated by `BuffType == 3` and is a *client-side UI pre-check during item-use*, not a general buff-add rule. Putting the guard in `BuffListModel.Add` would have rejected legitimate skill-cast / title-equip / evolution buff flows. The proper home for server-side enforcement is `ItemConsumePacketProcessor` for system buffs only — captured as a comment block in both `Add` methods for the future fix.

**Phase 3 remaining work (deferred, not lost):** §1 attendance window gate (config-driven; bin window is stale), §2 daily play-time event service + server→client panel packet (~4 h, new feature), §5 hot-time XP/drop multiplier service + server→client panel packet (~4 h, new feature), §6 daily-check streak service + server→client panel packet (~4 h, new feature). §3 friend-recommend deferred to Phase 8 because v487 client has no client→server Send-Recommend path.

## Tamer per-model "base status" baseline retired

DSO emulator carried a per-tamer-model baseline (`Asset_TamerBaseStatusAsset`, 12 rows) that was added on top of the per-level stats. Audit of v487 client showed it has no equivalent — the client computes tamer stats purely from `BaseMng.GetTamerBase(level, tamerType)` (DMBase.bin §1 in our setup), with equipment/socket/buff modifiers added at runtime. The per-model baseline was emulator-only.

Closer look at the consumer side made it clear the baseline barely did anything anyway: only `_baseMs` actually summed `BaseStatus.MSValue + LevelingStatus.MSValue`; HP/DS/AT/DE were all read from `LevelingStatus` exclusively. Retiring it brings the server in line with v487's stat math without losing any field that was meaningfully driving in-game numbers.

**All retirement edits are commented out, not deleted** — easy revert if a future feature wants the per-model baseline back. Files left in place: `CharacterBaseStatusAssetDTO.cs`, `CharacterBaseStatusAssetModel.cs`, `CharacterBaseStatusAssetConfiguration.cs`, plus the `TamerBaseStatusAsset*Query*` files in both Application projects (orphaned but compilable). The MariaDB `Asset_TamerBaseStatusAsset` table is left in place too — not migrated away, just no longer queried.

- `CharacterModelBehavior.cs:26` — `_baseMs` is now `LevelingStatus.MSValue` only. Net effect: naked-stats MS at level X is whatever DMBase.bin §1 ships for that tamerType+level. Equipment/Socket/Buff layers still accumulate via the `MS` getter as before.
- `CharacterModel.cs:155` — `BaseStatus` property commented.
- `CharacterModelBehavior.cs:1708` — `SetBaseStatus(...)` setter commented.
- `Game.Host/Managers/StatusManager.cs:18-21` — `GetTamerBaseStatus(model)` commented.
- `Game.Host/PacketProcessors/InitialInformationPacketProcessor.cs:146-150` — `character.SetBaseStatus(...)` call commented.
- `Character.Host/CharacterPacketProcessor.cs:168-172` — `character.SetBaseStatus(...)` call (character-creation path) commented.
- `Application.GameAssets/AssetsLoader.cs:25, 84` — `TamerBaseInfo` property + boot-time load commented.
- `Application.GameAssets/Mapping/GameAssetsProfile.cs:20` and `Application.CharacterAssets/Mapping/CharacterAssetsProfile.cs:20` — `Model→DTO` AutoMapper profile lines commented.
- `IServerQueriesRepository.cs:86, 114` — interface methods `GetTamerBaseStatusAsync` + `GetAllTamerBaseStatusAsync` commented.
- `Infraestructure/Repositories/Server/ServerQueriesRepository.cs:94, 438` — implementations commented.
- `Infraestructure/DatabaseContext/DatabaseContext.Asset.cs:19, 56` — `DbSet<CharacterBaseStatusAssetDTO>` and the `ApplyConfiguration(...)` call commented.
- `Application.GameAssets/Queries/TamerBaseStatusAssetsQueryHandler.cs` — handler now returns an empty list instead of calling the deleted repo method (it was the only orphaned handler that broke compile after the interface removal).

Game.Host, Character.Host, Account.Host all build clean (0 errors); 3-host smoke boot verified. No runtime errors observed during login/character-load on the test session — no `TamerBaseInfo` accesses, no null-derefs.

## TamerList.bin not migrated this round

Audit (see `Tamer.h` + `TamerMng.cpp::SaveBin`) found the bin holds 2 sections totaling 19844 B: 12 × 1500 B `CsTamer::sINFO` + 9 × 204 B `CsEmotion::sINFO`. After stripping strings (UI-only display fields: name, sound dir, comment, part name, gender path, emote command aliases), the only currently-server-relevant fields are `s_dwTamerID`, `s_nTamerType`, `s_Skill[5]` in §1 — and only `s_Skill[5]` actually drives a feature, which doesn't activate until Skill.bin lands (Phase 6).

The two queries the bin map plan pointed at TamerList.bin (`TamerBaseStatusAssetsQuery` + `TamerSkillAssetsQuery`) don't fit:

- `TamerBaseStatusAssetsQuery` returns per-model baseline stats not present anywhere in v487 bins. **Retired** — see section above.
- `TamerSkillAssetsQuery` returns `(SkillId, SkillCode, Duration)` which lives in Skill.bin, not TamerList.bin. **Deferred to Phase 6.**

`s_nTamerType` could be loaded as the authoritative key for `TamerLevelingAssetsQueryHandler` to drop the `tamerType == modelID − 80000` assumption, but that assumption is the conventional v487 convention (model 80001 = type 1, …, model 80012 = type 12) and worth verifying with a one-off bin probe before adding loader plumbing for one byte. Captured in memory plan; will revisit when Skill.bin lands and `s_Skill[5]` becomes actionable.

Phase 2 in the bin migration plan effectively reduces to "retire the per-model baseline" alone. Phase 3 (Buff.bin + Achieve.bin + Event.bin) is the next real bin-loading work.

## Mob packets: v487 wire format alignment + per-tamer batching

Mobs were not rendering in-game. Three layered bugs in the LoadMobsPacket / sync pipeline plus a client-side dispatch truncation. Net effect after the fixes: mobs spawn, walk, run, take damage, die, drop loot.

- **`LoadMobsPacket` per-mob payload** — rewritten to match v487's `SyncInMonster` exactly. Per mob: `nSync::Pos pos (8B)`, `cType (8B = handler u4 + type u4)`, `DstPos (8B)`, `u1 nHpRate`, **`u1 nLevel`** (was `Short` — overran the next field), **`u4 nMonSkill_Idx`** (was `Short` — undersized), `int nStack`, `u4 nCondition`, **`u4 nCnt = 0`** (was missing — without this seed the client's "is this a known mob?" map check fell through). Added a shared `WriteMobEntry` helper to keep all three ctors in lockstep.
- **`MobWalkPacket` / `MobRunPacket` / `UnloadMobsPacket` / `DestroyMobsPacket`** — fixed per-packet wire-format bugs found by tracing the v487 client `Recv*` dispatcher: `cType` written as `WriteUInt(8B)` everywhere (the client's `GetClass(u2)` read truncates `nClass ≥ 4` otherwise; companion patch in client `cCliGameSync.cpp` switches the four `SyncOutObject/SyncDelete/SyncMove/SyncWalk` dispatchers to `type.m_nClass` instead of `GetClass(nUID)`). `DestroyMobsPacket` got the missing trailing `WriteInt(0)` `cSyncType` terminator.
- **Per-tamer batching in `MapServerMonsterOperation`** — newly-visible mobs are now collected per tamer per cycle (`tamersToNotify` dict) and flushed as one `LoadMobsPacket` per tamer instead of one packet per mob. Cuts a packet-flood at zone entry that was overwhelming the client's per-frame load.

## Skill packets: v487 RecvSkill is gutted, follow-up packet carries everything

Tracing the v487 client showed `pGame::Skill (1015)`'s `RecvSkill` is an `assert_cs(false)` stub; the actual cast + damage flow is atomic on the follow-up packet (`ApplyAround` / `RangeSkillDmg` / `SkillHit`). Server was sending both a `CastSkillPacket(1015)` and the damage packet, which on v487 surfaced as a soft assert spam plus a malformed second packet because of an underrun in `SkillHitPacket`.

- **Removed all 8 `CastSkillPacket` broadcasts** in `PartnerSkillPacketProcessor.cs` (4 dungeon + 4 map server, two indent patterns each). Replaced with a comment explaining v487 RecvSkill is dead. Followed by `CastSkillPacket.cs` file deletion since it's now unreferenced.
- **`SkillHitPacket` `nBattleOption`** — was `WriteByte(0)` (1B); v487 reads `WriteInt(0)` (4B). Underrun was being absorbed as part of the next field, breaking the entire follow-up packet for any skill that landed on multiple targets.
- **`MonsterSkillDamagePacket.cs` deleted** — used dead packet ID `16011` (`Qinglongmon RaidChainSkill`) that no v487 client cares about. `GameMapMobBehavior.cs` switched the 2 broadcast sites to a per-target `SkillHitPacket` loop (renamed loop var to `hitTarget` to avoid shadowing the outer `target`).

## DigimonEvo.bin: `SEvolutionInfo[9]` is the per-form outgoing-evolution table

Re-audit of `DigimonEvo.bin`. The previous audit marked offset 8..80 (`SEvolutionInfo[9]`, 9 × 8 bytes) as "redundant — each branch target appears as its own evolveObj." That was wrong: it IS the table the client's QuickEvol UI iterates by position. The client populates each evolution icon from `m_nEvolutionList[i]`, and on click sends `SendEvolution(uid, i)` — the server's `evoStage` from that packet is the index into this same per-form list.

Symptom: clicking any digivolve slot was a silent no-op. `PartnerEvolutionPacketProcessor`'s very first guard `if (evoLine == null || !evoLine.Any())` was firing on every request because the bin handler never populated `EvolutionLineAssetDTO.Stages`. Server sent `DigimonEvolutionFailPacket` and bailed without logging.

- `DigimonEvoBinLoader.cs` now parses the 9 × 8-byte `SEvolutionInfo` array (each entry: `int nSlot`, `int dwDigimonID`) and exposes them as a `IReadOnlyList<DigimonEvoStage>` on `DigimonEvoLine`.
- `DigimonEvolutionAssetsQueryHandler.cs` populates `EvolutionLineAssetDTO.Stages` from those per-form entries (`Type = TargetType`, `Value = Slot`). Empty slots (`INVAIDE` sentinel) are preserved so the index from the client lines up 1:1.
- Scoped to `Application.GameAssets` only — the `Application.CharacterAssets` side is the character-creation path and doesn't need `Stages`.

## Leveling: bin EXP is wire-format units (×100), server compares against real units

Partner at 5xxx% should-have-leveled but didn't. The v487 client reads tamer/digimon EXP threshold via `s_dwExp * 0.01f` (`FmTamer.cpp:169`), so a bin row of `13500` displays as `135%` on screen. The server stores `tamer.CurrentExperience` in real units, and the OLD pre-bin path compared against `Asset_CharacterLevelStatus.ExpValue` which was already-divided. Without the same divide on the bin path, server compared e.g. `8010 < 13500` and never leveled up while the UI showed 5xxx%.

- `TamerLevelingAssetsQueryHandler.cs` and `DigimonLevelingAssetsQueryHandler.cs` now divide bin `Exp` by 100 to match the real-units convention used by the level-up comparator.

## Account legacy backfill at login

Legacy accounts created before `AccountModel.Create` standardized the four account-level item lists (`AccountWarehouse`, `CashWarehouse`, `ShopWarehouse`, `BuyHistory`) had `null` rows for those slots. `ComplementarInformationPacketProcessor`'s `LoadInventoryPacket` call hit a null-deref and the handler crashed silently — no log, no client message — leaving the player stuck in `Connected` state at the post-character-select loading screen.

- New `EnsureAccountItemListAsync(accountId, type)` on `IAccountCommandsRepository` + `AccountCommandsRepository`. Idempotent: no-op if a row of that type already exists. Sizes default to the bin-driven values (`ItemListModel.BinDrivenDefaults`) when present, falling back to the matching `GeneralSizeEnum` constant.
- New `CreateAccountItemListCommand` + handler under `Application/Separar/Commands/Create/`. Sent during login to backfill any missing list before the in-memory `account.ItemList.ForEach(character.AddItemList)` runs.
- `InitialInformationPacketProcessor.cs` now calls the command for each of the four list types before mapping into the character.

## Game host: async exception observability

`GamePacketProcessor.ProcessPacketAsync` is invoked fire-and-forget from `GameServer.OnDataReceivedEvent` (no `await`), so any async exception thrown inside a packet handler ended up on the unobserved-task scheduler and disappeared. This was masking real handler crashes (the EnsureList null-deref above was found only after adding the catch).

- Added a try/catch around the `processor.Process(client, data)` call site logging `Handler ({Type}) threw for tamer {TamerId}: {Msg}`. Surface area only — no behavior change for the success path.

## Game.Host: DMBase.bin fully wired (all 11 sections)

Phase-2 of the bin migration. `DMBase.bin` now backs every server feature it describes, with each unwired field documented as N/A only after the v487 client was grepped to verify no consumer exists. New `DMBaseBinLoader` parses all 11 sections at boot and replaces the matching DB-backed query handlers.

**Sections wired to existing features:**

- §1, §2 — `TamerLevelingAssetsQuery`, `DigimonLevelingAssetsQuery` already migrated; this commit also fixes a latent bug from the original migration: the bin's leveling-rank key for §2 is `s_nDigimonType` (offset 392, byte, values 1..4), not `s_dwCharSize` (offset 394, ushort, visual-scale percentages). The previous loader read the wrong offset, so `digimon.BaseInfo.ScaleType` carried percentages and `ExpManager`'s lookup `DigimonLevelInfo.Where(x => x.ScaleType == digimon.BaseInfo.ScaleType)` returned 0 rows for every digimon — silent breakage of digimon EXP/level-up. Loader now reads offset 392; handler also sets `DigimonLevelStatusAssetDTO.StatusId = rec.Id` (was unset, defaulted to 0; `StatusManager.GetDigimonBaseStatus` queries by `StatusId` and `.Single()` was throwing inside `InitialInformationPacketProcessor`, hanging the loading screen after character select).
- §3 — `MapInfo.ShoutSec` drives a per-map shout cooldown in `ShoutMessagePacketProcessor` (`ConcurrentDictionary<long, DateTime>` per-tamer state). `EnableCheckMacro` is a client-side CAPTCHA flag (`MacroProtectContents.cpp:102`) — bin loaded for parity, no server feature.
- §4 — `JumpBoosterPacketProcessor` enforces the bin allowlist of `(item → destinationMap)` pairs; refuses with log + system message on mismatch.
- §6 — `MaxGuildPerson` cap enforced on member-add (`GuildInviteAcceptPacketProcessor`); new `GuildLevelService` implements server-driven guild auto-leveling (validates `Fame`/`NeedPerson`/`MasterLevel` against next-level requirements, bumps level, persists via new `UpdateGuildLevelCommand` + `IServerCommandsRepository.UpdateGuildLevelAsync`). `GuildModelBehavior.AddExperience` + `LevelUp` added. Hooked from `QuestDeliverPacketProcessor` (+1 fame per quest delivery — minimal feeder; tune as desired).
- §7 — `MaxTacticsHouse` (DigimonArchive cap) and `MaxWareHouse` checked in the relevant `ItemConsumePacketProcessor` slot-expand paths. `MaxShareStash` drives `AccountWarehouse` initial size via a new `ItemListModel.BinDrivenDefaults` static dictionary, set once at boot in `Program.cs`. `UnionStore`/`ConsumeXG`/`ChargeXG` documented N/A — no v487 client consumer (verified by grep of `~/vm/dmoclient-main`).
- §8 — `PersonStore.PersonCharge` applies a 2% commission on consigned-shop sales (`ConsignedShopPurchaseItemPacketProcessor`). `PersonStore.StoreDist` enforces a server-side proximity gate when a player tries to open a personal shop (`TamerShopOpenPacketProcessor`) — refuses if another shop tamer is within `StoreDist`, mirroring the client check at `DataMng.cpp:3696` but without trusting a manipulated client.
- §9 — `PlayPenalty` (Korean-region playtime fatigue) implemented as `FatigueService` (`Managers/FatigueService.cs`). New `Fatigue:Enabled` config key (default off — see `appsettings.Development.Example.json`). `FATIGUE_HOOK` marker comment is searchable across the codebase (~70 occurrences, 23 sites). Wired through every reward call-site: `ExpManager.ReceiveTamerExperience` and `ReceiveDigimonExperience` accept a `decimal fatigueMultiplier = 1m`; all 17 invocations across `MapServer*Operation`, `EventServer*Operation`, `DungeonsServer*Operation`, `QuestDeliverPacketProcessor`, and `ItemConsumePacketProcessor` pass `_fatigueService.GetMultipliers(client).exp`. Drop-side: `BitDropReward` and `ItemDropReward` in all three monster-operation files short-circuit on `drop == 0` and scale chance otherwise. GM-issued exp grants intentionally bypass fatigue. `GameClient.SessionStart` added to track session play-time.
- §11 — `StatusApplyAssetQuery` migrated from DB to bin; bin's `EvolutionStageApply` table replaces the 17-row `Asset_StatusApply` table that was a hidden mirror.
- §12 — `DigimonEvoMaxLevel` plumbed via new `EvolutionLineAssetDTO.SkillMaxLevels byte[]` field; per-evolution-stage skill caps flow from the bin through `DigimonEvolutionAssetsQueryHandler` into `DigimonEvolutionSkillModel.MaxLevel` (new `SetMaxLevel` setter; new `DigimonEvolutionModelBehavior.SetSkillMaxLevels`; both `AddEvolutions` overloads call it).
- §13 — D-skill expansion items (Type 202, "Skill DigiCode") flow through a dedicated packet `pDigimon::DigimonSkillLimitOpen` (3245), NOT generic ItemConsume — verified in `cCliGameSkill.cpp:3038` (gated by `SDM_DIGIMONSKILL_LV_EXPEND_20181206`, defined in v487). New `DigimonSkillLimitOpenPacketProcessor`: validates inventory item, looks up bin §13 entry by `Section`, validates partner's evolution stage against `AllowedEvoTypes`, raises every skill slot's `MaxLevel` by `5 × ExpansionRank` (rank 1/2/3 → +5/+10/+15; the bin doesn't carry the delta, this is a Korean-MMO convention), persists via `UpdateEvolutionCommand`, decrements the item, and replies with the cEvoUnit shape (new `DigimonSkillLimitOpenResultPacket` writes the 19-byte `cEvoUnit` struct: `bitfield#1: SkillExp(26) | SkillExpLevel(6)`, `bitfield#2: SlotState(4) | MaxSkillLevelStep(8) | Reserved(20)`, `SkillPoint(1)`, `SkillLevel[5]`, `SkillMaxLevel[5]`). The pre-existing ItemConsume `Type==202` branch is kept as exploit-protection — refuses + logs since the legitimate path is the dedicated packet.

**Sections documented N/A in v487** (loaded for parity; no server feature, no client consumer): §3 `EnableCheckMacro`, §5 `PartyDist`, §6 `IncMember`/`ItemNo*`/`MaxGuild2Master`, §7 `UnionStore`/`ConsumeXG`/`ChargeXG`, §8 `EmploymentCharge`/`Objects`. Each was verified by client-source grep before being marked N/A — never inferred from field name alone.

## DigimonEvo.bin: missing fields wired + server-side gates

Audit caught 4 latent bugs from the original Phase-1 migration where bin fields were parsed by the loader but never propagated into the DTO; consumers saw default zeros and silently misbehaved.

- **`SlotLevel ← line.EvoSlot`** — `QuestDeliverPacketProcessor` indexes `Tamer.Partner.Evolutions[evolutionQuest.SlotLevel - 1]`. With `SlotLevel = 0` (default) the access becomes `Evolutions[-1]` and throws. Property's `private set` was opening only via AutoMapper from the DB path; changed to `public set` on `EvolutionLineAssetDTO` and the bin handlers populate it.
- **`UnlockItemSection ← line.UseItem` + `UnlockItemSectionAmount ← line.UseItemNum`** — `EvolutionUnlockPacketProcessor` reads these to decide which inventory items to consume for the unlock. With both 0 the item-based unlock path was effectively broken; only the quest-based path (`UnlockQuestId == questId && UnlockItemSection == 0`) worked.

Two new server-side gates added (real exploits prevented):

- **`m_nEnableSlot == 0` refused** — bin offset 88. Client `DigimonUser.cpp:2355,:2821` skips closed slots entirely; without server enforcement, a crafted packet could unlock them. v487 bin has 14 closed lines. `EvolutionUnlockPacketProcessor` now refuses + logs.
- **`m_nOpenQualification == 3 (XAI_SYSTEM)` refused** — bin offset 90. NEED_QUALITICATION enum from `LibProj/CsFileTable/DigimonEvolveObj.h:8`: `0=NONE, 1=PARTNERMON, 2=ROYAL_KNIGHT, 3=XAI_SYSTEM`. The Xai system requires per-tamer eligibility state that isn't tracked yet. v487 bin has 47 lines requiring Xai. Refused pending Xai-state plumbing.

Both `DigimonEvo.cs` POCO and `DigimonEvoBinLoader.cs` updated in both `Application.GameAssets` and `Application.CharacterAssets`.

## Memory of decisions

- **Verify in client source first** — the audit caught two cases where a field name suggested one meaning but the v487 consumer used a different one (`s_nDigimonType` vs `s_dwCharSize`; `sPLAY_PANELTY` is fatigue not death-penalty). Methodology: grep `~/vm/dmoclient-main/{DProject,common_vs2019,LibProj}` for `Get<Field>()` / `m_<field>` / `s_<field>` / namespace-qualified usage. If 0 references after #ifdef checks, document N/A. Never guess from name.
- **Defaults stay safe** — fatigue is off, guild auto-level grants tiny amounts (1 fame/quest), D-skill cap delta uses a well-defined convention rather than a guess. All toggles ship "off" in the example config.

## Initial info packet (`InitialInfoPacket`)

Wire format alignment with v487 client (`Domain/.../Packets/GameServer/InitialInfoPacket.cs`):

- **Channel field** — switched from `WriteInt` (4 bytes) to `WriteShort` (2 bytes) to match the client's `n2` channel slot. Reading 4 bytes from a 2-byte field on the client surfaced as a junk channel index in the channel-select dropdown (e.g. `8332xxxx`).
- **`shortBeforeSlot`** — added a 2-byte zero in **both** the `if (party != null)` and `else` branches before the slot block under v487 layout, so downstream offsets line up regardless of party state.
- **`CloneLevel`** — kept as `WriteShort` (2 bytes) to match the client struct.

## Item / inventory item models

- `ItemModelBehavior.cs`, `ItemListModelBehavior.cs` — items kept at the v487 fixed 68-byte size (no growth from later regions' fields).

## Digimon evolution model

- `DigimonEvolutionModelBehavior.cs` — fixed-pad each evolution row to 19 bytes so the client can index by stride.

## Character model regions

- `CharacterModelBehavior.cs` — `MapRegions` initial capacity bumped 192 → 200; `SerializeMapRegion` padding adjusted to match v487 expected stride.

## Diagnostic strip

- `Game.Host/PacketProcessors/InitialInformationPacketProcessor.cs` — removed a session-only diagnostic that wrote `InitialInfoPacket` bytes to a hardcoded `/tmp` path. Replaced with the standard `client.Send(new InitialInfoPacket(...))` call.

## Local-bind defaults

- All host `appsettings.Development.json` files defaulted `GameServer:PublicAddress` (and similar) to `127.0.0.1` for out-of-the-box local play. Production / staging IPs go in deployment-specific overrides only.
- ASP.NET hosts (`Account.Api`, `Admin`): `launchSettings.json` `applicationUrl` values bind to `127.0.0.1` instead of a hardcoded public IP.

## Repository hygiene

- Real `appsettings.Development.json` files are git-ignored. Each host ships an `appsettings.Development.Example.json` with placeholder credentials; copy to the real name before running.
- Removed an out-of-tree `Tools/Discord` folder (was already removed from the .sln) and a 71 MB packed asset file under `Tools/NPCEditor/Util/`.
- Added `db/dso.sql` — schema + seed dump used to bring up a fresh database matching what the hosts expect.

## Application split — Character vs Game asset paths

`DigitalWorldOnline.Application` was split into three projects so per-host asset-loading code can be swapped independently in a later effort (e.g. moving from DB-backed asset queries to bin/pack-file readers, per host). No runtime behavior change — purely structural.

- **`DigitalWorldOnline.Application`** — slimmed; keeps shared queries/commands (Account, Server, Config, Character core, Routine). 183 handlers.
- **`DigitalWorldOnline.Application.CharacterAssets`** (new) — referenced only by `Character.Host`. Contains the 4 character-creation queries: `TamerBaseStatusQuery`, `TamerLevelStatusQuery`, `DigimonBaseInfoQuery`, `DigimonEvolutionAssetsByTypeQuery`. Plus its own `CharacterAssetsProfile` (AutoMapper) and a `CharacterAssetsMarker` type used as the `AddMediatR(typeof(...).Assembly)` handle.
- **`DigitalWorldOnline.Application.GameAssets`** (new) — referenced by `Game.Host`, `Routine.Host`, and `Account.Api`. Contains `AssetsLoader` + `ConfigsLoader` + the 40 game-side queries they fire (33 `AssetsLoader` queries + 4 game extras + 3 config queries). Plus `GameAssetsProfile` and `GameAssetsMarker`.

Host wiring:
- Each host's `services.AddMediatR(...)` switched from a single-assembly scan to an explicit list of the assemblies the host references.
- Each host's `services.AddAutoMapper(typeof(AssetsProfile))` updated to the appropriate per-project profile (or removed for hosts that don't need any asset map — `Account.Host`).
- `Application.Admin/Queries/Get{Item,Mob,Raid}AssetQuery` stayed in `Application` (admin-CRUD on assets is a different pattern and shares `IAdminQueriesRepository` with 24 other admin queries — splitting them off would fragment that pattern).

Seam is enforced at the project graph: `Character.Host`'s csproj has no `ProjectReference` to `Application.GameAssets` (and vice-versa), so neither host can reach into the other's asset code at compile time. `error CS0234: namespace 'GameAssets' does not exist in the namespace 'DigitalWorldOnline.Application'` confirms.

All 6 runtime hosts plus `Account.Api` and `Admin` build clean (`dotnet build -c Debug`); sln-level build still fails only on the 3 documented Windows-only tool projects (`NPCEditor`, `Helper`, `BinXmlConverteer`).

## Character.Host: static data from Pack03 bins instead of DB

First slice of a broader migration to source static game data from the v487 client's Pack03 bin files rather than dedicated `Asset_*` DB tables. DB stays for dynamic per-account state (account rows, character/digimon writes, name uniqueness, etc.). Only Character.Host is touched in this commit; Game.Host follows in a later patch.

The four MediatR query handlers Character.Host consults during character creation are now backed by in-memory loaders parsing files under `Bins/data/bin/english/`:

- **`TamerBaseStatusQuery`** → `DMBase.bin` section 1, key `(model − 80000)·1000 + 1`. Loader filters to `level == 1` only (12 rows out of 1440), since Character.Host never asks for higher levels — that's level-up math, which is Game.Host's job.
- **`TamerLevelStatusQuery`** → same `DMBase.bin` section 1; throws if asked for a level > 1, since the loader pre-filtered them out.
- **`DigimonBaseInfoQuery`** → `Digimon_List.bin` (single-section in v487; 634 records of 572 bytes). Filtered at load time to the 4 selectable starter types — Agumon (31001), Lalamon (31002), Gaomon (31003), Falcomon (31004) — so 630 unused entries never enter memory.
- **`DigimonEvolutionAssetsByTypeQuery`** → `DigimonEvo.bin`, same starter filter. Each digimon's evolution forms are flattened into `Lines[]` sorted by `m_nEvoSlot` so consumers see Rookie → Champion → Ultimate → Mega → alts → Burst order. The server's `DigimonModel.AddEvolutions` auto-unlocks the first two — that ordering matters and is now explicit in the loader.

A separate validation gate sits on top of `CreateCharacter` packets: the handler refuses any tamer/digimon model that isn't `bEnable=true` in `CharCreateTable.bin`. In v487 that's 4-of-12 tamers (Marcus/Touma/Yoshi/Ikuto) and 4-of-86 digimon — the same set the client UI surfaces. Mismatches are logged with the offending account ID + values; nothing reaches `CharacterModel.Create` or the DB. Earlier code blindly trusted whatever the client sent.

Notes:

- **Field mapping gotcha**: the client struct (`CsDigimon::sINFO`) names the per-digimon Element field `s_eBaseNatureType` — the original DMO term. The server DTO renamed it to `Element` (`DigimonElementEnum`). Loader maps directly: bin offset 352 → DTO `Element`. Verified by DB cross-check.
- **Stat coverage**: bin's per-level stat record (`CsBase::sINFO`) carries 8 fields (HP/DS/MS/DE/EV/CT/AT/HT). The DTO's other 4 (AS/AR/BL/WS) are not in DMBase's per-level table — AS/AR live in `Digimon_List.bin` as base stats; tamer-side AS/AR/BL/WS aren't per-level scaling inputs in v487 and stay 0.
- **`std::string` serialization**: `CharCreateTable.bin` uses variable-length records — `[int sizeBytes][sizeBytes raw chars]` for the voice-file field. The original parser-by-fixed-stride happened to land on the right byte boundaries because voice strings are empty in v487, but the field labels were wrong (e.g. what looked like a `Selectable` byte was actually `bShow`; the real selectability flag is `bEnable`).

Working set after filtering: ~150 records in memory across all four bins (12 tamer rows + 4 digimon entries + 4 evolution trees with 33 lines + 12+86 entries of CharCreateTable for the validation gate). Down from ~2,500 raw records.

Tested end-to-end against a live v487 client. Created character lands in `Character_Tamer` (model 80001), digimon lands in `Digimon_Digimon` (model 31001, HatchGrade=Perfect), and `Digimon_Evolution` gets the 9-row Agumon tree in EvoSlot order with the first two unlocked — verbatim from the bin.

Bin files themselves are not bundled with the repo (publisher copyright). See README for the expected `Bins/` layout. Same commit untracks `src/Tools/DataImporter/XMLs/` — replaced by the bin pipeline.
