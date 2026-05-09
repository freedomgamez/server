// FATIGUE_HOOK — playtime fatigue subsystem (DMBase.bin section 9, sPLAY_PANELTY).
// Korean-region anti-grind feature: as a tamer's session play-time grows, exp/drop
// rewards are multiplied by progressively smaller fractions (level 1 = full rate,
// level 2 = 70%, level 3 = 0%). Defaults OFF for non-Korean regions; flip via
// `Fatigue:Enabled` in appsettings.Development.json. Search for `FATIGUE_HOOK` to
// find every callsite.

using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.Entities;
using Microsoft.Extensions.Configuration;

namespace DigitalWorldOnline.Game.Managers;

public sealed class FatigueService
{
    private const string ConfigKey = "Fatigue:Enabled";

    /// <summary>
    /// Per-fatigue-level session-time threshold in minutes. Bin section 9 has 3 tiers
    /// (level 1 / 2 / 3) but doesn't carry the time thresholds — those are pCountry-specific
    /// in the v487 client. Defaulting to 60-minute steps (Korean MMO convention) for
    /// well-defined behavior when Fatigue:Enabled is true; tweak if a different region's
    /// thresholds are desired.
    /// </summary>
    private static readonly int[] LevelThresholdMinutes = { 0, 60, 120 }; // levels 1/2/3 start at 0/60/120 minutes of session

    private readonly DMBaseBinLoader _dmBase;
    private readonly bool _enabled;

    public FatigueService(DMBaseBinLoader dmBase, IConfiguration configuration)
    {
        _dmBase = dmBase;
        _enabled = bool.TryParse(configuration[ConfigKey], out var v) && v;
    }

    public bool IsEnabled => _enabled;

    /// <summary>
    /// Returns the current (expMultiplier, dropMultiplier) pair for the given client's
    /// session play-time. Both are in [0.0, 1.0]. Returns (1.0, 1.0) when disabled.
    /// </summary>
    public (decimal exp, decimal drop) GetMultipliers(GameClient client)
    {
        if (!_enabled) return (1m, 1m);

        var sessionMinutes = (DateTime.UtcNow - client.SessionStart).TotalMinutes;
        int level = ResolveLevel(sessionMinutes);

        if (!_dmBase.Data.PlayPenalty.TryGetValue(level, out var penalty))
            return (1m, 1m);

        // Bin's s_nExp / s_nDrop are percentage MULTIPLIERS (100 = full, 70 = 70%, 0 = none),
        // verified against client `ToolTip_PlayTime.cpp:151` which renders `100 - s_nExp` as
        // the "exp reduction" tooltip percentage.
        return (penalty.Exp / 100m, penalty.Drop / 100m);
    }

    private static int ResolveLevel(double sessionMinutes)
    {
        // Walk the threshold list in reverse — pick the highest level whose threshold has been crossed.
        for (int i = LevelThresholdMinutes.Length - 1; i >= 0; i--)
        {
            if (sessionMinutes >= LevelThresholdMinutes[i])
                return i + 1;  // levels are 1-indexed in the bin (matches s_nPaneltyLevel)
        }
        return 1;
    }
}
