# server

## Layout

```
src/
├── Source/
│   ├── Distribution/   ← entry-point hosts (executables, Blazor app, Web API)
│   ├── Application/    ← MediatR Commands & Queries, services, asset/config loaders
│   ├── Domain/         ← entities, DTOs, enums, packet readers/writers, repository interfaces
│   └── Infra/          ← DbContext, migrations, repository implementations, AutoMapper
└── Tools/              ← standalone utilities (NPCEditor, DataImporter, ExcelToDatabase, …)
db/                     ← schema + seed dump
ServerConfig.xml        ← gameplay config (hatch failure rates, starter map/items, …)
DigitalWorldOnline.sln
global.json             ← pins the .NET 7 SDK
```

## Hosts

Each host is its own process with its own `appsettings.Development.json` + TCP listener.

| Host | Default port | Purpose |
|---|---|---|
| `Account.Host` | 7029 | Login / authentication |
| `Character.Host` | 7050 | Character list + selection |
| `Game.Host` | 7607 | The world. Subdivided into `MapServers/`, `DungeonsServer/`, `EventsServer/`, `PvpServers/`, `Managers/`, `PacketProcessors/`. |
| `Routine.Host` | — | Quartz-scheduled background jobs (rankings, cleanup, …) |
| `Account.Api` | 5189 / 7189 | REST API consumed by launcher / web |
| `Admin` | 5000 / 5001 | Blazor Server admin panel |

## Prerequisites

- **.NET 7 SDK** (pinned in `global.json` with `rollForward: latestMajor`)
- A local database to point the hosts at — see `db/dso.sql` for the schema + seed dump

## First-time setup

1. **Clone**
   ```bash
   git clone git@github.com:freedomgamez/server.git
   cd server
   ```

2. **Restore and build**
   ```bash
   dotnet restore DigitalWorldOnline.sln
   dotnet build DigitalWorldOnline.sln -c Debug
   ```
   On Linux the full `.sln` build will fail on three Windows-only tool projects (`NPCEditor`, `Helper`, `BinXmlConverteer`). They are not part of the runtime — build the server projects individually if you want a clean build:
   ```bash
   dotnet build src/Source/Distribution/DigitalWorldOnline.Account.Host -c Debug
   dotnet build src/Source/Distribution/DigitalWorldOnline.Character.Host -c Debug
   dotnet build src/Source/Distribution/DigitalWorldOnline.Game.Host -c Debug
   dotnet build src/Source/Distribution/DigitalWorldOnline.Routine.Host/DigitalWorldOnline.Routine -c Debug
   dotnet build src/Source/Distribution/DigitalWorldOnline.Account.Api -c Debug
   dotnet build src/Source/Distribution/DigitalWorldOnline.Admin -c Debug
   ```

3. **Load the schema + seed data**
   ```bash
   # adjust client / credentials for your environment
   <db-cli> < db/dso.sql
   ```

4. **Configure each host**

   Each host folder ships an `appsettings.Development.Example.json`. Copy it to `appsettings.Development.json` and edit the values for your local setup:
   ```bash
   for d in \
     src/Source/Distribution/DigitalWorldOnline.Account.Host \
     src/Source/Distribution/DigitalWorldOnline.Character.Host \
     src/Source/Distribution/DigitalWorldOnline.Game.Host \
     src/Source/Distribution/DigitalWorldOnline.Routine.Host/DigitalWorldOnline.Routine \
     src/Source/Distribution/DigitalWorldOnline.Account.Api \
     src/Source/Distribution/DigitalWorldOnline.Admin
   do
     cp "$d/appsettings.Development.Example.json" "$d/appsettings.Development.json"
   done
   ```

   The `appsettings.Development.json` files are git-ignored. Edit each one and replace:

   - `Database:Connection` — fill in your real connection string (the placeholder is `Pwd=REPLACE_WITH_YOUR_PASSWORD`)
   - `GameServer:PublicAddress` — the address clients will be told to reconnect to (default `127.0.0.1` for local play)

5. **Populate the static-data bins**

   Static game data (item lists, digimon stats, evolution chains, character-create tables, etc.) is **not** bundled with this repo — those files come from your own Pack03 extraction of the v487 client. Populate the following layout under `Serv/SOURCE/`:

   ```
   Bins/
   └── data/
       └── bin/
           └── english/
               ├── CharCreateTable.bin
               ├── DMBase.bin
               ├── Digimon_List.bin
               ├── DigimonEvo.bin
               └── ... (49 total in v487)
   ```

   Loaders walk up from `AppContext.BaseDirectory` to find this directory at runtime, so the same layout works for `dotnet run` (project dir cwd) and for published deployments.

## Running

Each host is a separate process. Open one terminal per host:

```bash
dotnet run --project src/Source/Distribution/DigitalWorldOnline.Account.Host
dotnet run --project src/Source/Distribution/DigitalWorldOnline.Character.Host
dotnet run --project src/Source/Distribution/DigitalWorldOnline.Game.Host
dotnet run --project src/Source/Distribution/DigitalWorldOnline.Routine.Host/DigitalWorldOnline.Routine
```

For the two ASP.NET hosts, override the bind URL on Linux (the in-tree `launchSettings.json` profiles otherwise may try to bind to a non-local IP):

```bash
ASPNETCORE_URLS=http://localhost:5189 ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project src/Source/Distribution/DigitalWorldOnline.Account.Api --no-launch-profile

ASPNETCORE_URLS=http://localhost:5288 ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project src/Source/Distribution/DigitalWorldOnline.Admin --no-launch-profile
```

Recommended start order: `Account.Host` → `Character.Host` → `Game.Host` → `Routine.Host` → `Account.Api` → `Admin`.

## EF Core migrations

Migrations live in `src/Source/Infra/DigitalWorldOnline.Infraestructure/Migrations`. The `Infraestructure` project is itself the startup project for design-time work (it has `Microsoft.EntityFrameworkCore.Design` referenced):

```bash
export DSO_DB_CONNECTION="<your connection string>"

dotnet ef migrations add <Name> \
  --project src/Source/Infra/DigitalWorldOnline.Infraestructure \
  --startup-project src/Source/Infra/DigitalWorldOnline.Infraestructure

dotnet ef database update \
  --project src/Source/Infra/DigitalWorldOnline.Infraestructure \
  --startup-project src/Source/Infra/DigitalWorldOnline.Infraestructure
```

`DatabaseContext` falls back to the `DSO_DB_CONNECTION` environment variable when DI hasn't supplied options (i.e. design-time).

## Tools

Standalone utilities under `src/Tools/`. They are **not** part of the server runtime.

| Tool | Platform | Purpose |
|---|---|---|
| `DataImporter` (`DSO.DataImport.csproj`) | Cross-platform | Reads asset XMLs from `XMLs/` and inserts rows. Usage: `DSO.DataImport "<connection-string>" [xmls-base-dir]`. |
| `ExcelToDatabase` (`DSO.ExcelImporter.csproj`) | Cross-platform | Reads a TSV of skill codes and inserts rows. Usage: `DSO.ExcelImporter "<connection-string>" <tsv-file>`. |
| `NPCEditor` | Windows only (`net8.0-windows`) | NPC asset editor. |
| `Helper`, `BinXmlConverteer`, `BinXmlConverter`, `FullPackingTool`, `Hackerman/PortSecurity` | Windows only | Various asset / packaging utilities. They do not touch the database. |

Build the Windows-only tools on a Windows machine.

## Conventions

- The "Infrastructure" project is misspelled `Infraestructure` everywhere (folder, csproj, namespace). Keep the misspelling rather than renaming piecemeal.
- `.editorconfig` disables `IDE0058` (unused expression value) — don't add `_ = ...` discards just to silence the warning.
- Logging is Serilog with rolling-file sinks under `logs/<Component>/<Level>` per host.
- The "Tests" entry in the `.sln` is an empty solution folder. There are no test projects — don't invent `dotnet test` commands.
- Hosts wire up the database through `services.AddDsoDatabase(configuration)` (`Infraestructure/Extensions/ServiceCollectionExtensions.cs`). Don't bypass it.

## Architecture

Clean / onion architecture. Dependency direction:

```
Distribution → Application → Domain ← Infraestructure
```

`Domain` (`DigitalWorldOnline.Commons`) has no project dependencies on the other layers and is referenced by everything else. The `Application` layer is **MediatR** (CQRS-style); cross-cutting use cases are `IRequest`/`IRequestHandler` pairs under `Routines/Commands/` and `Routines/Queries/`.

Each game-facing host follows a trio pattern: `XxxServer.cs` (TCP listener) → `XxxPacketReader.cs` (deserialization) → `XxxPacketProcessor.cs` (dispatch). `Game.Host` additionally splits handler logic into one file per packet under `PacketProcessors/`, plus `GameMasterCommandsProcessor.cs` for GM commands.
