# Server/Client Gap Audit Notes (2026-05-13)

Investigation-only notes collected from agent audits. No fixes applied in this document.

## 1) Persistence & Transition Risks

- Non-awaited persistence calls in multiple map/dungeon runtime flows can drop last item state on crash/transition.
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/MapServers/MapServerTamerOperation.cs:259`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/DungeonsServer/DungeonsServerTamerOperation.cs:254`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/MapServers/MapServerMonsterOperation.cs:1049`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/DungeonsServer/DungeonsServerMonsterOperation.cs:943`
- Disconnect path has fire-and-forget async calls and no strict persistence barrier before remove.
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/GameServer.cs:113`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/GameServer.cs:126`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/GameServer.cs:141`
- `RemoveClient` paths are detach-only and do not persist state themselves.
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/MapServers/MapServerBaseOperation.cs:209`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/DungeonsServer/DungeonsServerBaseOperation.cs:362`
- Transition processors with potential persistence/order windows:
  - `WarpGate`, `WarpGateDungeon`, `JumpBooster`, `SwitchChannel`, `TamerSummonSucess`, `PartyMemberLeave`, `PartyMemberKick`, `DieConfirm`, `ItemConsume` map-transfer path.

## 2) Map/Channel Parity Gaps

- Region unlock bitfield semantics mismatch vs client expectation (`0x80` map-open + lower bits region-open).
  - `src/Source/Domain/DigitalWorldOnline.Commons/Models/Character/CharacterMapRegionModelBehavior.cs:8`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/RegionUnlockPacketProcessor.cs:35`
- Channel reject flow likely maps to “full” instead of true fail state client expects.
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/SwitchChannelPacketProcessor.cs:153`
  - `src/Source/Domain/DigitalWorldOnline.Commons/Packets/GameServer/AvailableChannelsPacket.cs:25`
  - `src/Source/Domain/DigitalWorldOnline.Commons/Utils/Utils.cs:289`
- Portal null-check order bug risk (`portal` dereference before null check):
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/WarpGatePacketProcessor.cs:63`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/WarpGateDungeonPacketProcessor.cs:66`
- Portal restriction indexing parity risk (`PortalIndex` handling):
  - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/PortalAssetsQueryHandler.cs:31`

## 3) Item System Parity Gaps (ItemList.bin-driven behavior)

- `ItemList.bin` loader currently drops enforcement-relevant fields (`UseMode`, `UseTimeGroup`, `UseCharacter`, `QuestRequire`, `UseBattle`, etc.), limiting server-side rule parity.
  - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Bins/ItemListBinLoader.cs:98`
- No generic client-equivalent item-use gate before per-type consume logic.
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/ItemConsumePacketProcessor.cs:83`
- Move/split/equip paths weak on destination/bound/level/use-mode enforcement parity.
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/ItemMovePacketProcessor.cs:226`
  - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/ItemSplitPacketProcessor.cs:47`
- Bound-state/runtime parity gap: static `BoundType` exists but mutable instance-limited state parity with client is incomplete.
  - `src/Source/Domain/DigitalWorldOnline.Commons/Models/Base/ItemModel.cs:16`
  - `src/Source/Domain/DigitalWorldOnline.Commons/Models/Base/ItemModelBehavior.cs:224`
- Socket/identify parity gaps:
  - identify handler applies first option and exits early
    - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/ItemSocketIdentifyPacketProcessor.cs:65`
  - socket-in ignores requested socket order from packet
    - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/ItemSocketInPacketProcessor.cs:48`
  - socket-out does not use destination slot and may clear state before reinsert success
    - `src/Source/Distribution/DigitalWorldOnline.Game.Host/PacketProcessors/ItemSocketOutPacketProcessor.cs:45`
- Equipped effect level-gating parity risk:
  - `src/Source/Domain/DigitalWorldOnline.Commons/Models/Character/CharacterModelBehavior.cs:957`

## 4) Bin Loader Coverage / Structural Risk

- Highest loader parity risk flagged in audits:
  - `MapBinLoader` (variable/fixed mix, minimal assertions, unused/underused sections)
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Bins/MapBinLoader.cs:119`
  - `ItemListBinLoader` (large record + skipped sections still possibly gameplay-relevant)
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Bins/ItemListBinLoader.cs:50`
- Additional medium risk:
  - `DigimonEvoBinLoader` trailing battle-evo block not fully consumed/validated.
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Bins/DigimonEvoBinLoader.cs:78`

## 5) Bin Migration Completeness (Static Data Still DB-backed)

- Static domains likely still DB-backed while client has bin-driven canonical sources:
  - NPC assets
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/NpcAssetsQueryHandler.cs:18`
  - NPC Coliseum assets
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/NpcColiseumAssetsQueryHandler.cs:18`
  - Quest assets
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/QuestAssetsQueryHandler.cs:18`
  - Seal/master-card related assets
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/SealStatusAssetsQueryHandler.cs:18`
- Item subdomains split across bin+DB (parity risk):
  - scan detail, container, accessory roll, item craft filters
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/ScanDetailAssetQueryHandler.cs:18`
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/ContainerAssetQueryHandler.cs:18`
    - `src/Source/Application/DigitalWorldOnline.Application.GameAssets/Queries/AccessoryRollAssetsQueryHandler.cs:18`
    - `src/Source/Application/DigitalWorldOnline.Application/Separar/Queries/ItemCraftAssetsByFilterQueryHandler.cs:18`

## 6) Notes

- This file is a tracking snapshot of identified gaps only.
- Prioritization from audits: P0 = persistence/transition safety + map/channel critical semantics + item enforcement parity.
