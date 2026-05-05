using System.Collections.Concurrent;
using VeloFile.Core;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace VeloFile.Windows.Preview;

public sealed class WindowsThumbnailProvider : IThumbnailProvider
{
    private readonly ConcurrentDictionary<string, ThumbnailArtifact> _genericIconCache = new(StringComparer.OrdinalIgnoreCase);

    public int CachedGenericIconCount => _genericIconCache.Count;

    public async ValueTask<ThumbnailProviderResult> GenerateAsync(
        ListedFileItem item,
        ThumbnailProviderContext context,
        CancellationToken cancellationToken)
    {
        if (item.Kind is FileSystemEntryKind.Other)
        {
            return Generic(item, "generic-icon");
        }

        try
        {
            using var stream = item.Kind is FileSystemEntryKind.Directory
                ? await ReadFolderThumbnailAsync(item.FullPath, cancellationToken).ConfigureAwait(false)
                : await ReadFileThumbnailAsync(item.FullPath, cancellationToken).ConfigureAwait(false);

            if (stream is null || stream.Size == 0)
            {
                return Generic(item, "generic-icon");
            }

            var bytes = await PreviewStreamHelpers.ReadAllBytesAsync(stream, cancellationToken).ConfigureAwait(false);
            return bytes.Length == 0
                ? Generic(item, "generic-icon")
                : ThumbnailProviderResult.Success(new ThumbnailArtifact(
                    GenericDisplayText(item),
                    EncodedFormat: "image",
                    EncodedBytes: bytes,
                    IsGenericIcon: false));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return Generic(item, ExpectedFileSystemExceptions.ReasonCode(ex));
        }
        catch
        {
            return Generic(item, "generic-icon");
        }
    }

    private static async Task<StorageItemThumbnail?> ReadFileThumbnailAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var file = await StorageFile.GetFileFromPathAsync(path)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        return await file.GetThumbnailAsync(
                ThumbnailMode.ListView,
                requestedSize: 64,
                ThumbnailOptions.UseCurrentScale)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<StorageItemThumbnail?> ReadFolderThumbnailAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var folder = await StorageFolder.GetFolderFromPathAsync(path)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        return await folder.GetThumbnailAsync(
                ThumbnailMode.ListView,
                requestedSize: 64,
                ThumbnailOptions.UseCurrentScale)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
    }

    private ThumbnailProviderResult Generic(ListedFileItem item, string reasonCode)
    {
        return ThumbnailProviderResult.GenericIcon(
            _genericIconCache.GetOrAdd(CacheKey(item), _ => ThumbnailArtifact.GenericIcon(GenericDisplayText(item))),
            reasonCode);
    }

    private static string CacheKey(ListedFileItem item)
    {
        return item.Kind is FileSystemEntryKind.Directory
            ? "directory"
            : "file:" + Path.GetExtension(item.Name).ToUpperInvariant();
    }

    private static string GenericDisplayText(ListedFileItem item)
    {
        if (item.Kind is FileSystemEntryKind.Directory)
        {
            return "DIR";
        }

        var extension = Path.GetExtension(item.Name).TrimStart('.');
        return string.IsNullOrWhiteSpace(extension)
            ? "FILE"
            : extension[..Math.Min(extension.Length, 4)].ToUpperInvariant();
    }
}
