using Microsoft.UI.Xaml.Data;
using VeloFile.App.ViewModels;

namespace VeloFile.App.Ui;

public sealed class FileListRowOpacityConverter : IValueConverter
{
    public double HiddenOpacity { get; set; } = 1.0;

    public double ProtectedOpacity { get; set; } = 1.0;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not FileListRowViewModel row)
        {
            return 1.0;
        }

        return FileListRowOpacityResourceSelector.GetOpacityResourceKey(row) switch
        {
            FileListRowOpacityResourceSelector.HiddenOpacityResourceKey => HiddenOpacity,
            FileListRowOpacityResourceSelector.ProtectedOpacityResourceKey => ProtectedOpacity,
            _ => 1.0
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException("File-list row opacity is derived from semantic row state.");
    }

}
