using AutoMapper;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.GameHost;

/// <summary>
/// Driver for <see cref="MapTypeEnum.Pvp"/> maps — one instance per DB row,
/// no channels (matchmaking spaces don't need them).
/// </summary>
public sealed class PvpMapDriver : MapDriver
{
    public override MapTypeEnum Type => MapTypeEnum.Pvp;

    public override async Task RefreshInstances(
        ISender sender,
        IMapper mapper,
        MapRegistry registry,
        ILogger logger,
        CancellationToken ct)
    {
        var dtos = await sender.Send(new GameMapsConfigQuery(MapTypeEnum.Pvp), ct);

        foreach (var dto in dtos)
        {
            if (registry.GetByMapAndChannel(MapTypeEnum.Pvp, dto.MapId, channelIdx: 0) != null)
                continue;

            var cfg = mapper.Map<MapConfigModel>(dto);
            var def = new MapDefinition(cfg);
            var instance = new MapInstance(def, channelIdx: 0, cfg.Mobs, cfg.SummonMobs, cfg.KillSpawns);

            logger.Debug("Initializing new instance for {Type} map {Id} - {Name}...",
                def.Type, def.Id, def.Name);
            registry.Register(MapTypeEnum.Pvp, instance);
        }
    }

    /// <summary>
    /// Pvp tick — tamer-op only, no monsters / drops.  Matches the legacy
    /// PvpServerBaseOperation.RunMap body exactly.
    /// </summary>
    public override async Task RunMap(
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
            await Task.WhenAll(Task.Run(() => tamerOp(map), ct));
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
