namespace DigitalWorldOnline.Application.GameAssets.Bins;

internal static class BinPath
{
    /// <summary>
    /// Walk up from the running assembly's directory until we find a
    /// <c>Bins/data/bin/english/</c> sibling. Same logic as the CharacterAssets copy —
    /// duplicated rather than shared so the two Application-* projects don't depend
    /// on each other.
    /// </summary>
    public static string ResolveDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Bins", "data", "bin", "english");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            "Could not locate 'Bins/data/bin/english/' walking up from " +
            $"AppContext.BaseDirectory ({AppContext.BaseDirectory}).");
    }
}
