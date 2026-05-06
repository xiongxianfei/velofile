using VeloFile.Core.Terminal;
using VeloFile.Windows.Processes;
using VeloFile.Windows.Terminal;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.Terminal;

[TestClass]
[TestCategory("Terminal")]
public sealed class WindowsTerminalProcessLauncherTests
{
    [TestMethod]
    public async Task Terminal_launch_uses_process_start_data_without_shell_command_text()
    {
        var starter = new RecordingWindowsProcessStarter();
        var launcher = new WindowsTerminalProcessLauncher(starter);
        var target = new TerminalTarget(
            "wt",
            TerminalTargetKind.WindowsTerminal,
            "Windows Terminal",
            @"C:\Users\User\AppData\Local\Microsoft\WindowsApps\wt.exe");
        var workingDirectory = @"D:\scratch\project & whoami";

        var result = await launcher.LaunchAsync(new TerminalLaunchRequest(target, workingDirectory));

        Assert.AreEqual(TerminalLaunchStatus.Succeeded, result.Status);
        var request = starter.Requests.Single();
        Assert.AreEqual(target.ExecutablePath, request.FileName);
        Assert.AreEqual(workingDirectory, request.WorkingDirectory);
        Assert.IsFalse(request.UseShellExecute);
        Assert.IsNull(request.CommandText);
        CollectionAssert.Contains(request.ArgumentList.ToArray(), workingDirectory);
    }

    [TestMethod]
    public async Task Process_start_failure_returns_recoverable_terminal_failure()
    {
        var starter = new RecordingWindowsProcessStarter { Exception = new InvalidOperationException("boom") };
        var launcher = new WindowsTerminalProcessLauncher(starter);
        var target = new TerminalTarget("cmd", TerminalTargetKind.CommandPrompt, "Command Prompt", @"C:\Windows\System32\cmd.exe");

        var result = await launcher.LaunchAsync(new TerminalLaunchRequest(target, @"D:\scratch"));

        Assert.AreEqual(TerminalLaunchStatus.Failed, result.Status);
        Assert.AreEqual(TerminalLaunchReasonCodes.TerminalLaunchFailed, result.ReasonCode);
    }

    private sealed class RecordingWindowsProcessStarter : IWindowsProcessStarter
    {
        public List<WindowsProcessStartRequest> Requests { get; } = [];

        public Exception? Exception { get; set; }

        public void Start(WindowsProcessStartRequest request)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            Requests.Add(request);
        }
    }
}
