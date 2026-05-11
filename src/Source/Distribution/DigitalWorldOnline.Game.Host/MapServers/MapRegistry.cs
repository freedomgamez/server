using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;

namespace DigitalWorldOnline.GameHost;

/// <summary>
/// Single source of truth for live <see cref="MapInstance"/> instances across all
/// four <c>*Server</c> siblings (default <see cref="MapServer"/>,
/// <see cref="DungeonsServer"/>, <see cref="PvpServer"/>, <see cref="EventServer"/>).
///
/// Indexed by <c>(MapTypeEnum, MapId, ChannelIdx)</c> for O(1) lookup.  Carries
/// tamer-id → map and handler → map caches that the per-frame combat / packet
/// hot path can read without scanning a flat <c>List&lt;MapInstance&gt;</c>.
///
/// The four servers each still hold their own <c>List&lt;MapInstance&gt;</c> for
/// backwards compatibility with the existing 77 lookup sites that haven't been
/// migrated yet (Map Rework Phase B); the registry shares the underlying list
/// references so reads via either path see the same data.  New code paths
/// should query the registry exclusively.
///
/// Map Rework Phase A — purely additive.  Phase B migrates the 77 sites.
/// </summary>
public sealed class MapRegistry
{
    // ─── core indexes ───────────────────────────────────────────────────────
    // Outer: MapTypeEnum (Default / Pvp / Event / Dungeon).
    // Middle: MapId (the int player-facing map number).
    // Inner: List of channels for that (type, mapId) — order matches insertion;
    //        accessor methods filter by ChannelIdx.
    private readonly Dictionary<MapTypeEnum, Dictionary<int, List<MapInstance>>> _byType = new();

    // Flat list per type — shared reference with each *Server's `Maps` list so
    // the legacy code path sees the same data.  Replaces the per-server new
    // List<MapInstance>() at construction.
    private readonly Dictionary<MapTypeEnum, List<MapInstance>> _flat = new();

    // tamerId → the MapInstance they're currently inside.  Updated by OnTamerEnter
    // / OnTamerLeave hooks that the existing client-add / client-remove paths
    // will call once Phase B migrates them.  Lookups walk the flat list as a
    // fallback when the cache is cold (e.g. first packet after login).
    private readonly Dictionary<long, MapInstance> _byTamerId = new();

    private readonly object _lock = new();

    public MapRegistry()
    {
        foreach (MapTypeEnum t in Enum.GetValues(typeof(MapTypeEnum)))
        {
            _byType[t] = new Dictionary<int, List<MapInstance>>();
            _flat[t] = new List<MapInstance>();
        }
    }

    // ─── per-server flat-list views (back-compat) ───────────────────────────

    /// <summary>
    /// Returns the same <see cref="List{MapInstance}"/> reference that should back
    /// each <c>*Server.Maps</c> property.  Callers MUST go through
    /// <see cref="Register"/> / <see cref="Unregister"/> for mutations; direct
    /// <c>.Add</c> on this list bypasses the indexes and breaks O(1) lookups.
    /// </summary>
    public List<MapInstance> GetFlatBacking(MapTypeEnum type) => _flat[type];

    // ─── lifecycle ──────────────────────────────────────────────────────────

    /// <summary>
    /// Add a <see cref="MapInstance"/> instance to the registry.  Called by each
    /// <c>*Server.SearchNewMaps</c> after AutoMapper builds the instance.
    /// Idempotent — registering the same <c>(type, Id, Channel)</c> twice is
    /// a no-op (preserves the existing entry).
    /// </summary>
    public void Register(MapTypeEnum type, MapInstance map)
    {
        lock (_lock)
        {
            var byMapId = _byType[type];
            if (!byMapId.TryGetValue(map.MapId, out var channels))
            {
                channels = new List<MapInstance>();
                byMapId[map.MapId] = channels;
            }

            if (channels.Any(m => m.Id == map.Id && m.Channel == map.Channel))
                return;

            channels.Add(map);
            _flat[type].Add(map);
        }
    }

    /// <summary>Remove a map instance from every index.  Called when a channel closes.</summary>
    public void Unregister(MapTypeEnum type, MapInstance map)
    {
        lock (_lock)
        {
            if (_byType[type].TryGetValue(map.MapId, out var channels))
            {
                channels.Remove(map);
                if (channels.Count == 0) _byType[type].Remove(map.MapId);
            }
            _flat[type].Remove(map);

            // Drop tamer-cache entries pointing at this instance.
            var stale = _byTamerId.Where(kv => ReferenceEquals(kv.Value, map))
                                  .Select(kv => kv.Key).ToList();
            foreach (var id in stale) _byTamerId.Remove(id);
        }
    }

    // ─── lookups ────────────────────────────────────────────────────────────

    public IReadOnlyList<MapInstance> GetByType(MapTypeEnum type)
    {
        // Defensive snapshot — callers iterating across map ticks shouldn't see
        // concurrent registration mid-loop.
        lock (_lock) return _flat[type].ToArray();
    }

    /// <summary>O(1) lookup of one specific channel of one map.</summary>
    public MapInstance? GetByMapAndChannel(MapTypeEnum type, int mapId, byte channelIdx)
    {
        lock (_lock)
        {
            if (!_byType[type].TryGetValue(mapId, out var channels)) return null;
            for (int i = 0; i < channels.Count; i++)
                if (channels[i].Channel == channelIdx) return channels[i];
            return null;
        }
    }

    /// <summary>All channels currently running for a given <c>(type, mapId)</c>.</summary>
    public IReadOnlyList<MapInstance> GetChannelsOf(MapTypeEnum type, int mapId)
    {
        lock (_lock)
        {
            if (!_byType[type].TryGetValue(mapId, out var channels))
                return Array.Empty<MapInstance>();
            return channels.ToArray();
        }
    }

    /// <summary>
    /// Find which map instance currently holds a given tamer.  Hot path —
    /// cache-first, falls back to a list walk only when the cache is cold.
    /// </summary>
    public MapInstance? FindByTamer(long tamerId)
    {
        lock (_lock)
        {
            if (_byTamerId.TryGetValue(tamerId, out var cached) && cached.Clients.Any(c => c.TamerId == tamerId))
                return cached;

            // Cold-path fallback — scan every type's flat list.  Updates the
            // cache so subsequent reads are O(1).
            foreach (var (_, list) in _flat)
            {
                foreach (var m in list)
                {
                    if (m.Clients.Any(c => c.TamerId == tamerId))
                    {
                        _byTamerId[tamerId] = m;
                        return m;
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Called from <see cref="MapInstance"/>'s tamer-enter path (Phase B will hook
    /// the existing <c>AddClient</c> / <c>RemoveClient</c> sites).  Keeps the
    /// tamer→map cache hot so combat-path lookups don't have to scan.
    /// </summary>
    public void OnTamerEnter(MapInstance map, long tamerId)
    {
        lock (_lock) _byTamerId[tamerId] = map;
    }

    public void OnTamerLeave(long tamerId)
    {
        lock (_lock) _byTamerId.Remove(tamerId);
    }

    /// <summary>
    /// Find a dungeon instance by its <see cref="MapInstance.DungeonId"/> (the
    /// party-id or solo tamer-id that scoped it).  Dungeons are not
    /// channel-keyed; this is the canonical lookup for the dungeon path.
    /// </summary>
    public MapInstance? FindByDungeonId(long dungeonId)
    {
        lock (_lock)
        {
            foreach (var m in _flat[MapTypeEnum.Dungeon])
                if (m.DungeonId == dungeonId) return m;
            return null;
        }
    }

    // ─── diagnostics ────────────────────────────────────────────────────────

    /// <summary>Total live channel-instances across every map of a given type.</summary>
    public int Count(MapTypeEnum type)
    {
        lock (_lock) return _flat[type].Count;
    }

    /// <summary>For <c>/channels</c> GM command (Channel Step 7).</summary>
    public IEnumerable<(int MapId, byte Channel, int Population)> Summarise(MapTypeEnum type)
    {
        lock (_lock)
        {
            foreach (var m in _flat[type])
                yield return (m.MapId, m.Channel, m.Clients.Count);
        }
    }

    // ─── Phase E — channel-pick helpers ─────────────────────────────────────

    /// <summary>
    /// Return the channel of <c>(type, mapId)</c> with the fewest connected
    /// clients.  Used by initial-map-entry to spread new arrivals across
    /// channels.  Returns <c>null</c> if no channel exists yet.
    /// </summary>
    public MapInstance? PickLowestPopulated(MapTypeEnum type, int mapId)
    {
        lock (_lock)
        {
            if (!_byType[type].TryGetValue(mapId, out var channels) || channels.Count == 0)
                return null;
            MapInstance? best = null;
            int bestCount = int.MaxValue;
            foreach (var c in channels)
            {
                if (!c.Initialized) continue;
                if (c.Clients.Count < bestCount)
                {
                    bestCount = c.Clients.Count;
                    best = c;
                }
            }
            return best;
        }
    }

    /// <summary>Highest channel index currently registered for a map.</summary>
    public byte HighestChannelIdx(MapTypeEnum type, int mapId)
    {
        lock (_lock)
        {
            if (!_byType[type].TryGetValue(mapId, out var channels) || channels.Count == 0)
                return 0;
            byte max = 0;
            foreach (var c in channels)
                if (c.Channel > max) max = c.Channel;
            return max;
        }
    }
}
