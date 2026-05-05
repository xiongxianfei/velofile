using System.Text;
using VeloFile.Core;
using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public sealed class WindowsPdfPreviewProvider : IPreviewProvider
{
    public PreviewOperation Operation => PreviewOperation.PdfFirstPageRender;

    public bool CanPreview(PreviewRequest request)
    {
        return string.Equals(Path.GetExtension(request.Item.Name), ".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async ValueTask<PreviewProviderResult> PreviewAsync(
        PreviewRequest request,
        PreviewProviderContext context,
        CancellationToken cancellationToken)
    {
        if (KnownLength(request) is > WindowsPreviewLimits.MaxPdfBytes)
        {
            return PreviewProviderResult.Unsupported("pdf-too-large");
        }

        try
        {
            await using var stream = new FileStream(
                request.Item.FullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 81920,
                useAsync: true);

            if (stream.Length > WindowsPreviewLimits.MaxPdfBytes)
            {
                return PreviewProviderResult.Unsupported("pdf-too-large");
            }

            var bytes = new byte[Math.Min(64 * 1024, (int)Math.Min(stream.Length, 64 * 1024))];
            var read = await stream.ReadAsync(bytes, cancellationToken).ConfigureAwait(false);
            return ProcessBytes(bytes, read);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return PreviewProviderResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
        catch
        {
            return PreviewProviderResult.Failed("pdf-corrupt");
        }
    }

    private static long? KnownLength(PreviewRequest request)
    {
        return request.Metadata.SizeBytes ?? request.Item.Length;
    }

    private static PreviewProviderResult ProcessBytes(byte[] bytes, int read)
    {
        if (read < 5
            || bytes[0] != (byte)'%'
            || bytes[1] != (byte)'P'
            || bytes[2] != (byte)'D'
            || bytes[3] != (byte)'F'
            || bytes[4] != (byte)'-')
        {
            return PreviewProviderResult.Failed("pdf-corrupt");
        }

        var sample = Encoding.ASCII.GetString(bytes, 0, read);
        if (!sample.Contains("/Type /Page", StringComparison.Ordinal))
        {
            return PreviewProviderResult.Failed("pdf-corrupt");
        }

        return PreviewProviderResult.Success(
            PreviewContent.PdfFirstPage("PDF Page 1 preview ready"));
    }
}
