namespace VeloFile.Corpus.Tests.TestRuntime;

internal static class RepoOutputSnapshot
{
    public static string[] CaptureGeneratedOutputPaths(DirectoryInfo repoRoot)
    {
        var paths = new[]
        {
            Path.Combine(repoRoot.FullName, ".velofile-corpus-root"),
            Path.Combine(repoRoot.FullName, ".velofile-tools"),
            Path.Combine(repoRoot.FullName, "corpora"),
            Path.Combine(repoRoot.FullName, "diagnostics"),
            Path.Combine(repoRoot.FullName, "benchmarks"),
            Path.Combine(repoRoot.FullName, "tools", "VeloFile.Corpus", "publish"),
            Path.Combine(repoRoot.FullName, "src", "VeloFile.Core", "publish"),
            Path.Combine(repoRoot.FullName, "src", "VeloFile.Windows", "publish")
        };

        return Capture(paths);
    }

    public static string[] Capture(IEnumerable<string> paths)
    {
        return paths
            .SelectMany(CapturePath)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<string> CapturePath(string path)
    {
        if (File.Exists(path))
        {
            yield return "file:" + path;
            yield break;
        }

        if (!Directory.Exists(path))
        {
            yield return "missing:" + path;
            yield break;
        }

        yield return "dir:" + path;
        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            yield return "child:" + Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
