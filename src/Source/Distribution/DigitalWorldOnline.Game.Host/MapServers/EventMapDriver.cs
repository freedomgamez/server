using AutoMapper;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Map;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.GameHost;

/// <summary>
/// Driver for <see cref="MapTypeEnum.Event"/> maps.  Event maps are bootstrapped
/// in <c>EventServer.AddContent()</c> at construction time (not from DB), so
/// there's no periodic refresh + no automatic idle cleanup.  Event lifetimes
/// are managed by event-specific schedulers.
/// </summary>
public sealed class EventMapDriver : MapDriver
{
    public override MapTypeEnum Type => MapTypeEnum.Event;

    /// <summary>
    /// Pre-init hook that <see cref="EventServer.RunMap"/> uses to seed the
    /// map with randomly-placed cloned mobs the first time an event map ticks.
    /// EventServer wires this from its <c>GetMapMobs</c> implementation.
    /// </summary>
    public Func<MapInstance, CancellationToken, Task>? OnFirstTick { get; set; }

    public override Task RefreshInstances(
        ISender sender,
        IMapper mapper,
        MapRegistry registry,
        ILogger logger,
        CancellationToken ct) => Task.CompletedTask;

    public override void CleanIdle(MapRegistry registry, ILogger logger)
    {
        // No idle sweep — event maps live for the event's scheduled lifetime.
    }

    /// <summary>
    /// Event tick — sequential tamer/monster/drops, no stopwatch, no tail
    /// delay (events schedule their own pacing).  Calls <see cref="OnFirstTick"/>
    /// before the first <see cref="MapInstance.Initialize"/> if EventServer set it.
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
            if (!map.Initialized && OnFirstTick != null)
                await OnFirstTick(map, ct);

            map.Initialize();
            map.ManageHandlers();

            await tamerOp(map);
            await monsterOp(map);
            await dropsOp(map);
        }
        catch (Exception ex)
        {
            logger.Error("Unexpected error at event map running: {Message} {Stack}",
                ex.Message, ex.StackTrace);
        }
    }
}
