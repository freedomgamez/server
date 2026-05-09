using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Mechanics;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.Managers;

/// <summary>
/// Implements DMBase.bin section 6 (sGuild) auto-leveling for guilds.
///
/// v487 has no client UI to trigger guild level-up — the protocol's pGuild::LevelUp
/// receiver is gated by GUILD_RENEWAL which is undefined in v487. Instead, the level
/// is conveyed via GUILD_INFO at login (see CommunityContent.cpp:555). So leveling
/// is server-driven: when guild experience or membership crosses a bin threshold,
/// the server bumps the level and persists it. The client picks up the new level on
/// the next info refresh.
///
/// Bin requirements per next level:
///   - <c>Fame</c>           — required cumulative guild experience
///   - <c>NeedPerson</c>     — required current member count
///   - <c>MasterLevel</c>    — required guild-master tamer level
///
/// Not enforced (no v487 driver): <c>ItemNo1/2 + ItemCount1/2</c> (would need a
/// guild-treasury feature) and <c>MaxGuild2Master</c> (semantics unclear).
/// </summary>
public sealed class GuildLevelService
{
    private readonly DMBaseBinLoader _dmBase;
    private readonly ISender _sender;
    private readonly ILogger _logger;

    public GuildLevelService(DMBaseBinLoader dmBase, ISender sender, ILogger logger)
    {
        _dmBase = dmBase;
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Adds <paramref name="amount"/> to <paramref name="guild"/>'s experience and tries
    /// to level up if the bin's requirements for the next level are met. Returns true if
    /// at least one level-up happened.
    /// </summary>
    public async Task<bool> AddExperienceAndTryLevelUp(GuildModel guild, CharacterModel master, int amount)
    {
        if (guild == null) return false;

        var startLevel = guild.Level;
        guild.AddExperience(amount);

        // Try to bump level repeatedly while requirements are met (in case big exp grants
        // cross multiple thresholds at once).
        bool leveled = false;
        while (TryLevelUpOnce(guild, master)) leveled = true;

        if (leveled || amount > 0)
            await _sender.Send(new UpdateGuildLevelCommand(guild.Id, guild.Level, guild.CurrentExperience));

        if (leveled)
            _logger.Information("Guild {GuildId} {Name} leveled up: {OldLvl} -> {NewLvl} (exp {Exp}).",
                guild.Id, guild.Name, startLevel, guild.Level, guild.CurrentExperience);

        return leveled;
    }

    private bool TryLevelUpOnce(GuildModel guild, CharacterModel master)
    {
        int nextLevel = guild.Level + 1;
        if (!_dmBase.Data.GuildLevels.TryGetValue(nextLevel, out var next))
            return false; // already at max bin-defined level

        if (guild.CurrentExperience < next.Fame) return false;
        if (guild.Members.Count < next.NeedPerson) return false;
        if (master != null && master.Level < next.MasterLevel) return false;

        guild.LevelUp();
        return true;
    }
}
