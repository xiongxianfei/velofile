using VeloFile.Core;
using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public sealed class WindowsImagePreviewProvider : IPreviewProvider
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".gif",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private readonly IImagePreviewDecoder _decoder;

    public WindowsImagePreviewProvider()
        : this(new WindowsImagePreviewDecoder())
    {
    }

    public WindowsImagePreviewProvider(IImagePreviewDecoder decoder)
    {
        _decoder = decoder;
    }

    public PreviewOperation Operation => PreviewOperation.ImageDecode;

    public bool CanPreview(PreviewRequest request)
    {
        return SupportedExtensions.Contains(Path.GetExtension(request.Item.Name));
    }

    public async ValueTask<PreviewProviderResult> PreviewAsync(
        PreviewRequest request,
        PreviewProviderContext context,
        CancellationToken cancellationToken)
    {
        if (KnownLength(request) is > WindowsPreviewLimits.MaxImageBytes)
        {
            return PreviewProviderResult.Unsupported("image-too-large");
        }

        try
        {
            var artifact = await _decoder.DecodeAsync(request.Item.FullPath, cancellationToken).ConfigureAwait(false);
            if (artifact.PixelWidth > WindowsPreviewLimits.MaxImageDimensionPixels
                || artifact.PixelHeight > WindowsPreviewLimits.MaxImageDimensionPixels)
            {
                return PreviewProviderResult.Unsupported("image-dimensions-too-large");
            }

            if (artifact.EncodedBytes.Length == 0)
            {
                return PreviewProviderResult.Failed("decode-error");
            }

            return PreviewProviderResult.Success(PreviewContent.Image(artifact));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ImagePreviewInputTooLargeException)
        {
            return PreviewProviderResult.Unsupported("image-too-large");
        }
        catch (PreviewInputLengthUnavailableException)
        {
            return PreviewProviderResult.Unsupported("preview-input-length-unavailable");
        }
        catch (ImagePreviewDimensionsTooLargeException)
        {
            return PreviewProviderResult.Unsupported("image-dimensions-too-large");
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return PreviewProviderResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
        catch
        {
            return PreviewProviderResult.Failed("decode-error");
        }
    }

    private static long? KnownLength(PreviewRequest request)
    {
        return request.Metadata.SizeBytes ?? request.Item.Length;
    }
}
