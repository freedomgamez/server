# CHANGELOG

Notable patches applied during the v487-client compatibility work. Grouped by area, not strictly chronological.

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
