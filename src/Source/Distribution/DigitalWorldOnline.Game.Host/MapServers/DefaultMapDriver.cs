using AutoMapper;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.GameHost;

/// <summary>
/// Driver for <see cref="MapTypeEnum.Default"/> maps — the open-world / town /
/// field maps that every player passes through.  Spawns
/// <see cref="BaselineChannelsPerMap"/> instances of every default map at boot
/// so each map has multiple channels available immediately; auto-scales up
/// when populations saturate, and tears idle non-baseline channels down.
/// </summary>
public sealed class DefaultMapDriver : MapDriver
{
    private readonly MapBinLoader _mapBin;
    private readonly MonsterBinLoader _monsterBin;
    private readonly DigimonListBinLoader _digimonListBin;

    public DefaultMapDriver(MapBinLoader mapBin, MonsterBinLoader monsterBin, DigimonListBinLoader digimonListBin)
    {
        _mapBin = mapBin;
        _monsterBin = monsterBin;
        _digimonListBin = digimonListBin;
    }

    /// <summary>
    /// Always-on channel count per default map.  Channels 0..N-1 spawn at boot
    /// and are never auto-closed (Phase E Step 5 protection — channel 0 stays
    /// pinned even if empty so /channels always returns ≥ 1 entry).
    /// </summary>
    public const int BaselineChannelsPerMap = 3;

    /// <summary>Population at which a new channel is spawned (every existing channel ≥ this).</summary>
    private const int ScaleUpThreshold = 200;

    /// <summary>Population below which a non-baseline channel is candidate for tear-down.</summary>
    private const int ScaleDownThreshold = 30;

    /// <summary>Headroom required on the absorbing channel before tear-down can fire.</summary>
    private const int ScaleDownHeadroom = ScaleUpThreshold - 30;

    /// <summary>
    /// Hard cap on simultaneous channels per map.  Matches v487 client's
    /// <c>nLimit::Channel = 32</c> in <c>common_vs2019/pLimit.h</c> (the
    /// in-source comment notes retail capped creation at 16 in practice, but
    /// the client arrays hold 32 slots so we use the array max).
    /// </summary>
    private const int MaxChannelsPerMap = 32;

    public override MapTypeEnum Type => MapTypeEnum.Default;

    public override async Task RefreshInstances(
        ISender sender,
        IMapper mapper,
        MapRegistry registry,
        ILogger logger,
        CancellationToken ct)
    {
        var dtos = BuildDefaultMapsFromBins();

        foreach (var dto in dtos)
        {
            // Map the DTO once to harvest the catalog shape (Id, MapId, Name,
            // Type, DungeonIdTemplate).  Per channel, do a *fresh* map to get
            // independent mob / summon / kill-spawn lists.
            var anchor = mapper.Map<MapConfigModel>(dto);
            var def = new MapDefinition(anchor);

            // Baseline pass — make sure channels 0..N-1 exist.
            for (byte channelIdx = 0; channelIdx < BaselineChannelsPerMap; channelIdx++)
            {
                if (registry.GetByMapAndChannel(MapTypeEnum.Default, def.MapId, channelIdx) != null)
                    continue;

                Spawn(def, dto, mapper, registry, channelIdx, logger);
            }

            // Phase E Step 5 — auto-scale up.  When *every* live channel of
            // this map is at or above the threshold, spawn a fresh channel
            // at maxIdx+1 (capped).
            if (NeedsScaleUp(def.MapId, registry))
            {
                byte newIdx = (byte)(registry.HighestChannelIdx(MapTypeEnum.Default, def.MapId) + 1);
                if (newIdx < MaxChannelsPerMap)
                {
                    logger.Information(
                        "Auto-scale-up on map {MapId} ({Name}): spawning channel {Channel} (all existing channels >= {Threshold})",
                        def.MapId, def.Name, newIdx, ScaleUpThreshold);
                    Spawn(def, dto, mapper, registry, newIdx, logger);
                }
                else
                {
                    logger.Warning(
                        "Auto-scale-up wanted on map {MapId} ({Name}) but channel cap {Cap} reached",
                        def.MapId, def.Name, MaxChannelsPerMap);
                }
            }
        }

        // Phase E Step 5 — auto-scale down.  Channel 0..(BaselineChannelsPerMap-1)
        // are pinned; for higher channels, if population stayed below the
        // low-water mark and other channels have headroom, mark for teardown.
        // The driver's default CleanIdle (MapDriver base) handles actual
        // removal — we just flag via MapInstance.CloseMap when criteria match.
        foreach (var map in registry.GetByType(MapTypeEnum.Default))
        {
            if (map.Channel < BaselineChannelsPerMap)
                continue;
            if (map.Clients.Count >= ScaleDownThreshold)
                continue;
            if (map.CloseMap)
                continue; // already flagged

            var siblings = registry.GetChannelsOf(MapTypeEnum.Default, map.MapId);
            bool hasAbsorber = false;
            foreach (var s in siblings)
            {
                if (ReferenceEquals(s, map)) continue;
                if (s.Clients.Count <= ScaleDownHeadroom)
                {
                    hasAbsorber = true;
                    break;
                }
            }
            if (!hasAbsorber)
                continue;

            // TODO Phase E follow-up: actively migrate any straggling clients
            // before the channel is unregistered (push them MapSwap to channel 0).
            // For now: only tear down when the channel is already empty.
            if (map.Clients.Count > 0)
                continue;

            logger.Information(
                "Auto-scale-down: flagging map {MapId} ({Name}) channel {Channel} for teardown (idle, absorber available)",
                map.MapId, map.Name, map.Channel);
            map.MarkForClose();
        }
    }

    private List<MapConfigDTO> BuildDefaultMapsFromBins()
    {
        if (!_mapBin.IsLoaded || !_monsterBin.IsLoaded)
            throw new InvalidOperationException("Map static catalogs must come from bins (DefaultMapDriver).");

        var result = new List<MapConfigDTO>(_mapBin.Data.MapsById.Count);
        foreach (var map in _mapBin.Data.MapsById.Values.OrderBy(x => x.MapId))
        {
            var mobs = new List<MobConfigDTO>();
            if (_mapBin.Data.MonstersByMapId.TryGetValue(map.MapId, out var mapMobs))
            {
                long id = 1;
                foreach (var mapMob in mapMobs)
                {
                    foreach (var spawn in MapMonsterSpawnMaterializer.Expand(mapMob))
                    {
                        if (!_monsterBin.Data.ByType.TryGetValue(spawn.MonsterTableId, out var mon))
                            continue;

                        long mobId = id++;
                        var digimon = _digimonListBin.Data.FindByType(spawn.MonsterTableId);
                        var attribute = digimon != null && Enum.IsDefined(typeof(DigimonAttributeEnum), digimon.Attribute)
                            ? (DigimonAttributeEnum)digimon.Attribute
                            : DigimonAttributeEnum.None;
                        var element = digimon != null && Enum.IsDefined(typeof(DigimonElementEnum), digimon.Element)
                            ? (DigimonElementEnum)digimon.Element
                            : DigimonElementEnum.Neutral;
                        var family1 = digimon != null && Enum.IsDefined(typeof(DigimonFamilyEnum), digimon.Family1)
                            ? (DigimonFamilyEnum)digimon.Family1
                            : DigimonFamilyEnum.None;
                        var family2 = digimon != null && Enum.IsDefined(typeof(DigimonFamilyEnum), digimon.Family2)
                            ? (DigimonFamilyEnum)digimon.Family2
                            : DigimonFamilyEnum.None;
                        var family3 = digimon != null && Enum.IsDefined(typeof(DigimonFamilyEnum), digimon.Family3)
                            ? (DigimonFamilyEnum)digimon.Family3
                            : DigimonFamilyEnum.None;

                        mobs.Add(new MobConfigDTO
                        {
                            Id = mobId,
                            Type = spawn.MonsterTableId,
                            Model = mon.ModelId,
                            Name = $"Mob {spawn.MonsterTableId}",
                            Level = (byte)Math.Min(byte.MaxValue, mon.Level),
                            ViewRange = mon.Sight,
                            HuntRange = mon.HuntRange,
                            Class = mon.Class,
                            Coliseum = false,
                            Round = 0,
                            WeekDay = DungeonDayOfWeekEnum.Sunday,
                            ColiseumMobType = ColiseumMobTypeEnum.Normal,
                            ReactionType = DigimonReactionTypeEnum.Passive,
                            Attribute = attribute,
                            Element = element,
                            Family1 = family1,
                            Family2 = family2,
                            Family3 = family3,
                            RespawnInterval = spawn.RespawnSeconds,
                            HPValue = mon.Hp,
                            DSValue = mon.Ds,
                            DEValue = mon.DefPower,
                            EVValue = mon.Evasion,
                            MSValue = mon.MoveSpeed,
                            WSValue = mon.WalkSpeed,
                            CTValue = mon.CriticalRate,
                            ATValue = mon.AttPower,
                            ASValue = mon.AttSpeed,
                            ARValue = mon.AttRange,
                            HTValue = mon.HitRate,
                            BLValue = 0,
                            Location = new MobLocationConfigDTO
                            {
                                Id = mobId,
                                MobConfigId = mobId,
                                MapId = (short)map.MapId,
                                X = spawn.X,
                                Y = spawn.Y
                            },
                            ExpReward = new MobExpRewardConfigDTO
                            {
                                Id = mobId,
                                MobId = mobId,
                                TamerExperience = mon.ExpMax,
                                DigimonExperience = mon.ExpMax
                            },
                            DropReward = new MobDropRewardConfigDTO
                            {
                                Id = mobId,
                                MobId = mobId
                            },
                            GameMapConfigId = map.MapId
                        });
                    }
                }
            }

            result.Add(new MapConfigDTO
            {
                Id = map.MapId,
                MapId = map.MapId,
                Name = $"Map {map.MapId}",
                Type = MapTypeEnum.Default,
                Mobs = mobs,
                KillSpawns = new()
            });
        }

        return result;
    }

    private static void Spawn(
        MapDefinition def,
        DigitalWorldOnline.Commons.DTOs.Config.MapConfigDTO dto,
        IMapper mapper,
        MapRegistry registry,
        byte channelIdx,
        ILogger logger)
    {
        var perChannel = mapper.Map<MapConfigModel>(dto);
        var instance = new MapInstance(
            def,
            channelIdx,
            perChannel.Mobs,
            perChannel.SummonMobs,
            perChannel.KillSpawns);

        logger.Debug(
            "Initializing instance for {Type} map {Id} - {Name} (channel {Channel})...",
            def.Type, def.Id, def.Name, channelIdx);
        registry.Register(MapTypeEnum.Default, instance);
    }

    private static bool NeedsScaleUp(int mapId, MapRegistry registry)
    {
        var channels = registry.GetChannelsOf(MapTypeEnum.Default, mapId);
        if (channels.Count == 0) return false;
        foreach (var c in channels)
            if (c.Clients.Count < ScaleUpThreshold)
                return false;
        return true;
    }
}
