using VeloFile.Core;

namespace VeloFile.Windows.Preview;

internal static class PreviewInputLengthGuard
{
    public static void EnsureWithinLimit(Stream stream, long maxBytes, Func<Exception> createTooLargeException)
    {
        long length;
        try
        {
            length = stream.Length;
        }
        catch (Exception ex) when (ex is NotSupportedException || ExpectedFileSystemExceptions.IsExpected(ex))
        {
            throw new PreviewInputLengthUnavailableException(ex);
        }

        if (length > maxBytes)
        {
            throw createTooLargeException();
        }
    }
}

public sealed class ImagePreviewInputTooLargeException : Exception
{
}

public sealed class PdfPreviewInputTooLargeException : Exception
{
}

public sealed class PreviewInputLengthUnavailableException : Exception
{
    public PreviewInputLengthUnavailableException(Exception innerException)
        : base("Preview input length could not be determined.", innerException)
    {
    }
}
