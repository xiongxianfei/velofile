using Windows.Storage.Streams;

namespace VeloFile.Windows.Preview;

internal static class PreviewStreamHelpers
{
    public static async Task<byte[]> ReadAllBytesAsync(
        IRandomAccessStream stream,
        CancellationToken cancellationToken)
    {
        stream.Seek(0);
        var size = checked((int)stream.Size);
        using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)size).AsTask(cancellationToken).ConfigureAwait(false);
        var bytes = new byte[size];
        reader.ReadBytes(bytes);
        return bytes;
    }
}
