namespace VeloFile.Windows.Preview;

internal static class WindowsPreviewLimits
{
    public const long MaxImageBytes = 100L * 1024 * 1024;
    public const long MaxTextBytes = 100L * 1024 * 1024;
    public const long MaxPdfBytes = 500L * 1024 * 1024;
    public const int MaxTextPreviewBytes = 1024 * 1024;
    public const int MaxImageDimensionPixels = 8192;
}
