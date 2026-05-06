using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using VeloFile.Core.Terminal;

namespace VeloFile.Core.Diagnostics;

public static class DiagnosticStringSanitizer
{
    private static readonly Regex GuidN = new("^[0-9a-fA-F]{32}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex GuidD = new("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex Ulid = new("^[0-9A-HJKMNP-TV-Z]{26}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PathFingerprint = new("^[0-9a-f]{64}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> EventTypes = new(StringComparer.Ordinal)
    {
        "operation.failure",
        "persistence.fallback",
        "persistence.field-fallback",
        "crash.marker",
        "last-action.marker",
        "terminal.launch"
    };

    private static readonly HashSet<string> Severities = new(StringComparer.Ordinal)
    {
        "info",
        "warning",
        "error"
    };

    private static readonly HashSet<string> Components = new(StringComparer.Ordinal)
    {
        "app",
        "benchmark",
        "diagnostics",
        "file-operation",
        "navigation",
        "persistence",
        "preview",
        "search",
        "session",
        "storage",
        "terminal"
    };

    private static readonly HashSet<string> OperationKinds = new(StringComparer.Ordinal)
    {
        "benchmark",
        "copy",
        "delete",
        "file-operation",
        "move",
        "navigation",
        "preview",
        "read",
        "rename",
        "search",
        "session-restore",
        "terminal-launch",
        "write"
    };

    private static readonly HashSet<string> ResultStates = new(StringComparer.Ordinal)
    {
        "cancelled",
        "crashed",
        "failed",
        "fallback",
        "field-fallback",
        "recorded",
        "recovered",
        "skipped",
        "succeeded",
        "timed-out"
    };

    private static readonly HashSet<string> ReasonCodes = CreateReasonCodes();

    private static readonly HashSet<string> DocumentTypes = new(StringComparer.Ordinal)
    {
        "favorites",
        "recentLocations",
        "session",
        "settings",
        "unknown"
    };

    private static readonly HashSet<string> MigrationResults = new(StringComparer.Ordinal)
    {
        "degraded",
        "failed",
        "migrated",
        "not-needed"
    };

    private static readonly HashSet<string> FallbackSources = new(StringComparer.Ordinal)
    {
        "canonical",
        "lastKnownGood",
        "safeDefaults"
    };

    private static readonly HashSet<string> SchemaFieldCodes = new(StringComparer.Ordinal)
    {
        "activeTabIndex",
        "entries",
        "entries[]",
        "pinnedLocations",
        "pinnedLocations[]",
        "showFileExtensions",
        "showHiddenFiles",
        "showProtectedOperatingSystemFiles",
        "tabs",
        "tabs[]",
        "windowPlacement"
    };

    private static readonly HashSet<string> LastActionCategories = new(StringComparer.Ordinal)
    {
        "diagnostics",
        "file-operation",
        "navigation",
        "preview",
        "preview-generation",
        "search",
        "session-restore",
        "startup",
        "terminal-launch"
    };

    private static readonly HashSet<string> PathClassifications = new(StringComparer.Ordinal)
    {
        "cloud-placeholder",
        "local",
        "mapped",
        "network",
        "protected",
        "removable",
        "unavailable",
        "unknown"
    };

    private static readonly HashSet<string> ExtensionClasses = new(StringComparer.Ordinal)
    {
        ".7z",
        ".bmp",
        ".cs",
        ".csv",
        ".doc",
        ".docx",
        ".exe",
        ".gif",
        ".jpeg",
        ".jpg",
        ".json",
        ".md",
        ".pdf",
        ".png",
        ".ppt",
        ".pptx",
        ".ps1",
        ".rar",
        ".tif",
        ".tiff",
        ".txt",
        ".xls",
        ".xlsx",
        ".xml",
        ".zip"
    };

    private static readonly HashSet<string> TerminalTargetKinds = new(StringComparer.Ordinal)
    {
        "command-prompt",
        "git-bash",
        "powershell-7",
        "windows-powershell",
        "windows-terminal",
        "wsl"
    };

    private static readonly IReadOnlyDictionary<string, DiagnosticFieldPolicy> FieldPolicies =
        new ReadOnlyDictionary<string, DiagnosticFieldPolicy>(new Dictionary<string, DiagnosticFieldPolicy>(StringComparer.Ordinal)
        {
            ["eventId"] = DiagnosticFieldPolicy.GeneratedIdentifier,
            ["eventType"] = DiagnosticFieldPolicy.Known(EventTypes),
            ["severity"] = DiagnosticFieldPolicy.Known(Severities),
            ["component"] = DiagnosticFieldPolicy.Known(Components),
            ["operationId"] = DiagnosticFieldPolicy.GeneratedIdentifier,
            ["correlationId"] = DiagnosticFieldPolicy.GeneratedIdentifier,
            ["operationKind"] = DiagnosticFieldPolicy.Known(OperationKinds),
            ["resultState"] = DiagnosticFieldPolicy.Known(ResultStates),
            ["reasonCode"] = DiagnosticFieldPolicy.Known(ReasonCodes),
            ["documentType"] = DiagnosticFieldPolicy.Known(DocumentTypes),
            ["migrationResult"] = DiagnosticFieldPolicy.Known(MigrationResults),
            ["fallbackSource"] = DiagnosticFieldPolicy.Known(FallbackSources),
            ["fallbackFieldCodes"] = DiagnosticFieldPolicy.Known(SchemaFieldCodes),
            ["lastActionMarkerCategory"] = DiagnosticFieldPolicy.Known(LastActionCategories),
            ["pathClassification"] = DiagnosticFieldPolicy.Known(PathClassifications),
            ["pathFingerprint"] = DiagnosticFieldPolicy.PathFingerprint,
            ["extensionClass"] = DiagnosticFieldPolicy.Known(ExtensionClasses),
            ["terminalTargetKind"] = DiagnosticFieldPolicy.Known(TerminalTargetKinds)
        });

    public static string Sanitize(string fieldName, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return FieldPolicies.TryGetValue(fieldName, out var policy) && policy.Allows(value)
            ? value
            : Redact(value);
    }

    private static string Redact(string value)
    {
        return "redacted-string";
    }

    private static HashSet<string> CreateReasonCodes()
    {
        var reasonCodes = new HashSet<string>(StringComparer.Ordinal)
        {
            "access-denied",
            "canonical-and-backup-unreadable",
            "canonical-unreadable",
            "cancelled",
            "corrupt",
            "decode-error",
            "field-fallback",
            "invalid-path",
            "io-error",
            "last-known-good-used",
            "migration-fallback",
            "missing",
            "primary-read-failed",
            "safe-defaults-used",
            "security-denied",
            "timeout",
            "unsupported",
            "unknown"
        };

        foreach (var reasonCode in TerminalLaunchReasonCodes.All)
        {
            reasonCodes.Add(reasonCode);
        }

        return reasonCodes;
    }

    private sealed record DiagnosticFieldPolicy(
        Func<string, bool> Allows)
    {
        public static DiagnosticFieldPolicy GeneratedIdentifier { get; } = new(value =>
            GuidN.IsMatch(value)
            || GuidD.IsMatch(value)
            || Ulid.IsMatch(value));

        public static DiagnosticFieldPolicy PathFingerprint { get; } = new(value => DiagnosticStringSanitizer.PathFingerprint.IsMatch(value));

        public static DiagnosticFieldPolicy Known(HashSet<string> allowed)
        {
            return new DiagnosticFieldPolicy(allowed.Contains);
        }
    }
}
