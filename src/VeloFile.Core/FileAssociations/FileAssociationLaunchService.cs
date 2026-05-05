using VeloFile.Core.Listing;

namespace VeloFile.Core.FileAssociations;

public sealed class FileAssociationLaunchService
{
    private readonly IFileAssociationLaunchAdapter _adapter;

    public FileAssociationLaunchService(IFileAssociationLaunchAdapter adapter)
    {
        _adapter = adapter;
    }

    public Task<FileAssociationLaunchResult> OpenAsync(ListedFileItem item, CancellationToken cancellationToken = default)
    {
        return LaunchAsync(FileAssociationLaunchKind.Open, item, cancellationToken);
    }

    public Task<FileAssociationLaunchResult> OpenWithAsync(ListedFileItem item, CancellationToken cancellationToken = default)
    {
        return LaunchAsync(FileAssociationLaunchKind.OpenWith, item, cancellationToken);
    }

    private async Task<FileAssociationLaunchResult> LaunchAsync(
        FileAssociationLaunchKind kind,
        ListedFileItem item,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);

        try
        {
            return await _adapter
                .LaunchAsync(new FileAssociationLaunchRequest(kind, item.FullPath, ModifySystemAssociations: false), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return FileAssociationLaunchResult.Failed(kind);
        }
    }
}
