using DigitalWorldOnline.Commons.Models.Asset;

namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// Parses v487 <c>New_Element.bin</c> — identical byte layout to <c>Nature.bin</c>; both
/// are written by the same <c>NatureMng::SaveBin</c> path in the client and contain a
/// (newer) element-vs-element delta matrix.  Server consumers prefer New_Element values
/// over Nature when both are loaded — match what the client does (NatureMng loads them in
/// sequence with New_Element overwriting Nature for shared keys; in v487 the matrices
/// differ only in updated multipliers, not in keys).
/// </summary>
public sealed class NewElementBinLoader
{
    private const string FileName = "New_Element.bin";

    private NatureData? _data;

    public NatureData Data => _data ?? throw new InvalidOperationException(
        $"{nameof(NewElementBinLoader)}: bin not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public NatureData Load()
    {
        if (_data != null) return _data;
        _data = NatureBinLoader.LoadFromPath(System.IO.Path.Combine(BinPath.ResolveDirectory(), FileName));
        return _data;
    }
}
