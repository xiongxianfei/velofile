using VeloFile.Core.FileAssociations;
using VeloFile.Windows.Processes;

namespace VeloFile.Windows.ShellExecute;

public sealed class WindowsFileAssociationLauncher : IFileAssociationLaunchAdapter
{
    private readonly IWindowsProcessStarter _processStarter;

    public WindowsFileAssociationLauncher(IWindowsProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? WindowsProcessStarter.Instance;
    }

    public Task<FileAssociationLaunchResult> LaunchAsync(
        FileAssociationLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _processStarter.Start(new WindowsProcessStartRequest(
                request.Path,
                WorkingDirectory: null,
                UseShellExecute: true,
                ArgumentList: [],
                Verb: request.Kind is FileAssociationLaunchKind.OpenWith ? "openas" : null,
                CommandText: null,
                ModifySystemAssociations: request.ModifySystemAssociations));
            return Task.FromResult(FileAssociationLaunchResult.Succeeded(request.Kind));
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(FileAssociationLaunchResult.Failed(request.Kind));
        }
    }
}
