namespace VeloFile.Core.Navigation;

public sealed record BreadcrumbSegment(string DisplayName, string FullPath);

public static class BreadcrumbPath
{
    public static IReadOnlyList<BreadcrumbSegment> Parse(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var trimmed = path.Trim();
        var root = Path.GetPathRoot(trimmed);
        if (string.IsNullOrWhiteSpace(root))
        {
            return [new BreadcrumbSegment(trimmed, trimmed)];
        }

        var segments = new List<BreadcrumbSegment>
        {
            new(root, root)
        };

        var remainder = trimmed[root.Length..].Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrEmpty(remainder))
        {
            return segments;
        }

        var parts = remainder.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        var currentPath = root;
        foreach (var part in parts)
        {
            currentPath = Path.Combine(currentPath, part);
            segments.Add(new BreadcrumbSegment(part, currentPath));
        }

        return segments;
    }
}
