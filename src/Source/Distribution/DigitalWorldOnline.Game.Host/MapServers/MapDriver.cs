using AutoMapper;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.GameHost;

/// <summary>
/// Strategy for per-<see cref="MapTypeEnum"/> map operations.  Each of the four
/// hosting servers (default / dungeon / pvp / event) owns one concrete driver
/// that the server defers to for:
/// <list type="bullet">
///   <item><see cref="RefreshInstances"/> — how to load definitions + instantiate
///   new <see cref="MapInstance"/> instances (1:1 vs N-channels vs per-party vs
///   never-from-DB) and register them with <see cref="MapRegistry"/>.</item>
///   <item><see cref="CleanIdle"/> — when to tear instances down.  Default
///   behaviour sweeps anything with <see cref="MapInstance.CloseMap"/> set.</item>
///   <item>Broadcasts (<see cref="BroadcastForMap"/> and friends) + lookups
///   (<see cref="GetMobByHandler"/>, <see cref="MobsAttacking"/>, drop helpers,
///   nearby-tamer/mob queries).  These collapse the 4× near-identical bodies
///   each <c>*ServerBaseOperation</c> previously copied from the others.</item>
///   <item><see cref="RunMap"/> — per-tick body.  Default = parallel
///   tamer/monster/drops + 500 ms delay (MapServer / Dungeons).  Pvp + Event
///   override to drop monster/drops and run sequentially.</item>
/// </list>
/// </summary>
public abstract class MapDriver
{
    public abstract MapTypeEnum Type { get; }

    /// <summary>
    /// Periodic lifecycle: load static definitions from the source of truth
    /// (DB today, bin in Phase F) and register any new instances with the
    /// registry.  Called from the owning <c>*Server</c>'s periodic poll.
    /// </summary>
    public abstract Task RefreshInstances(
        ISender sender,
        IMapper mapper,
        MapRegistry registry,
        ILogger logger,
        CancellationToken ct);

    /// <summary>
    /// Sweep idle instances (default: anything where <see cref="MapInstance.CloseMap"/>
    /// is set) and remove them from the registry.  Override to add per-type
    /// constraints (e.g. "never close channel 0" — Phase E).
    /// </summary>
    public virtual void CleanIdle(MapRegistry registry, ILogger logger)
    {
        var removed = registry.GetByType(Type).Where(m => m.CloseMap).ToList();
        foreach (var map in removed)
        {
            logger.Debug("Removing inactive instance for {Type} map {Id} - {Name}...",
                map.Type, map.Id, map.Name);
            registry.Unregister(Type, map);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // Broadcast helpers — collapse the 4× duplicated bodies on the *Server
    // classes.  All four servers had identical implementations; the only
    // axis that varied was the <see cref="MapTypeEnum"/> used to filter the
    // registry, which the driver provides.
    // ──────────────────────────────────────────────────────────────────────

    public void BroadcastForChannel(MapRegistry registry, byte channel, byte[] packet)
    {
        foreach (var map in registry.GetByType(Type))
            if (map.Channel == channel)
                map.BroadcastForMap(packet);
    }

    public void BroadcastGlobal(MapRegistry registry, byte[] packet)
    {
        foreach (var map in registry.GetByType(Type))
            if (map.Clients.Any())
                map.BroadcastForMap(packet);
    }

    public void BroadcastForMap(MapRegistry registry, short mapId, byte[] packet)
    {
        foreach (var map in registry.GetChannelsOf(Type, mapId))
            map.BroadcastForMap(packet);
    }

    public void BroadcastForUniqueTamer(MapRegistry registry, long tamerId, byte[] packet)
    {
        var map = registry.FindByTamer(tamerId);
        map?.BroadcastForUniqueTamer(tamerId, packet);
    }

    public void BroadcastForTargetTamers(MapRegistry registry, List<long> targetTamers, byte[] packet)
    {
        // Event/Pvp/Dungeons are single-instance-per-key so the first target's
        // map covers the rest.  For default maps the first target's channel is
        // what we broadcast on — cross-channel raids would need an explicit fan
        // out (Phase E).
        if (targetTamers.Count == 0) return;
        var map = registry.FindByTamer(targetTamers[0]);
        map?.BroadcastForTargetTamers(targetTamers, packet);
    }

    public void BroadcastForTargetTamers(MapRegistry registry, long sourceId, byte[] packet)
    {
        var map = registry.FindByTamer(sourceId);
        if (map == null) return;
        map.BroadcastForTargetTamers(map.TamersView[sourceId], packet);
    }

    public void BroadcastForTamerViewsAndSelf(MapRegistry registry, long sourceId, byte[] packet)
    {
        var map = registry.FindByTamer(sourceId);
        map?.BroadcastForTamerViewsAndSelf(sourceId, packet);
    }

    // ──────────────────────────────────────────────────────────────────────
    // Drop / lookup helpers — same dedup story.
    // ──────────────────────────────────────────────────────────────────────

    public void AddMapDrop(MapRegistry registry, Drop drop)
    {
        var map = registry.GetChannelsOf(Type, drop.Location.MapId).FirstOrDefault();
        map?.DropsToAdd.Add(drop);
    }

    public void RemoveDrop(MapRegistry registry, Drop drop)
    {
        var map = registry.GetChannelsOf(Type, drop.Location.MapId).FirstOrDefault();
        map?.RemoveMapDrop(drop);
    }

    public Drop? GetDrop(MapRegistry registry, short mapId, int dropHandler)
        => registry.GetChannelsOf(Type, mapId).FirstOrDefault()?.GetDrop(dropHandler);

    public bool MobsAttacking(MapRegistry registry, short mapId, long tamerId)
    {
        var map = registry.FindByTamer(tamerId)
            ?? registry.GetChannelsOf(Type, mapId).FirstOrDefault();
        return map?.MobsAttacking(tamerId) ?? false;
    }

    public List<CharacterModel> GetNearbyTamers(MapRegistry registry, short mapId, long tamerId)
    {
        var map = registry.FindByTamer(tamerId)
            ?? registry.GetChannelsOf(Type, mapId).FirstOrDefault();
        return map?.NearbyTamers(tamerId);
    }

    public MobConfigModel? GetMobByHandler(MapRegistry registry, short mapId, int handler)
        => registry.GetChannelsOf(Type, mapId).FirstOrDefault()?
              .Mobs.FirstOrDefault(x => x.GeneralHandler == handler);

    public MapInstance? FindMapForLocation(MapRegistry registry, Location location)
        => registry.GetChannelsOf(Type, location.MapId).FirstOrDefault();

    public void AddSummonMobs(MapRegistry registry, short mapId, Commons.Models.Summon.SummonMobModel summon)
    {
        var map = registry.GetChannelsOf(Type, mapId).FirstOrDefault();
        map?.AddMob(summon);
    }

    public List<MobConfigModel> GetMobsNearbyPartner(MapRegistry registry, Location location, int range)
    {
        var targetMap = registry.GetChannelsOf(Type, location.MapId).FirstOrDefault();
        if (targetMap == null) return new List<MobConfigModel>();

        return targetMap.Mobs
            .Where(m => m.Alive
                     && EuclideanWithin(location.X, location.Y, m.CurrentLocation.X, m.CurrentLocation.Y, range))
            .DistinctBy(m => m.Id)
            .ToList();
    }

    public List<MobConfigModel> GetMobsNearbyTargetMob(MapRegistry registry, short mapId, int handler, int range)
    {
        var targetMap = registry.GetChannelsOf(Type, mapId).FirstOrDefault();
        if (targetMap == null) return new List<MobConfigModel>();

        var originMob = targetMap.Mobs.FirstOrDefault(m => m.GeneralHandler == handler);
        if (originMob == null) return new List<MobConfigModel>();

        var result = new List<MobConfigModel> { originMob };
        result.AddRange(targetMap.Mobs
            .Where(m => m.Alive
                     && EuclideanWithin(originMob.CurrentLocation.X, originMob.CurrentLocation.Y,
                                        m.CurrentLocation.X, m.CurrentLocation.Y, range)));
        return result.DistinctBy(m => m.Id).ToList();
    }

    private static bool EuclideanWithin(int ax, int ay, int bx, int by, int range)
    {
        long dx = ax - bx, dy = ay - by;
        return dx * dx + dy * dy <= (long)range * range;
    }

    // ──────────────────────────────────────────────────────────────────────
    // Per-tick driver.  Default = parallel TamerOp + MonsterOp + DropsOp with
    // a 500 ms tail delay (MapServer / Dungeons).  Pvp/Event override.
    // The three operation callbacks come from the owning *Server (they vary
    // per type — Pvp doesn't run mobs/drops, etc.).
    // ──────────────────────────────────────────────────────────────────────

    public virtual async Task RunMap(
        MapInstance map,
        Func<MapInstance, Task> tamerOp,
        Func<MapInstance, Task> monsterOp,
        Func<MapInstance, Task> dropsOp,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            map.Initialize();
            map.ManageHandlers();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await Task.WhenAll(
                Task.Run(() => tamerOp(map), ct),
                Task.Run(() => monsterOp(map), ct),
                Task.Run(() => dropsOp(map), ct));
            stopwatch.Stop();

            if (stopwatch.Elapsed.TotalMilliseconds >= 1000)
                logger.Warning("RunMap ({Type} {MapId} ch{Channel}): {Ms} ms",
                    map.Type, map.MapId, map.Channel, stopwatch.Elapsed.TotalMilliseconds);

            await Task.Delay(500, ct);
        }
        catch (Exception ex)
        {
            logger.Error("Unexpected error at map running: {Message} {Stack}",
                ex.Message, ex.StackTrace);
        }
    }
}
