using VeloFile.Core.Listing;

namespace VeloFile.Core.Preview;

public sealed class PreviewMetadataProvider
{
    public PreviewMetadata GetMetadata(ListedFileItem item)
    {
        return new PreviewMetadata(
            item.Name,
            item.Length,
            item.CreationTimeUtc,
            item.LastWriteTimeUtc,
            item.LastAccessTimeUtc,
            item.Attributes,
            TypeDescription(item),
            ExtensionClass(item));
    }

    private static string TypeDescription(ListedFileItem item)
    {
        if (item.Kind is FileSystemEntryKind.Directory)
        {
            return "Folder";
        }

        var extension = ExtensionClass(item);
        return extension is "none" ? "File" : $"{extension} file";
    }

    private static string ExtensionClass(ListedFileItem item)
    {
        var extension = Path.GetExtension(item.Name);
        return string.IsNullOrWhiteSpace(extension)
            ? "none"
            : extension.ToLowerInvariant();
    }
}

public sealed class MetadataOnlyPreviewProvider : IPreviewProvider
{
    public PreviewOperation Operation => PreviewOperation.MetadataFallback;

    public bool CanPreview(PreviewRequest request)
    {
        return true;
    }

    public ValueTask<PreviewProviderResult> PreviewAsync(
        PreviewRequest request,
        PreviewProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(PreviewProviderResult.Unsupported("unsupported"));
    }
}
