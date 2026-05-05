using VeloFile.Core.Listing;

namespace VeloFile.Core.Visibility;

public sealed record VisibilitySettings(
    bool ShowHiddenFiles,
    bool ShowProtectedOperatingSystemFiles,
    bool ShowFileExtensions)
{
    public static VisibilitySettings Default { get; } = new(
        ShowHiddenFiles: false,
        ShowProtectedOperatingSystemFiles: false,
        ShowFileExtensions: true);
}

public static class FileVisibilityProjector
{
    private static readonly HashSet<string> KnownExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".cs",
        ".csv",
        ".doc",
        ".docx",
        ".exe",
        ".gif",
        ".jpeg",
        ".jpg",
        ".json",
        ".md",
        ".pdf",
        ".png",
        ".ppt",
        ".pptx",
        ".ps1",
        ".tif",
        ".tiff",
        ".txt",
        ".xls",
        ".xlsx",
        ".xml",
        ".zip"
    };

    public static ListedFileItem? Project(FileSystemEntrySnapshot entry, VisibilitySettings settings)
    {
        if (entry.IsProtectedOperatingSystemFile && !settings.ShowProtectedOperatingSystemFiles)
        {
            return null;
        }

        if (entry.IsHidden && !settings.ShowHiddenFiles)
        {
            return null;
        }

        return new ListedFileItem(
            FullPath: entry.FullPath,
            Name: entry.Name,
            DisplayName: DisplayName(entry, settings),
            Kind: entry.Kind,
            Length: entry.Length,
            LastWriteTimeUtc: entry.LastWriteTimeUtc,
            Attributes: entry.Attributes,
            IsHidden: entry.IsHidden,
            IsProtectedOperatingSystemFile: entry.IsProtectedOperatingSystemFile,
            IsVisuallyDimmed: entry.IsHidden || entry.IsProtectedOperatingSystemFile,
            CreationTimeUtc: entry.CreationTimeUtc,
            LastAccessTimeUtc: entry.LastAccessTimeUtc);
    }

    private static string DisplayName(FileSystemEntrySnapshot entry, VisibilitySettings settings)
    {
        if (settings.ShowFileExtensions || entry.Kind is FileSystemEntryKind.Directory)
        {
            return entry.Name;
        }

        var extension = Path.GetExtension(entry.Name);
        if (string.IsNullOrEmpty(extension) || !KnownExtensions.Contains(extension))
        {
            return entry.Name;
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(entry.Name);
        return string.IsNullOrEmpty(nameWithoutExtension) ? entry.Name : nameWithoutExtension;
    }
}
