namespace DigitalWorldOnline.Application.CharacterAssets;

/// <summary>
/// Empty marker type. Used by host DI bootstraps as a stable handle on this assembly
/// for things like <c>services.AddMediatR(typeof(CharacterAssetsMarker).Assembly)</c>,
/// so the assembly reference doesn't depend on whichever handler happens to live here today.
/// </summary>
public sealed class CharacterAssetsMarker
{
}
