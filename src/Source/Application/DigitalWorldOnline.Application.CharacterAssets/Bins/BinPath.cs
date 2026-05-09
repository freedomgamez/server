namespace DigitalWorldOnline.Application.CharacterAssets.Bins;

internal static class BinPath
{
    /// <summary>
    /// Walk up from the running assembly's directory until we find a
    /// <c>Bins/data/bin/english/</c> sibling. This makes loaders robust to where the host
    /// happens to be launched from (project dir for <c>dotnet run</c>, publish dir for
    /// production deployments).
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
