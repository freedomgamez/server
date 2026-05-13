# Phase 1 Audit — `ItemList.bin` + `Npc.bin`

Date: 2026-05-13

## Client-parity constraints
- Parse order must mirror client `SaveBin/_LoadBin` exactly.
- Default MSVC alignment/padding rules apply (`pack(8)`), no `#pragma pack` override in item/npc headers.
- `TCHAR` width and locale-macro paths are layout-sensitive (`ITEM_*`, `NPC_NAME_LEN`).
- Some `ItemList.bin` tail sections are variable-length (`CmBinSave::Save_Value` style), not raw fixed structs.

## `ItemList.bin` section order (from client `ItemMng.cpp`)
1. `CsItem::sINFO[]`
2. `CsItemTap::sINFO[]`
3. `CsItemCoolTime::sINFO[]`
4. `MapDisp` (`DWORD type`, `DWORD dispId`)
5. `MapTypeName` (`DWORD type`, `sTYPENAME`)
6. `Rank` (`int itemNo`, `CsItemRank::sRANK`)
7. `ElementItem1` (`CsElementItem::sINFO`)
8. `ElementItem2` (`CsElementItem::sINFO`)
9. `Exchange` (`int idx`, `CsItemExchange::sINFO`)
10. `AccessoryOption` (`int idx`, `CsAccessory_Option::sINFO`)
11. `AccessoryEnchant` (`int idx`, `CsAccessory_Enchant::sINFO`)
12. `ItemProductionTable` (variable)
13. `AssistItemGroupTable` (variable)
14. `ItemXAISystemTable` (variable)
15. `RankEffect` (variable)
16. `DpartsItem` (variable)

## `Npc.bin` section order (from client `NpcMng.cpp`)
1. Npc base records (`CsNpc::sINFO`) + type-specific payload + extra-data payload
2. `ModelInfo` section
3. `NpcEvent` section

## High-priority server consumers to cut over first
- `Asset_ItemInfo`:
  - `Application/.../Queries/ItemAssetsQueryHandler.cs`
  - `Application/.../GameAssets/AssetsLoader.cs`
  - runtime packet consumers (`ItemConsume`, `ItemMove`, shop/economy, hatch/seal/evo, reward flows)
- `Asset_Npc*`:
  - `Application/.../Queries/NpcAssetsQueryHandler.cs`
  - `Application/.../Queries/NpcColiseumAssetsQueryHandler.cs`
  - runtime consumers (`NpcPurchase`, `WarpGate`, `WarpGateDungeon`, coliseum/dungeon monster ops)

## Phase-1 cutover order
1. Implement `ItemList.bin` loader + section validation logging.
2. Switch `ItemAssetsQuery` to bin source (initially read-only paths first).
3. Migrate transactional item flows after parity verification.
4. Implement `Npc.bin` loader + type payload validation.
5. Switch NPC query handlers and validate talk/shop/portal/warehouse behavior parity.

