using System.Text;
using VeloFile.Core;
using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public sealed class WindowsTextPreviewProvider : IPreviewProvider
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bat",
        ".cmd",
        ".config",
        ".cs",
        ".css",
        ".csv",
        ".editorconfig",
        ".html",
        ".ini",
        ".js",
        ".json",
        ".log",
        ".md",
        ".ps1",
        ".py",
        ".sh",
        ".sln",
        ".ts",
        ".txt",
        ".xaml",
        ".xml",
        ".yaml",
        ".yml"
    };

    public PreviewOperation Operation => PreviewOperation.TextReadAndEncodingDetection;

    public bool CanPreview(PreviewRequest request)
    {
        return SupportedExtensions.Contains(Path.GetExtension(request.Item.Name));
    }

    public async ValueTask<PreviewProviderResult> PreviewAsync(
        PreviewRequest request,
        PreviewProviderContext context,
        CancellationToken cancellationToken)
    {
        if (KnownLength(request) is > WindowsPreviewLimits.MaxTextBytes)
        {
            return PreviewProviderResult.Unsupported("text-too-large");
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

            if (stream.Length > WindowsPreviewLimits.MaxTextBytes)
            {
                return PreviewProviderResult.Unsupported("text-too-large");
            }

            var readLimit = WindowsPreviewLimits.MaxTextPreviewBytes + 4;
            var bytes = new byte[Math.Min(readLimit, (int)Math.Min(stream.Length, readLimit))];
            var read = await stream.ReadAsync(bytes, cancellationToken).ConfigureAwait(false);
            return ProcessBytes(bytes, read, stream.Length);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return PreviewProviderResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
    }

    private static long? KnownLength(PreviewRequest request)
    {
        return request.Metadata.SizeBytes ?? request.Item.Length;
    }

    private static bool LooksBinary(ReadOnlySpan<byte> bytes)
    {
        var controlCount = 0;
        foreach (var value in bytes)
        {
            if (value == 0)
            {
                return true;
            }

            if (value < 0x20 && value is not (byte)'\r' and not (byte)'\n' and not (byte)'\t' and not 0x1b)
            {
                controlCount++;
            }
        }

        return bytes.Length > 0 && controlCount > bytes.Length / 20;
    }

    private static PreviewProviderResult ProcessBytes(byte[] bytes, int read, long streamLength)
    {
        var span = bytes.AsSpan(0, read);
        if (LooksBinary(span))
        {
            return PreviewProviderResult.Unsupported("text-binary");
        }

        var decodeBytes = span.Length > WindowsPreviewLimits.MaxTextPreviewBytes
            ? span[..WindowsPreviewLimits.MaxTextPreviewBytes]
            : span;
        var truncated = streamLength > decodeBytes.Length || span.Length > WindowsPreviewLimits.MaxTextPreviewBytes;

        try
        {
            var text = DecodeText(decodeBytes);
            return PreviewProviderResult.Success(PreviewContent.Text(text, truncated));
        }
        catch (DecoderFallbackException)
        {
            return PreviewProviderResult.Unsupported("text-unsupported-encoding");
        }
    }

    private static string DecodeText(ReadOnlySpan<byte> bytes)
    {
        if (StartsWith(bytes, 0xef, 0xbb, 0xbf))
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                .GetString(bytes[3..]);
        }

        if (StartsWith(bytes, 0xff, 0xfe))
        {
            return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true)
                .GetString(bytes[2..]);
        }

        if (StartsWith(bytes, 0xfe, 0xff))
        {
            return new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true)
                .GetString(bytes[2..]);
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
            .GetString(bytes);
    }

    private static bool StartsWith(ReadOnlySpan<byte> bytes, params byte[] prefix)
    {
        return bytes.Length >= prefix.Length && bytes[..prefix.Length].SequenceEqual(prefix);
    }
}
