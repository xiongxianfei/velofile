using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public static class WindowsPreviewProviderFactory
{
    public static IReadOnlyList<IPreviewProvider> CreateDefault()
    {
        return
        [
            new WindowsImagePreviewProvider(),
            new WindowsTextPreviewProvider(),
            new WindowsPdfPreviewProvider(),
            new MetadataOnlyPreviewProvider()
        ];
    }
}
