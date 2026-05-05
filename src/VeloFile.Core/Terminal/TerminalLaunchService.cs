using VeloFile.Core.Diagnostics;

namespace VeloFile.Core.Terminal;

public sealed class TerminalLaunchService
{
    private readonly TerminalDiscoveryService _discoveryService;
    private readonly IWorkingDirectoryProbe _workingDirectoryProbe;
    private readonly ITerminalProcessLauncher _processLauncher;
    private readonly Func<string?> _preferredTargetId;
    private readonly IDiagnosticSink? _diagnostics;
    private readonly Func<DateTimeOffset> _utcNow;

    public TerminalLaunchService(
        TerminalDiscoveryService discoveryService,
        IWorkingDirectoryProbe workingDirectoryProbe,
        ITerminalProcessLauncher processLauncher,
        Func<string?>? preferredTargetId = null,
        IDiagnosticSink? diagnostics = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _discoveryService = discoveryService;
        _workingDirectoryProbe = workingDirectoryProbe;
        _processLauncher = processLauncher;
        _preferredTargetId = preferredTargetId ?? (() => null);
        _diagnostics = diagnostics;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<TerminalLaunchResult> LaunchDefaultAsync(
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory) || !_workingDirectoryProbe.Exists(workingDirectory))
        {
            var unavailable = TerminalLaunchResult.WorkingDirectoryUnavailable();
            RecordLaunch(null, unavailable);
            return unavailable;
        }

        var discovery = await _discoveryService.DiscoverAsync(_preferredTargetId(), cancellationToken).ConfigureAwait(false);
        if (discovery.DefaultTarget is null)
        {
            var unavailable = TerminalLaunchResult.TerminalUnavailable();
            RecordLaunch(null, unavailable);
            return unavailable;
        }

        return await LaunchTargetAsync(discovery.DefaultTarget, workingDirectory, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<TerminalDiscoveryResult> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        return _discoveryService.DiscoverAsync(_preferredTargetId(), cancellationToken);
    }

    public async Task<TerminalLaunchResult> LaunchAsync(
        string targetId,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory) || !_workingDirectoryProbe.Exists(workingDirectory))
        {
            var unavailable = TerminalLaunchResult.WorkingDirectoryUnavailable();
            RecordLaunch(null, unavailable);
            return unavailable;
        }

        var discovery = await _discoveryService.DiscoverAsync(targetId, cancellationToken).ConfigureAwait(false);
        var target = discovery.Targets.FirstOrDefault(item => string.Equals(item.Id, targetId, StringComparison.OrdinalIgnoreCase));
        if (target is null)
        {
            var unavailable = TerminalLaunchResult.TerminalUnavailable();
            RecordLaunch(null, unavailable);
            return unavailable;
        }

        return await LaunchTargetAsync(target, workingDirectory, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TerminalLaunchResult> LaunchTargetAsync(
        TerminalTarget target,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _processLauncher
                .LaunchAsync(new TerminalLaunchRequest(target, workingDirectory.Trim(), CommandText: null), cancellationToken)
                .ConfigureAwait(false);
            RecordLaunch(target, result);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            var failed = TerminalLaunchResult.Failed(target);
            RecordLaunch(target, failed);
            return failed;
        }
    }

    private void RecordLaunch(TerminalTarget? target, TerminalLaunchResult result)
    {
        if (_diagnostics is null)
        {
            return;
        }

        try
        {
            _diagnostics.Write(new DiagnosticEvent
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = "terminal.launch",
                UtcTimestamp = _utcNow(),
                SequenceNumber = 0,
                Severity = result.Status is TerminalLaunchStatus.Succeeded ? "info" : "warning",
                Component = "terminal",
                OperationKind = "terminal-launch",
                ResultState = ToDiagnosticResultState(result.Status),
                ReasonCode = result.ReasonCode,
                TerminalTargetKind = target is null ? null : ToDiagnosticTargetKind(target.Kind)
            });
        }
        catch
        {
            // Diagnostics are best-effort and must not break explicit launch commands.
        }
    }

    private static string ToDiagnosticResultState(TerminalLaunchStatus status)
    {
        return status is TerminalLaunchStatus.Succeeded ? "succeeded" : "failed";
    }

    private static string ToDiagnosticTargetKind(TerminalTargetKind kind)
    {
        return kind switch
        {
            TerminalTargetKind.WindowsTerminal => "windows-terminal",
            TerminalTargetKind.PowerShell7 => "powershell-7",
            TerminalTargetKind.WindowsPowerShell => "windows-powershell",
            TerminalTargetKind.CommandPrompt => "command-prompt",
            TerminalTargetKind.GitBash => "git-bash",
            TerminalTargetKind.WslDistribution => "wsl",
            _ => "terminal"
        };
    }
}
