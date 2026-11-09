namespace Taipan.Compiler.Text;

/// <summary>
/// Paths built from one the user gave, written with the separators it was given with (SPEC §4.7), so
/// a message names `tests/cases/x/helper.tpn` on Windows too, when that's how the main file was named.
/// </summary>
public static class SourcePaths
{
    /// <summary>A file in the same directory as <paramref name="path"/>.</summary>
    public static string Beside(string path, string fileName)
    {
        var end = path.LastIndexOfAny(['/', '\\']);
        return end < 0 ? fileName : path[..(end + 1)] + fileName;
    }

    /// <summary>A path found under <paramref name="given"/>, with its separators if it used only '/'.</summary>
    public static string LikeGiven(string given, string found) =>
        Path.DirectorySeparatorChar == '\\' && !given.Contains('\\') ? found.Replace('\\', '/') : found;
}
