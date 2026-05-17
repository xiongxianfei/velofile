using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace VeloFile.App.Ui;

public sealed class FileListIconGeometryConverter : IValueConverter
{
    private static readonly IReadOnlyDictionary<FileListIconKind, string> GeometryResourceKeys =
        new Dictionary<FileListIconKind, string>
        {
            [FileListIconKind.FileGeneric] = "VfIconGeometryFileGeneric",
            [FileListIconKind.Folder] = "VfIconGeometryFolder",
            [FileListIconKind.Pdf] = "VfIconGeometryPdf",
            [FileListIconKind.Image] = "VfIconGeometryImage",
            [FileListIconKind.Text] = "VfIconGeometryText",
            [FileListIconKind.Spreadsheet] = "VfIconGeometrySpreadsheet",
            [FileListIconKind.Executable] = "VfIconGeometryExecutable",
            [FileListIconKind.Markdown] = "VfIconGeometryMarkdown",
            [FileListIconKind.ThumbnailFallback] = "VfIconGeometryThumbnailFallback"
        };

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var iconKind = value is FileListIconKind kind ? kind : FileListIconKind.FileGeneric;
        var resourceKey = GeometryResourceKeys.TryGetValue(iconKind, out var mappedKey)
            ? mappedKey
            : GeometryResourceKeys[FileListIconKind.FileGeneric];

        if (Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true
            && resource is Geometry geometry)
        {
            return geometry;
        }

        return Application.Current?.Resources.TryGetValue(GeometryResourceKeys[FileListIconKind.FileGeneric], out var fallback) == true
            ? fallback as Geometry
            : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException("File-list icon geometry conversion is one-way.");
    }
}
