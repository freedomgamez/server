# Digital-Shinka-Online

Private server emulator for Digimon Masters Online (v487 client).

## Static-data layout

Static game data (item lists, digimon stats, evolution chains, character-create tables, etc.) is **not** bundled with this repo — those files come from your own Pack03 extraction of the v487 client.

When standing up Character.Host (and eventually Game.Host), populate this layout under `Serv/SOURCE/`:

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

The loaders walk up from `AppContext.BaseDirectory` to find this directory at runtime, so they work for both `dotnet run` (project dir cwd) and published deployments.
