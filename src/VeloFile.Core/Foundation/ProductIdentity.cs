namespace VeloFile.Core.Foundation;

public static class ProductIdentity
{
    public static string Name => "VeloFile";

    public static string Tagline => "fast, lightweight open-source file explorer";

    public static IReadOnlyList<string> SupportedWindowsVersions { get; } =
    [
        "Windows 10",
        "Windows 11"
    ];
}
