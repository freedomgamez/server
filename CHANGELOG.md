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
