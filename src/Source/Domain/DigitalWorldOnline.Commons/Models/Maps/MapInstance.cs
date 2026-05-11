using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Models.TamerShop;

namespace DigitalWorldOnline.Commons.Models.Map
{
    /// <summary>
    /// Per-channel runtime state for one (MapType, MapId, Channel) tuple.
    /// Holds a reference to its <see cref="MapDefinition"/> for catalog data
    /// (<c>Id</c>, <c>MapId</c>, <c>Name</c>, <c>Type</c>) — those flow through
    /// passthrough properties so legacy callers keep working unchanged.
    ///
    /// Mobs / SummonMobs / KillSpawns live here, not on the definition, because
    /// they mutate per-instance (HP, alive/dead, summon-grown lists).  The
    /// definition shapes are loaded from DB / bin once per <see cref="MapId"/>;
    /// each channel's <see cref="MapInstance"/> gets its own freshly-mapped runtime
    /// copies of the mob/spawn templates so combat on channel 0 doesn't bleed
    /// into channel 1.
    /// </summary>
    public sealed partial class MapInstance
    {
        // ─── catalog (immutable for this instance) ──────────────────────────
        public MapDefinition Definition { get; private set; } = default!;

        public long Id => Definition.Id;
        public int MapId => Definition.MapId;
        public string Name => Definition.Name;
        public MapTypeEnum Type => Definition.Type;

        // ─── runtime state ──────────────────────────────────────────────────

        /// <summary>
        /// Per-instance key.  For default/event/pvp maps, mirrors
        /// <c>Definition.DungeonIdTemplate</c>.  For dungeon instances this is
        /// overwritten via <see cref="SetId"/> with the party-id (or solo
        /// tamer-id) that owns the instance.
        /// </summary>
        public long DungeonId { get; private set; }

        public byte Channel { get; private set; }
        public DateTime WithoutTamers { get; private set; }
        public DateTime NextDatabaseOperation { get; private set; }
        public bool Initialized { get; private set; }
        public bool Operating { get; private set; }
        public bool UpdateMobs { get; private set; }

        public List<MobConfigModel> Mobs { get; private set; }
        public List<SummonMobModel> SummonMobs { get; private set; }
        public List<KillSpawnConfigModel> KillSpawns { get; private set; }

        public List<MobConfigModel> MobsToAdd { get; private set; }
        public List<MobConfigModel> MobsToRemove { get; private set; }
        public List<GameClient> Clients { get; private set; }
        public List<Drop> Drops { get; private set; }
        public List<ConsignedShop> ConsignedShops { get; private set; }
        public Dictionary<long, List<long>> TamersView { get; private set; }
        public Dictionary<long, List<long>> MobsView { get; private set; }
        public Dictionary<long, List<long>> DropsView { get; private set; }
        public Dictionary<long, List<long>> ConsignedShopView { get; private set; }
        public Dictionary<short, long> TamerHandlers { get; private set; }
        public Dictionary<short, long> DigimonHandlers { get; private set; }
        public Dictionary<short, long> MobHandlers { get; private set; }
        public Dictionary<short, long> DropHandlers { get; private set; }
        public List<int> ColiseumMobs = new();

        public object DropsLock { get; private set; }
        public object ClientsLock { get; private set; }
        public object DigimonHandlersLock { get; private set; }
        public object TamerHandlersLock { get; private set; }

        // ─── construction ───────────────────────────────────────────────────

        /// <summary>
        /// Canonical constructor (Phase C).  Caller is responsible for handing
        /// over fresh, per-instance mob / summon / kill-spawn lists — these
        /// lists mutate at runtime and must not be shared between channels.
        /// </summary>
        public MapInstance(
            MapDefinition definition,
            byte channelIdx,
            List<MobConfigModel> mobs,
            List<SummonMobModel> summonMobs,
            List<KillSpawnConfigModel> killSpawns)
        {
            Definition = definition;
            DungeonId = definition.DungeonIdTemplate;
            Channel = channelIdx;

            Mobs = mobs ?? new List<MobConfigModel>();
            SummonMobs = summonMobs ?? new List<SummonMobModel>();
            KillSpawns = killSpawns ?? new List<KillSpawnConfigModel>();

            Drops = new List<Drop>();
            Clients = new List<GameClient>();
            ConsignedShops = new List<ConsignedShop>();

            WithoutTamers = DateTime.MaxValue;
            NextDatabaseOperation = DateTime.Now.AddSeconds(30);

            DropsLock = new object();
            ClientsLock = new object();
            DigimonHandlersLock = new object();
            TamerHandlersLock = new object();
        }

        /// <summary>
        /// Legacy in-process constructor used by <c>EventServer.AddContent</c>
        /// to seed event maps before any DB row exists for them.  Synthesises a
        /// minimal <see cref="MapDefinition"/> and an empty runtime.
        /// </summary>
        public MapInstance(short mapId, List<MobConfigModel> mobs, List<Drop> drops)
            : this(
                BuildLegacyDefinition(mapId),
                channelIdx: 0,
                mobs: mobs,
                summonMobs: new List<SummonMobModel>(),
                killSpawns: new List<KillSpawnConfigModel>())
        {
            Drops = drops ?? new List<Drop>();
        }

        private static MapDefinition BuildLegacyDefinition(short mapId)
        {
            var cfg = new MapConfigModel();
            cfg.SetMapId(mapId);
            return new MapDefinition(cfg);
        }

        /// <summary>
        /// Parameterless constructor retained for any remaining
        /// reflection/serialisation use (AutoMapper, JSON).  Sets a zero-valued
        /// definition that must be replaced before the instance is used.
        /// </summary>
        public MapInstance()
        {
            Definition = new MapDefinition(new MapConfigModel());
            Channel = 0;
            DungeonId = 0;

            Mobs = new List<MobConfigModel>();
            SummonMobs = new List<SummonMobModel>();
            KillSpawns = new List<KillSpawnConfigModel>();
            Drops = new List<Drop>();
            Clients = new List<GameClient>();
            ConsignedShops = new List<ConsignedShop>();

            WithoutTamers = DateTime.MaxValue;
            NextDatabaseOperation = DateTime.Now.AddSeconds(30);

            DropsLock = new object();
            ClientsLock = new object();
            DigimonHandlersLock = new object();
            TamerHandlersLock = new object();
        }

        // ─── mutators (for code that previously used MapConfigModelBehavior) ──

        public void SetId(int dungeonId) => DungeonId = dungeonId;
    }
}
