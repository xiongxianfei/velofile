using VeloFile.Core.Diagnostics;
using VeloFile.Core.Terminal;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Terminal;

[TestClass]
[TestCategory("Terminal")]
public sealed class TerminalLaunchServiceTests
{
    [TestMethod]
    public async Task Open_terminal_uses_selected_target_and_structured_working_directory()
    {
        var target = Target(TerminalTargetKind.WindowsTerminal, "Windows Terminal");
        var launcher = new RecordingTerminalProcessLauncher();
        var service = new TerminalLaunchService(
            new TerminalDiscoveryService(new StaticTerminalTargetSource([target])),
            new StaticWorkingDirectoryProbe(exists: true),
            launcher);
        var pathWithMetacharacters = @"D:\scratch\project & whoami";

        var result = await service.LaunchDefaultAsync(pathWithMetacharacters);

        Assert.AreEqual(TerminalLaunchStatus.Succeeded, result.Status);
        Assert.AreEqual(target, launcher.Requests.Single().Target);
        Assert.AreEqual(pathWithMetacharacters, launcher.Requests.Single().WorkingDirectory);
        Assert.IsNull(launcher.Requests.Single().CommandText);
    }

    [TestMethod]
    public async Task Missing_terminal_returns_user_visible_failure_without_process_launch()
    {
        var launcher = new RecordingTerminalProcessLauncher();
        var service = new TerminalLaunchService(
            new TerminalDiscoveryService(new StaticTerminalTargetSource([])),
            new StaticWorkingDirectoryProbe(exists: true),
            launcher);

        var result = await service.LaunchDefaultAsync(@"D:\scratch");

        Assert.AreEqual(TerminalLaunchStatus.TerminalUnavailable, result.Status);
        Assert.AreEqual(TerminalLaunchReasonCodes.TerminalUnavailable, result.ReasonCode);
        Assert.AreEqual(0, launcher.Requests.Count);
    }

    [TestMethod]
    public async Task Inaccessible_working_directory_returns_user_visible_failure_without_process_launch()
    {
        var target = Target(TerminalTargetKind.CommandPrompt, "Command Prompt");
        var launcher = new RecordingTerminalProcessLauncher();
        var service = new TerminalLaunchService(
            new TerminalDiscoveryService(new StaticTerminalTargetSource([target])),
            new StaticWorkingDirectoryProbe(exists: false),
            launcher);

        var result = await service.LaunchDefaultAsync(@"D:\missing");

        Assert.AreEqual(TerminalLaunchStatus.WorkingDirectoryUnavailable, result.Status);
        Assert.AreEqual(TerminalLaunchReasonCodes.WorkingDirectoryUnavailable, result.ReasonCode);
        Assert.AreEqual(0, launcher.Requests.Count);
    }

    [TestMethod]
    public async Task Terminal_launch_diagnostics_record_target_identity_without_command_text_or_path()
    {
        var diagnostics = new CollectingDiagnosticSink();
        var target = Target(TerminalTargetKind.WindowsTerminal, "Windows Terminal");
        var service = new TerminalLaunchService(
            new TerminalDiscoveryService(new StaticTerminalTargetSource([target])),
            new StaticWorkingDirectoryProbe(exists: true),
            new RecordingTerminalProcessLauncher(),
            diagnostics: diagnostics,
            utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z"));
        var workingDirectory = @"D:\scratch\project & whoami";

        await service.LaunchDefaultAsync(workingDirectory);

        var diagnostic = diagnostics.Events.Single();
        Assert.AreEqual("terminal.launch", diagnostic.EventType);
        Assert.AreEqual("terminal-launch", diagnostic.OperationKind);
        Assert.AreEqual("windows-terminal", diagnostic.TerminalTargetKind);
        var json = DiagnosticJsonSerializer.Serialize(diagnostic);
        StringAssert.Contains(json, "windows-terminal");
        Assert.IsFalse(json.Contains(workingDirectory, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("whoami", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Terminal_failure_diagnostics_preserve_reason_codes_without_command_text_or_path()
    {
        var missingDiagnostics = new CollectingDiagnosticSink();
        await AssertFailureDiagnosticAsync(
            new TerminalLaunchService(
                new TerminalDiscoveryService(new StaticTerminalTargetSource([])),
                new StaticWorkingDirectoryProbe(exists: true),
                new RecordingTerminalProcessLauncher(),
                diagnostics: missingDiagnostics,
                utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z")),
            missingDiagnostics,
            TerminalLaunchReasonCodes.TerminalUnavailable);

        var workingDirectoryDiagnostics = new CollectingDiagnosticSink();
        await AssertFailureDiagnosticAsync(
            new TerminalLaunchService(
                new TerminalDiscoveryService(new StaticTerminalTargetSource([Target(TerminalTargetKind.CommandPrompt, "Command Prompt")])),
                new StaticWorkingDirectoryProbe(exists: false),
                new RecordingTerminalProcessLauncher(),
                diagnostics: workingDirectoryDiagnostics,
                utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z")),
            workingDirectoryDiagnostics,
            TerminalLaunchReasonCodes.WorkingDirectoryUnavailable);

        var processFailureDiagnostics = new CollectingDiagnosticSink();
        await AssertFailureDiagnosticAsync(
            new TerminalLaunchService(
                new TerminalDiscoveryService(new StaticTerminalTargetSource([Target(TerminalTargetKind.PowerShell7, "PowerShell 7")])),
                new StaticWorkingDirectoryProbe(exists: true),
                new RecordingTerminalProcessLauncher(request => TerminalLaunchResult.Failed(request.Target)),
                diagnostics: processFailureDiagnostics,
                utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z")),
            processFailureDiagnostics,
            TerminalLaunchReasonCodes.TerminalLaunchFailed);
    }

    private static TerminalTarget Target(TerminalTargetKind kind, string displayName)
    {
        return new TerminalTarget(kind.ToString(), kind, displayName, @$"C:\Tools\{kind}.exe");
    }

    private static async Task AssertFailureDiagnosticAsync(
        TerminalLaunchService service,
        CollectingDiagnosticSink diagnostics,
        string expectedReasonCode)
    {
        var workingDirectory = @"C:\Users\alice\secret-project & pwsh -NoProfile -File C:\Users\alice\run.ps1";

        var result = await service.LaunchDefaultAsync(workingDirectory);

        Assert.AreEqual(expectedReasonCode, result.ReasonCode);
        var json = DiagnosticJsonSerializer.Serialize(diagnostics.Events.Single());
        Assert.AreEqual(expectedReasonCode, ReadStringField(json, "reasonCode"));
        Assert.IsFalse(json.Contains(workingDirectory, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("secret-project", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("pwsh -NoProfile", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("run.ps1", StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadStringField(string json, string fieldName)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return document.RootElement.GetProperty(fieldName).GetString();
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

    private sealed class StaticWorkingDirectoryProbe : IWorkingDirectoryProbe
    {
        private readonly bool _exists;

        public StaticWorkingDirectoryProbe(bool exists)
        {
            _exists = exists;
        }

        public bool Exists(string path)
        {
            return _exists;
        }
    }

    private sealed class RecordingTerminalProcessLauncher : ITerminalProcessLauncher
    {
        private readonly Func<TerminalLaunchRequest, TerminalLaunchResult>? _resultFactory;

        public RecordingTerminalProcessLauncher(Func<TerminalLaunchRequest, TerminalLaunchResult>? resultFactory = null)
        {
            _resultFactory = resultFactory;
        }

        public List<TerminalLaunchRequest> Requests { get; } = [];

        public Task<TerminalLaunchResult> LaunchAsync(TerminalLaunchRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(_resultFactory?.Invoke(request) ?? TerminalLaunchResult.Succeeded(request.Target));
        }
    }
}
