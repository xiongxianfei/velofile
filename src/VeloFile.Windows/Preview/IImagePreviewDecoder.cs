using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public interface IImagePreviewDecoder
{
    ValueTask<ImagePreviewArtifact> DecodeAsync(string path, CancellationToken cancellationToken);
}
