namespace VeloFile.Windows.Foundation;

public static class WindowsIntegrationBoundary
{
    public static string AssemblyName => "VeloFile.Windows";

    public static bool IsWindowsOnly => true;

    public static IReadOnlyList<string> AdapterCategories { get; } =
    [
        "Shell/Win32/WinRT interop",
        "OLE drag/drop",
        "Windows App SDK platform integration"
    ];
}
