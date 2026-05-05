using VeloFile.Core.DragDrop;
using VeloFile.Core.Listing;
using System.Security;

namespace VeloFile.Windows.DragDrop;

public sealed record WindowsOleDragDropData(
    bool CanDrop,
    IReadOnlyList<DropItem> Items,
    string? ReasonCode)
{
    public static WindowsOleDragDropData Rejected(string reasonCode)
    {
        return new WindowsOleDragDropData(CanDrop: false, [], reasonCode);
    }
}

public sealed class WindowsOleDragDropDataAdapter
{
    public WindowsOleDragDropData ExtractFileDrop(IReadOnlyList<string> fileDropPaths)
    {
        if (fileDropPaths.Count == 0)
        {
            return WindowsOleDragDropData.Rejected("ole-drop-no-files");
        }

        var items = new List<DropItem>(fileDropPaths.Count);
        foreach (var path in fileDropPaths)
        {
            var projection = TryCreateDropItem(path);
            if (projection.ReasonCode is not null)
            {
                return WindowsOleDragDropData.Rejected(projection.ReasonCode);
            }

            items.Add(projection.Item!);
        }

        return items.Count == 0
            ? WindowsOleDragDropData.Rejected("ole-drop-no-supported-files")
            : new WindowsOleDragDropData(CanDrop: true, items, ReasonCode: null);
    }

    private static DropItemProjection TryCreateDropItem(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return DropItemProjection.Rejected("drop-path-invalid");
        }

        try
        {
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                return DropItemProjection.Accepted(new DropItem(
                    fullPath,
                    Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                    FileSystemEntryKind.Directory));
            }

            return File.Exists(fullPath)
                ? DropItemProjection.Accepted(new DropItem(fullPath, Path.GetFileName(fullPath), FileSystemEntryKind.File))
                : DropItemProjection.Rejected("drop-path-unsupported");
        }
        catch (Exception ex) when (IsPathProjectionException(ex))
        {
            return DropItemProjection.Rejected("drop-path-invalid");
        }
    }

    private static bool IsPathProjectionException(Exception ex)
    {
        return ex is ArgumentException
            or NotSupportedException
            or PathTooLongException
            or IOException
            or UnauthorizedAccessException
            or SecurityException;
    }

    private sealed record DropItemProjection(DropItem? Item, string? ReasonCode)
    {
        public static DropItemProjection Accepted(DropItem item)
        {
            return new DropItemProjection(item, ReasonCode: null);
        }

        public static DropItemProjection Rejected(string reasonCode)
        {
            return new DropItemProjection(Item: null, reasonCode);
        }
    }
}
