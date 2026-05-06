using VeloFile.Core.Diagnostics;
using VeloFile.Core.Terminal;

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
            component: "preview",
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
    public void Diagnostic_serializer_redacts_dangerous_values_in_every_serialized_string_field()
    {
        var dangerousValues = new[]
        {
            "secret-plan",
            "payroll",
            "client-a",
            "id_rsa",
            ".env",
            ".npmrc",
            ".gitconfig",
            "private-report.foo",
            "strategy.internal",
            "customer-list.backup",
            "alice",
            "bob",
            "jdoe",
            "admin",
            "password reset",
            "tax documents",
            "client merger",
            "*.pdf invoice",
            "Users",
            "Documents",
            "Desktop",
            "OneDrive",
            "token-abc123",
            "ssh-private-key",
            "api_key",
            "Q4 layoffs",
            "medical bills",
            "private notes",
            @"C:\Users\alice\Documents\secret-plan.txt"
        };

        foreach (var dangerous in dangerousValues)
        {
            var diagnosticEvent = CreateAllStringFieldsEvent(dangerous);

            var json = DiagnosticJsonSerializer.Serialize(diagnosticEvent);

            Assert.IsFalse(json.Contains(dangerous, StringComparison.OrdinalIgnoreCase), $"Dangerous value was serialized unchanged: {dangerous}");
            Assert.IsFalse(json.Contains(PredictableShaRedactionToken(dangerous), StringComparison.OrdinalIgnoreCase), $"Dangerous value used a predictable unsalted redaction token: {dangerous}");
            StringAssert.Contains(json, "redacted-string");
        }
    }

    [TestMethod]
    public void Diagnostic_serializer_preserves_only_allowed_vocabulary_and_generated_ids()
    {
        var diagnosticEvent = new DiagnosticEvent
        {
            EventId = "0123456789abcdef0123456789abcdef",
            EventType = "persistence.field-fallback",
            UtcTimestamp = DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            SequenceNumber = 1,
            Severity = "warning",
            Component = "persistence",
            OperationId = "11111111111111111111111111111111",
            CorrelationId = "22222222-2222-2222-2222-222222222222",
            OperationKind = "read",
            ResultState = "field-fallback",
            ReasonCode = "field-fallback",
            DocumentType = "session",
            SchemaVersion = 1,
            MigrationResult = "not-needed",
            FallbackSource = "canonical",
            FallbackCount = 2,
            FallbackFieldCodes = ["windowPlacement", "activeTabIndex"],
            LastActionMarkerCategory = "navigation",
            PathClassification = "local",
            PathFingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            ExtensionClass = ".txt"
        };

        var json = DiagnosticJsonSerializer.Serialize(diagnosticEvent);

        StringAssert.Contains(json, "\"eventType\":\"persistence.field-fallback\"");
        StringAssert.Contains(json, "\"component\":\"persistence\"");
        StringAssert.Contains(json, "\"operationId\":\"11111111111111111111111111111111\"");
        StringAssert.Contains(json, "\"correlationId\":\"22222222-2222-2222-2222-222222222222\"");
        StringAssert.Contains(json, "\"reasonCode\":\"field-fallback\"");
        StringAssert.Contains(json, "\"fallbackFieldCodes\":[\"windowPlacement\",\"activeTabIndex\"]");
        Assert.IsFalse(json.Contains("redacted-string", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Terminal_diagnostic_reason_codes_survive_serialization_and_redact_dangerous_values()
    {
        var dangerousValues = new[]
        {
            @"C:\Users\alice\secret-project",
            @"pwsh -NoProfile -File C:\Users\alice\run.ps1",
            "alice",
            "secret-plan",
            ".env",
            "id_rsa"
        };

        foreach (var reasonCode in TerminalLaunchReasonCodes.All)
        {
            var diagnosticEvent = new DiagnosticEvent
            {
                EventId = dangerousValues[0],
                EventType = "terminal.launch",
                UtcTimestamp = DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
                SequenceNumber = 1,
                Severity = "warning",
                Component = "terminal",
                OperationId = dangerousValues[1],
                CorrelationId = dangerousValues[5],
                OperationKind = "terminal-launch",
                ResultState = "failed",
                ReasonCode = reasonCode,
                DocumentType = dangerousValues[4],
                MigrationResult = dangerousValues[3],
                FallbackSource = dangerousValues[3],
                FallbackFieldCodes = [dangerousValues[3]],
                LastActionMarkerCategory = dangerousValues[2],
                PathClassification = dangerousValues[2],
                ExtensionClass = dangerousValues[4],
                TerminalTargetKind = "windows-terminal"
            };

            var json = DiagnosticJsonSerializer.Serialize(diagnosticEvent);

            Assert.AreEqual(reasonCode, ReadStringField(json, "reasonCode"));
            Assert.AreNotEqual("redacted-string", ReadStringField(json, "reasonCode"));
            foreach (var dangerousValue in dangerousValues)
            {
                Assert.IsFalse(json.Contains(dangerousValue, StringComparison.OrdinalIgnoreCase), $"Dangerous value was serialized unchanged: {dangerousValue}");
            }
        }
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
                component: "persistence",
                operationKind: "write",
                reasonCode: "io-error",
                path: $@"C:\Users\alice\Documents\secret-{i}.txt",
                redactor: redactor,
                timestampUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z").AddSeconds(i)));
        }

        for (var i = 0; i < 5; i++)
        {
            writer.RecordCrashMarker("startup", DateTimeOffset.Parse("2026-05-04T00:00:00Z").AddMinutes(i));
        }

        writer.RecordLastActionMarker("navigation", "navigation", DateTimeOffset.Parse("2026-05-04T00:00:00Z"));
        writer.RecordLastActionMarker("navigation", "navigation", DateTimeOffset.Parse("2026-05-04T00:01:00Z"));

        Assert.IsTrue(Directory.GetFiles(Path.Combine(workspace.Root, "logs"), "*.jsonl").Length > 1);
        Assert.AreEqual(3, Directory.GetFiles(Path.Combine(workspace.Root, "crash-markers"), "*.json").Length);
        Assert.AreEqual(1, Directory.GetFiles(Path.Combine(workspace.Root, "last-action-markers"), "navigation.json").Length);
        Assert.IsTrue(writer.HasRepeatedCrashMarkers("startup", threshold: 3));

        var allDiagnostics = string.Join(Environment.NewLine, Directory.GetFiles(workspace.Root, "*", SearchOption.AllDirectories).Select(File.ReadAllText));

        Assert.IsFalse(allDiagnostics.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(allDiagnostics.Contains("secret-", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Local_diagnostics_are_best_effort_when_storage_is_unavailable()
    {
        var root = Path.Combine(Path.GetTempPath(), "velofile-diagnostics-tests", Guid.NewGuid().ToString("N"), "blocked-by-file");
        Directory.CreateDirectory(Path.GetDirectoryName(root)!);
        File.WriteAllText(root, "not a directory");

        var writer = new LocalDiagnosticLogStore(root, DiagnosticRetentionPolicy.Default);

        writer.Write(new DiagnosticEvent
        {
            EventId = "evt-1",
            EventType = "operation.failure",
            UtcTimestamp = DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            SequenceNumber = 1,
            Severity = "warning",
            Component = "persistence",
            OperationKind = "write",
            ResultState = "failed",
            ReasonCode = "io-error"
        });
        writer.RecordCrashMarker("startup", DateTimeOffset.Parse("2026-05-04T00:00:00Z"));
        writer.RecordLastActionMarker("navigation", "navigation", DateTimeOffset.Parse("2026-05-04T00:00:00Z"));

        Assert.IsFalse(writer.HasRepeatedCrashMarkers("startup", threshold: 1));
        Assert.IsNotNull(writer.LastFailureReasonCode);
    }

    private static DiagnosticEvent CreateAllStringFieldsEvent(string value)
    {
        return new DiagnosticEvent
        {
            EventId = value,
            EventType = value,
            UtcTimestamp = DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            SequenceNumber = 1,
            Severity = value,
            Component = value,
            OperationId = value,
            CorrelationId = value,
            OperationKind = value,
            ResultState = value,
            ReasonCode = value,
            DocumentType = value,
            MigrationResult = value,
            FallbackSource = value,
            FallbackFieldCodes = [value],
            LastActionMarkerCategory = value,
            PathClassification = value,
            PathFingerprint = value,
            ExtensionClass = value
        };
    }

    private static string PredictableShaRedactionToken(string value)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return "redacted-" + Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }

    private static string? ReadStringField(string json, string fieldName)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return document.RootElement.GetProperty(fieldName).GetString();
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
