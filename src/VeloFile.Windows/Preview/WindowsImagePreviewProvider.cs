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
            await using var stream = new FileStream(
                request.Item.FullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 4096,
                useAsync: true);

            if (stream.Length > WindowsPreviewLimits.MaxImageBytes)
            {
                return PreviewProviderResult.Unsupported("image-too-large");
            }

            var header = new byte[Math.Min(64 * 1024, (int)Math.Min(stream.Length, 64 * 1024))];
            var read = await stream.ReadAsync(header, cancellationToken).ConfigureAwait(false);
            var dimensions = TryReadDimensions(header.AsSpan(0, read));
            if (dimensions is null)
            {
                return PreviewProviderResult.Failed("decode-error");
            }

            var (width, height) = dimensions.Value;
            if (width > WindowsPreviewLimits.MaxImageDimensionPixels
                || height > WindowsPreviewLimits.MaxImageDimensionPixels)
            {
                return PreviewProviderResult.Unsupported("image-dimensions-too-large");
            }

            return PreviewProviderResult.Success(PreviewContent.Image(width, height));
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
            return PreviewProviderResult.Failed("decode-error");
        }
    }

    private static long? KnownLength(PreviewRequest request)
    {
        return request.Metadata.SizeBytes ?? request.Item.Length;
    }

    private static (int Width, int Height)? TryReadDimensions(ReadOnlySpan<byte> header)
    {
        return TryReadPngDimensions(header)
            ?? TryReadGifDimensions(header)
            ?? TryReadBmpDimensions(header)
            ?? TryReadJpegDimensions(header);
    }

    private static (int Width, int Height)? TryReadPngDimensions(ReadOnlySpan<byte> header)
    {
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        if (header.Length < 24 || !header[..8].SequenceEqual(signature))
        {
            return null;
        }

        return (ReadBigEndianInt32(header[16..20]), ReadBigEndianInt32(header[20..24]));
    }

    private static (int Width, int Height)? TryReadGifDimensions(ReadOnlySpan<byte> header)
    {
        if (header.Length < 10
            || !(header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)))
        {
            return null;
        }

        return (ReadLittleEndianUInt16(header[6..8]), ReadLittleEndianUInt16(header[8..10]));
    }

    private static (int Width, int Height)? TryReadBmpDimensions(ReadOnlySpan<byte> header)
    {
        if (header.Length < 26 || header[0] != (byte)'B' || header[1] != (byte)'M')
        {
            return null;
        }

        return (ReadLittleEndianInt32(header[18..22]), Math.Abs(ReadLittleEndianInt32(header[22..26])));
    }

    private static (int Width, int Height)? TryReadJpegDimensions(ReadOnlySpan<byte> header)
    {
        if (header.Length < 4 || header[0] != 0xff || header[1] != 0xd8)
        {
            return null;
        }

        var offset = 2;
        while (offset + 8 < header.Length)
        {
            if (header[offset] != 0xff)
            {
                return null;
            }

            var marker = header[offset + 1];
            offset += 2;
            if (marker is 0xd8 or 0xd9)
            {
                continue;
            }

            if (offset + 2 > header.Length)
            {
                return null;
            }

            var segmentLength = ReadBigEndianUInt16(header[offset..(offset + 2)]);
            if (segmentLength < 2 || offset + segmentLength > header.Length)
            {
                return null;
            }

            if (marker is >= 0xc0 and <= 0xc3)
            {
                var height = ReadBigEndianUInt16(header[(offset + 3)..(offset + 5)]);
                var width = ReadBigEndianUInt16(header[(offset + 5)..(offset + 7)]);
                return (width, height);
            }

            offset += segmentLength;
        }

        return null;
    }

    private static int ReadBigEndianInt32(ReadOnlySpan<byte> bytes)
    {
        return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
    }

    private static int ReadBigEndianUInt16(ReadOnlySpan<byte> bytes)
    {
        return (bytes[0] << 8) | bytes[1];
    }

    private static int ReadLittleEndianInt32(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    private static int ReadLittleEndianUInt16(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] | (bytes[1] << 8);
    }
}
