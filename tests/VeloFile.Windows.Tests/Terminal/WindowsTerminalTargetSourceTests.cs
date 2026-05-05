using VeloFile.Core.Terminal;
using VeloFile.Windows.Terminal;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.Terminal;

[TestClass]
[TestCategory("Terminal")]
public sealed class WindowsTerminalTargetSourceTests
{
    [TestMethod]
    public async Task Discovers_supported_windows_terminal_targets_without_work_in_constructor()
    {
        var probe = new RecordingWindowsTerminalProbe
        {
            WindowsTerminalPath = @"C:\Users\User\AppData\Local\Microsoft\WindowsApps\wt.exe",
            PowerShell7Path = @"C:\Program Files\PowerShell\7\pwsh.exe",
            WindowsPowerShellPath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
            CommandPromptPath = @"C:\Windows\System32\cmd.exe",
            GitBashPath = @"C:\Program Files\Git\git-bash.exe",
            WslPath = @"C:\Windows\System32\wsl.exe",
            WslDistributions = ["Ubuntu", "Debian"]
        };

        var source = new WindowsTerminalTargetSource(probe);

        Assert.AreEqual(0, probe.FindExecutableCallCount);

        var targets = await source.GetAvailableTargetsAsync();

        CollectionAssert.AreEquivalent(
            new[]
            {
                TerminalTargetKind.WindowsTerminal,
                TerminalTargetKind.PowerShell7,
                TerminalTargetKind.WindowsPowerShell,
                TerminalTargetKind.CommandPrompt,
                TerminalTargetKind.GitBash,
                TerminalTargetKind.WslDistribution,
                TerminalTargetKind.WslDistribution
            },
            targets.Select(target => target.Kind).ToArray());
        Assert.AreEqual(6, probe.FindExecutableCallCount);
        Assert.IsTrue(targets.Any(target => target.Id == "wsl:Ubuntu"));
        Assert.IsTrue(targets.Any(target => target.Id == "wsl:Debian"));
    }

    private sealed class RecordingWindowsTerminalProbe : IWindowsTerminalProbe
    {
        public string? WindowsTerminalPath { get; set; }

        public string? PowerShell7Path { get; set; }

        public string? WindowsPowerShellPath { get; set; }

        public string? CommandPromptPath { get; set; }

        public string? GitBashPath { get; set; }

        public string? WslPath { get; set; }

        public IReadOnlyList<string> WslDistributions { get; set; } = [];

        public int FindExecutableCallCount { get; private set; }

        public string? FindExecutable(TerminalTargetKind kind)
        {
            FindExecutableCallCount++;
            return kind switch
            {
                TerminalTargetKind.WindowsTerminal => WindowsTerminalPath,
                TerminalTargetKind.PowerShell7 => PowerShell7Path,
                TerminalTargetKind.WindowsPowerShell => WindowsPowerShellPath,
                TerminalTargetKind.CommandPrompt => CommandPromptPath,
                TerminalTargetKind.GitBash => GitBashPath,
                TerminalTargetKind.WslDistribution => WslPath,
                _ => null
            };
        }

        public IReadOnlyList<string> GetWslDistributions(CancellationToken cancellationToken = default)
        {
            return WslDistributions;
        }
    }
}
