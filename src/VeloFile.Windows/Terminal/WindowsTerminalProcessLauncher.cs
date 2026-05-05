using VeloFile.Core.Terminal;
using VeloFile.Windows.Processes;

namespace VeloFile.Windows.Terminal;

public sealed class WindowsTerminalProcessLauncher : ITerminalProcessLauncher
{
    private readonly IWindowsProcessStarter _processStarter;

    public WindowsTerminalProcessLauncher(IWindowsProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? WindowsProcessStarter.Instance;
    }

    public Task<TerminalLaunchResult> LaunchAsync(
        TerminalLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _processStarter.Start(ToProcessStartRequest(request));
            return Task.FromResult(TerminalLaunchResult.Succeeded(request.Target));
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(TerminalLaunchResult.Failed(request.Target));
        }
    }

    private static WindowsProcessStartRequest ToProcessStartRequest(TerminalLaunchRequest request)
    {
        var arguments = new List<string>();
        switch (request.Target.Kind)
        {
            case TerminalTargetKind.WindowsTerminal:
                arguments.Add("-d");
                arguments.Add(request.WorkingDirectory);
                break;
            case TerminalTargetKind.WslDistribution:
                if (!string.IsNullOrWhiteSpace(request.Target.WslDistributionName))
                {
                    arguments.Add("--distribution");
                    arguments.Add(request.Target.WslDistributionName);
                }

                arguments.Add("--cd");
                arguments.Add(request.WorkingDirectory);
                break;
        }

        return new WindowsProcessStartRequest(
            FileName: request.Target.ExecutablePath,
            WorkingDirectory: request.WorkingDirectory,
            UseShellExecute: false,
            ArgumentList: arguments,
            Verb: null,
            CommandText: null,
            ModifySystemAssociations: false);
    }
}
