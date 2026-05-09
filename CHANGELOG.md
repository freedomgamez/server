# CHANGELOG

Notable patches applied during the v487-client compatibility work. Grouped by area, not strictly chronological.

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
