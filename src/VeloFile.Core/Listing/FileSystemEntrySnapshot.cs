namespace VeloFile.Core.Listing;

public enum FileSystemEntryKind
{
    File,
    Directory,
    Other
}

public sealed record FileSystemEntrySnapshot(
    string FullPath,
    string Name,
    FileSystemEntryKind Kind,
    long? Length,
    DateTimeOffset? LastWriteTimeUtc,
    FileAttributes Attributes)
{
    public bool IsHidden => Attributes.HasFlag(FileAttributes.Hidden);

    public bool IsSystem => Attributes.HasFlag(FileAttributes.System);

    public bool IsProtectedOperatingSystemFile => IsHidden && IsSystem;
}

public sealed record ListedFileItem(
    string FullPath,
    string Name,
    string DisplayName,
    FileSystemEntryKind Kind,
    long? Length,
    DateTimeOffset? LastWriteTimeUtc,
    FileAttributes Attributes,
    bool IsHidden,
    bool IsProtectedOperatingSystemFile,
    bool IsVisuallyDimmed);
