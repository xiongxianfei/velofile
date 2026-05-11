namespace VeloFile.App.ViewModels;

public enum FileListRowVisibilityKind
{
    Normal,
    Hidden,
    ProtectedSystem
}

public static class FileListRowOpacityResourceSelector
{
    public const string HiddenOpacityResourceKey = "VfFileListRowHiddenOpacity";

    public const string ProtectedOpacityResourceKey = "VfFileListRowProtectedOpacity";

    public static string? GetOpacityResourceKey(FileListRowViewModel row)
    {
        return row.VisibilityKind switch
        {
            FileListRowVisibilityKind.ProtectedSystem => ProtectedOpacityResourceKey,
            FileListRowVisibilityKind.Hidden => HiddenOpacityResourceKey,
            _ => null
        };
    }
}
