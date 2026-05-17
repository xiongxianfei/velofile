using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.App.Ui;

public enum FileListIconKind
{
    FileGeneric,
    Folder,
    Pdf,
    Image,
    Text,
    Spreadsheet,
    Executable,
    Markdown,
    ThumbnailFallback
}

public static class FileListIconKindResolver
{
    public static FileListIconKind Resolve(ListedFileItem item, ThumbnailState thumbnail)
    {
        if (item.Kind is FileSystemEntryKind.Directory)
        {
            return FileListIconKind.Folder;
        }

        if (thumbnail.Status is ThumbnailStatus.Failed
            || string.Equals(thumbnail.ReasonCode, "thumbnail-fallback", StringComparison.Ordinal)
            || string.Equals(thumbnail.ReasonCode, "thumbnail-timeout", StringComparison.Ordinal))
        {
            return FileListIconKind.ThumbnailFallback;
        }

        var normalizedName = item.Name.ToLowerInvariant();
        if (normalizedName is ".env")
        {
            return FileListIconKind.Text;
        }

        return Path.GetExtension(normalizedName) switch
        {
            ".pdf" => FileListIconKind.Pdf,
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" => FileListIconKind.Image,
            ".txt" or ".ini" or ".env" or ".log" or ".json" or ".xml" => FileListIconKind.Text,
            ".md" or ".markdown" => FileListIconKind.Markdown,
            ".xls" or ".xlsx" or ".csv" => FileListIconKind.Spreadsheet,
            ".exe" or ".msi" or ".bat" or ".cmd" or ".ps1" => FileListIconKind.Executable,
            _ => FileListIconKind.FileGeneric
        };
    }
}
