namespace VeloFile.App.Input;

internal static class WinUiStorageItemDropPayloadProjection
{
    public static AppDragDropPayload ProjectPaths(
        IReadOnlyList<string?> storageItemPaths,
        Func<IReadOnlyList<string>, AppDragDropPayload> projectFilesystemPaths)
    {
        if (storageItemPaths.Count == 0)
        {
            return AppDragDropPayload.Unsupported("ole-drop-no-files");
        }

        var filesystemPaths = new List<string>(storageItemPaths.Count);
        foreach (var path in storageItemPaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return AppDragDropPayload.Unsupported("drop-storageitem-path-unavailable");
            }

            filesystemPaths.Add(path);
        }

        return projectFilesystemPaths(filesystemPaths);
    }
}
