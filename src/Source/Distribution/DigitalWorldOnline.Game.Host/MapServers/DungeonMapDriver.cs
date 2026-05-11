using AutoMapper;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Map;
using DigitalWorldOnline.Game.Managers;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.GameHost;

/// <summary>
/// Driver for <see cref="MapTypeEnum.Dungeon"/> maps — per-party instanced.
/// Channels do NOT apply here: each party (or solo tamer) gets its own private
/// <see cref="MapInstance"/>, keyed by <c>DungeonId == party.Id</c> (or
/// <c>== client.TamerId</c> when soloing).
///
/// The periodic <see cref="RefreshInstances"/> hook is a no-op — dungeons are
/// spawned on demand by the legacy <c>DungeonsServerBaseOperation.SearchNewMaps
/// (bool IsParty, GameClient client)</c> path when a party enters one.  That
/// on-demand path stays bespoke (party + region filters + per-day mob culling
/// don't generalise to a strategy method); the driver here is responsible only
/// for the lifecycle cadence.
/// </summary>
public sealed class DungeonMapDriver : MapDriver
{
    public override MapTypeEnum Type => MapTypeEnum.Dungeon;

    public override Task RefreshInstances(
        ISender sender,
        IMapper mapper,
        MapRegistry registry,
        ILogger logger,
        CancellationToken ct) => Task.CompletedTask;
}
