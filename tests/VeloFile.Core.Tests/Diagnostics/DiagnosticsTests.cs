using VeloFile.Core.Diagnostics;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Diagnostics;

[TestClass]
[TestCategory("Diagnostics")]
public sealed class DiagnosticsTests
{
    [TestMethod]
    public void Diagnostic_event_redacts_paths_and_prohibited_content()
    {
        var redactor = new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"));
        var sensitivePath = @"C:\Users\alice\Documents\secret-plan.txt";

        var diagnosticEvent = DiagnosticEvent.CreateFailure(
            eventId: "evt-1",
            sequenceNumber: 7,
            component: "Preview",
            operationKind: "preview",
            reasonCode: "access-denied",
            path: sensitivePath,
            redactor: redactor,
            timestampUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"));

        var json = DiagnosticJsonSerializer.Serialize(diagnosticEvent);

        StringAssert.Contains(json, "\"pathClassification\"");
        StringAssert.Contains(json, "\"pathFingerprint\"");
        Assert.IsFalse(json.Contains(sensitivePath, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("secret-plan", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("Documents", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("clipboard", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("search query", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("preview text", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Path_fingerprint_is_stable_per_installation_and_rotates_with_salt()
    {
        var first = new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"));
        var second = new PathRedactor(Convert.FromHexString("FFEEDDCCBBAA99887766554433221100"));
        var path = @"C:\Users\alice\Documents\secret-plan.txt";

        var firstFingerprint = first.Redact(path).PathFingerprint;
        var repeatFingerprint = first.Redact(path).PathFingerprint;
        var rotatedFingerprint = second.Redact(path).PathFingerprint;

        Assert.AreEqual(firstFingerprint, repeatFingerprint);
        Assert.AreNotEqual(firstFingerprint, rotatedFingerprint);
        Assert.IsFalse(firstFingerprint.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(firstFingerprint.Contains("secret-plan", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Path_redaction_classifies_extended_local_paths_without_treating_them_as_network()
    {
        var redactor = new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"));

        var localPath = redactor.Redact(@"\\?\C:\Users\alice\Documents\secret-plan.txt");
        var networkPath = redactor.Redact(@"\\?\UNC\server\share\secret-plan.txt");

        Assert.AreEqual("local", localPath.PathClassification);
        Assert.AreEqual("network", networkPath.PathClassification);
    }

    [TestMethod]
    public void Local_diagnostics_rotate_logs_and_retain_latest_markers()
    {
        using var workspace = TemporaryWorkspace.Create();
        var writer = new LocalDiagnosticLogStore(
            workspace.Root,
            new DiagnosticRetentionPolicy(
                MaxAge: TimeSpan.FromDays(30),
                MaxTotalBytes: 10_000,
                MaxFileBytes: 350,
                MaxCrashMarkers: 3));

        var redactor = new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"));

        for (var i = 0; i < 8; i++)
        {
            writer.Write(DiagnosticEvent.CreateFailure(
                eventId: $"evt-{i}",
                sequenceNumber: i,
                component: "Persistence",
                operationKind: "write",
                reasonCode: "simulated",
                path: $@"C:\Users\alice\Documents\secret-{i}.txt",
                redactor: redactor,
                timestampUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z").AddSeconds(i)));
        }

        for (var i = 0; i < 5; i++)
        {
            writer.RecordCrashMarker("startup", DateTimeOffset.Parse("2026-05-04T00:00:00Z").AddMinutes(i));
        }

        writer.RecordLastActionMarker("navigation", "Navigation", DateTimeOffset.Parse("2026-05-04T00:00:00Z"));
        writer.RecordLastActionMarker("navigation", "Navigation", DateTimeOffset.Parse("2026-05-04T00:01:00Z"));

        Assert.IsTrue(Directory.GetFiles(Path.Combine(workspace.Root, "logs"), "*.jsonl").Length > 1);
        Assert.AreEqual(3, Directory.GetFiles(Path.Combine(workspace.Root, "crash-markers"), "*.json").Length);
        Assert.AreEqual(1, Directory.GetFiles(Path.Combine(workspace.Root, "last-action-markers"), "navigation.json").Length);
        Assert.IsTrue(writer.HasRepeatedCrashMarkers("startup", threshold: 3));

        var allDiagnostics = string.Join(Environment.NewLine, Directory.GetFiles(workspace.Root, "*", SearchOption.AllDirectories).Select(File.ReadAllText));

        Assert.IsFalse(allDiagnostics.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(allDiagnostics.Contains("secret-", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TemporaryWorkspace Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "velofile-diagnostics-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TemporaryWorkspace(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
