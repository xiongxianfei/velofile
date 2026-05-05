using VeloFile.Core.Preview;

namespace VeloFile.Windows.Preview;

public interface IPdfPageRenderer
{
    ValueTask<PdfPagePreviewArtifact> RenderPageAsync(
        string path,
        int pageNumber,
        CancellationToken cancellationToken);
}
