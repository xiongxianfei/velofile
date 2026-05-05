using VeloFile.Core.Preview;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace VeloFile.Windows.Preview;

public sealed class WindowsImagePreviewDecoder : IImagePreviewDecoder
{
    public async ValueTask<ImagePreviewArtifact> DecodeAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var fileStream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            useAsync: true);
        using var imageStream = fileStream.AsRandomAccessStream();

        var decoder = await BitmapDecoder.CreateAsync(imageStream)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        var pixelWidth = checked((int)decoder.PixelWidth);
        var pixelHeight = checked((int)decoder.PixelHeight);
        if (pixelWidth > WindowsPreviewLimits.MaxImageDimensionPixels
            || pixelHeight > WindowsPreviewLimits.MaxImageDimensionPixels)
        {
            throw new ImagePreviewDimensionsTooLargeException();
        }

        using var bitmap = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        using var output = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        encoder.SetSoftwareBitmap(bitmap);
        await encoder.FlushAsync().AsTask(cancellationToken).ConfigureAwait(false);

        var bytes = await PreviewStreamHelpers.ReadAllBytesAsync(output, cancellationToken).ConfigureAwait(false);
        return new ImagePreviewArtifact(
            PixelWidth: pixelWidth,
            PixelHeight: pixelHeight,
            EncodedFormat: "png",
            EncodedBytes: bytes,
            SourceWasDownsampled: false);
    }
}

public sealed class ImagePreviewDimensionsTooLargeException : Exception
{
}
