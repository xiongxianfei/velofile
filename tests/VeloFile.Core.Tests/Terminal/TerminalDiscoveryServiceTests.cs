using VeloFile.Core.Terminal;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Terminal;

[TestClass]
[TestCategory("Terminal")]
public sealed class TerminalDiscoveryServiceTests
{
    [TestMethod]
    public async Task Default_order_prefers_windows_terminal_pwsh_windows_powershell_and_cmd()
    {
        var source = new StaticTerminalTargetSource([
            Target(TerminalTargetKind.WslDistribution, "Ubuntu"),
            Target(TerminalTargetKind.GitBash, "Git Bash"),
            Target(TerminalTargetKind.CommandPrompt, "Command Prompt"),
            Target(TerminalTargetKind.PowerShell7, "PowerShell 7"),
            Target(TerminalTargetKind.WindowsTerminal, "Windows Terminal"),
            Target(TerminalTargetKind.WindowsPowerShell, "Windows PowerShell")
        ]);
        var service = new TerminalDiscoveryService(source);

        var result = await service.DiscoverAsync();

        CollectionAssert.AreEqual(
            new[]
            {
                TerminalTargetKind.WindowsTerminal,
                TerminalTargetKind.PowerShell7,
                TerminalTargetKind.WindowsPowerShell,
                TerminalTargetKind.CommandPrompt,
                TerminalTargetKind.GitBash,
                TerminalTargetKind.WslDistribution
            },
            result.Targets.Select(target => target.Kind).ToArray());
        Assert.AreEqual(TerminalTargetKind.WindowsTerminal, result.DefaultTarget?.Kind);
    }

    [TestMethod]
    public async Task Explicitly_selected_optional_target_can_become_default_without_reordering_contract()
    {
        var gitBash = Target(TerminalTargetKind.GitBash, "Git Bash");
        var windowsTerminal = Target(TerminalTargetKind.WindowsTerminal, "Windows Terminal");
        var service = new TerminalDiscoveryService(new StaticTerminalTargetSource([gitBash, windowsTerminal]));

        var result = await service.DiscoverAsync(preferredTargetId: gitBash.Id);

        Assert.AreEqual(gitBash.Id, result.DefaultTarget?.Id);
        CollectionAssert.AreEqual(
            new[] { TerminalTargetKind.WindowsTerminal, TerminalTargetKind.GitBash },
            result.Targets.Select(target => target.Kind).ToArray());
    }

    [TestMethod]
    public async Task Discovery_probe_failure_returns_empty_result_instead_of_blocking_launch()
    {
        var service = new TerminalDiscoveryService(new ThrowingTerminalTargetSource());

        var result = await service.DiscoverAsync();

        Assert.AreEqual(0, result.Targets.Count);
        Assert.IsNull(result.DefaultTarget);
    }

    private static TerminalTarget Target(TerminalTargetKind kind, string displayName)
    {
        return new TerminalTarget(
            Id: kind == TerminalTargetKind.WslDistribution ? "wsl:Ubuntu" : kind.ToString(),
            kind,
            displayName,
            ExecutablePath: kind == TerminalTargetKind.WslDistribution ? @"C:\Windows\System32\wsl.exe" : @$"C:\Tools\{kind}.exe",
            WslDistributionName: kind == TerminalTargetKind.WslDistribution ? "Ubuntu" : null);
    }

    private sealed class StaticTerminalTargetSource : ITerminalTargetSource
    {
        private readonly IReadOnlyList<TerminalTarget> _targets;

        public StaticTerminalTargetSource(IReadOnlyList<TerminalTarget> targets)
        {
            _targets = targets;
        }

        public ValueTask<IReadOnlyList<TerminalTarget>> GetAvailableTargetsAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_targets);
        }
    }

    private sealed class ThrowingTerminalTargetSource : ITerminalTargetSource
    {
        public ValueTask<IReadOnlyList<TerminalTarget>> GetAvailableTargetsAsync(CancellationToken cancellationToken = default)
        {
            throw new IOException("probe failed");
        }
    }
}
