using VeloFile.Core.DragDrop;
using VeloFile.Core.Listing;

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

        var items = fileDropPaths
            .Select(TryCreateDropItem)
            .OfType<DropItem>()
            .ToArray();

        return items.Length == 0
            ? WindowsOleDragDropData.Rejected("ole-drop-no-supported-files")
            : new WindowsOleDragDropData(CanDrop: true, items, ReasonCode: null);
    }

    private static DropItem? TryCreateDropItem(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath))
        {
            return new DropItem(fullPath, Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)), FileSystemEntryKind.Directory);
        }

        return File.Exists(fullPath)
            ? new DropItem(fullPath, Path.GetFileName(fullPath), FileSystemEntryKind.File)
            : null;
    }
}
