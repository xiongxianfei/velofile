using VeloFile.Core;
using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public sealed class WindowsPdfPreviewProvider : IPagedPreviewProvider
{
    private readonly IPdfPageRenderer _renderer;

    public WindowsPdfPreviewProvider()
        : this(new WindowsPdfPageRenderer())
    {
    }

    public WindowsPdfPreviewProvider(IPdfPageRenderer renderer)
    {
        _renderer = renderer;
    }

    public PreviewOperation Operation => PreviewOperation.PdfFirstPageRender;

    public bool CanPreview(PreviewRequest request)
    {
        return string.Equals(Path.GetExtension(request.Item.Name), ".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public ValueTask<PreviewProviderResult> PreviewAsync(
        PreviewRequest request,
        PreviewProviderContext context,
        CancellationToken cancellationToken)
    {
        return PreviewPageAsync(request, pageNumber: 1, context, cancellationToken);
    }

    public async ValueTask<PreviewProviderResult> PreviewPageAsync(
        PreviewRequest request,
        int pageNumber,
        PreviewProviderContext context,
        CancellationToken cancellationToken)
    {
        if (KnownLength(request) is > WindowsPreviewLimits.MaxPdfBytes)
        {
            return PreviewProviderResult.Unsupported("pdf-too-large");
        }

        try
        {
            var artifact = await _renderer.RenderPageAsync(request.Item.FullPath, pageNumber, cancellationToken).ConfigureAwait(false);
            if (artifact.EncodedBytes.Length == 0)
            {
                return PreviewProviderResult.Failed("pdf-corrupt");
            }

            return PreviewProviderResult.Success(PreviewContent.PdfPage(artifact));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (PdfPreviewInputTooLargeException)
        {
            return PreviewProviderResult.Unsupported("pdf-too-large");
        }
        catch (PreviewInputLengthUnavailableException)
        {
            return PreviewProviderResult.Unsupported("preview-input-length-unavailable");
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return PreviewProviderResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
        catch (ArgumentOutOfRangeException)
        {
            return PreviewProviderResult.Failed("pdf-page-unavailable");
        }
        catch (InvalidDataException)
        {
            return PreviewProviderResult.Failed("pdf-corrupt");
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
}
