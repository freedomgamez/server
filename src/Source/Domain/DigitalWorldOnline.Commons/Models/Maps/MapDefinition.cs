using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Config;

namespace DigitalWorldOnline.Commons.Models.Map
{
    /// <summary>
    /// Immutable catalog half of a map — the data that's identical across every
    /// channel of the same <see cref="MapId"/> and never changes at runtime.
    ///
    /// Built once per map row at boot from the matching <see cref="MapConfigModel"/>
    /// AutoMapper output, then shared by reference across the N channel
    /// <see cref="MapInstance"/> instances that hang off it.  Per-channel runtime
    /// state (mobs, clients, drops, handlers, AoE zones, summon mobs grown at
    /// runtime, the per-instance <c>DungeonId</c>) lives on <see cref="MapInstance"/>.
    ///
    /// Phase F will replace the construction source with bin-loaded values; the
    /// shape stays the same so downstream code is unaffected.
    /// </summary>
    public sealed class MapDefinition
    {
        public long Id { get; }
        public int MapId { get; }
        public string Name { get; }
        public MapTypeEnum Type { get; }

        /// <summary>
        /// The DungeonId baked into the catalog row.  Instance state copies this
        /// into <c>MapInstance.DungeonId</c> at construction; dungeon instances then
        /// overwrite their copy with the per-party / per-tamer key.
        /// </summary>
        public long DungeonIdTemplate { get; }

        public MapDefinition(MapConfigModel cfg)
        {
            Id = cfg.Id;
            MapId = cfg.MapId;
            Name = cfg.Name;
            Type = cfg.Type;
            DungeonIdTemplate = cfg.DungeonId;
        }
    }
}
