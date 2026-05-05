namespace VeloFile.Core.Terminal;

public enum TerminalTargetKind
{
    WindowsTerminal,
    PowerShell7,
    WindowsPowerShell,
    CommandPrompt,
    GitBash,
    WslDistribution
}

public sealed record TerminalTarget(
    string Id,
    TerminalTargetKind Kind,
    string DisplayName,
    string ExecutablePath,
    string? WslDistributionName = null);

public sealed record TerminalDiscoveryResult(
    IReadOnlyList<TerminalTarget> Targets,
    TerminalTarget? DefaultTarget);

public interface ITerminalTargetSource
{
    ValueTask<IReadOnlyList<TerminalTarget>> GetAvailableTargetsAsync(CancellationToken cancellationToken = default);
}

public interface IWorkingDirectoryProbe
{
    bool Exists(string path);
}

public sealed record TerminalLaunchRequest(
    TerminalTarget Target,
    string WorkingDirectory,
    string? CommandText = null);

public enum TerminalLaunchStatus
{
    Succeeded,
    TerminalUnavailable,
    WorkingDirectoryUnavailable,
    Failed
}

public sealed record TerminalLaunchResult(
    TerminalLaunchStatus Status,
    string? ReasonCode,
    TerminalTarget? Target)
{
    public static TerminalLaunchResult Succeeded(TerminalTarget target)
    {
        return new TerminalLaunchResult(TerminalLaunchStatus.Succeeded, ReasonCode: null, target);
    }

    public static TerminalLaunchResult TerminalUnavailable()
    {
        return new TerminalLaunchResult(TerminalLaunchStatus.TerminalUnavailable, "terminal-unavailable", Target: null);
    }

    public static TerminalLaunchResult WorkingDirectoryUnavailable()
    {
        return new TerminalLaunchResult(TerminalLaunchStatus.WorkingDirectoryUnavailable, "working-directory-unavailable", Target: null);
    }

    public static TerminalLaunchResult Failed(TerminalTarget? target, string reasonCode = "terminal-launch-failed")
    {
        return new TerminalLaunchResult(TerminalLaunchStatus.Failed, reasonCode, target);
    }
}

public interface ITerminalProcessLauncher
{
    Task<TerminalLaunchResult> LaunchAsync(
        TerminalLaunchRequest request,
        CancellationToken cancellationToken = default);
}
