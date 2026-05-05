using System.Text;
using VeloFile.Core.Preview;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace VeloFile.Windows.Preview;

public sealed class WindowsPdfPageRenderer : IPdfPageRenderer
{
    public async ValueTask<PdfPagePreviewArtifact> RenderPageAsync(
        string path,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        await using var fileStream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            useAsync: true);
        using var pdfStream = fileStream.AsRandomAccessStream();
        var document = await PdfDocument.LoadFromStreamAsync(pdfStream)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        if (pageNumber > document.PageCount)
        {
            throw new InvalidDataException("PDF page is outside the document page range.");
        }

        using var page = document.GetPage((uint)(pageNumber - 1));
        using var output = new InMemoryRandomAccessStream();
        await page.RenderToStreamAsync(output).AsTask(cancellationToken).ConfigureAwait(false);

        output.Seek(0);
        var decoder = await BitmapDecoder.CreateAsync(output)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        var pixelWidth = checked((int)decoder.PixelWidth);
        var pixelHeight = checked((int)decoder.PixelHeight);
        var bytes = await PreviewStreamHelpers.ReadAllBytesAsync(output, cancellationToken).ConfigureAwait(false);

        return new PdfPagePreviewArtifact(
            PageNumber: pageNumber,
            PageCount: checked((int)document.PageCount),
            PixelWidth: pixelWidth,
            PixelHeight: pixelHeight,
            EncodedFormat: InferEncodedFormat(bytes),
            EncodedBytes: bytes,
            SourceWasDownsampled: false);
    }

    private static string InferEncodedFormat(byte[] bytes)
    {
        if (bytes.Length >= 8
            && bytes[0] == 137
            && Encoding.ASCII.GetString(bytes, 1, 3) == "PNG")
        {
            return "png";
        }

        return "image";
    }
}
