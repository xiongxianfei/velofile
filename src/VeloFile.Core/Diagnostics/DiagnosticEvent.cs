using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeloFile.Core.Diagnostics;

public sealed record DiagnosticEvent
{
    public required string EventId { get; init; }

    public required string EventType { get; init; }

    public required DateTimeOffset UtcTimestamp { get; init; }

    public required long SequenceNumber { get; init; }

    public required string Severity { get; init; }

    public required string Component { get; init; }

    public string? OperationId { get; init; }

    public string? CorrelationId { get; init; }

    public string? OperationKind { get; init; }

    public string? ResultState { get; init; }

    public string? ReasonCode { get; init; }

    public long? DurationMs { get; init; }

    public long? TimeoutBudgetMs { get; init; }

    public int? RetryCount { get; init; }

    public bool? CancellationFlag { get; init; }

    public string? DocumentType { get; init; }

    public int? SchemaVersion { get; init; }

    public string? MigrationResult { get; init; }

    public string? FallbackSource { get; init; }

    public int? UnknownFieldCount { get; init; }

    public int? CorruptFieldCount { get; init; }

    public string? LastActionMarkerCategory { get; init; }

    public string? PathClassification { get; init; }

    public string? PathFingerprint { get; init; }

    public string? ExtensionClass { get; init; }

    public static DiagnosticEvent CreateFailure(
        string eventId,
        long sequenceNumber,
        string component,
        string operationKind,
        string reasonCode,
        string? path,
        PathRedactor redactor,
        DateTimeOffset timestampUtc)
    {
        var redaction = path is null ? null : redactor.Redact(path);

        return new DiagnosticEvent
        {
            EventId = eventId,
            EventType = "operation.failure",
            UtcTimestamp = timestampUtc,
            SequenceNumber = sequenceNumber,
            Severity = "warning",
            Component = component,
            OperationKind = operationKind,
            ResultState = "failed",
            ReasonCode = reasonCode,
            PathClassification = redaction?.PathClassification,
            PathFingerprint = redaction?.PathFingerprint,
            ExtensionClass = redaction?.ExtensionClass
        };
    }

    public static DiagnosticEvent CreatePersistenceFallback(
        string documentType,
        string fallbackSource,
        string reasonCode,
        int unknownFieldCount,
        int corruptFieldCount)
    {
        return new DiagnosticEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            EventType = "persistence.fallback",
            UtcTimestamp = DateTimeOffset.UtcNow,
            SequenceNumber = 0,
            Severity = "warning",
            Component = "Persistence",
            OperationKind = "read",
            ResultState = "fallback",
            ReasonCode = reasonCode,
            DocumentType = documentType,
            FallbackSource = fallbackSource,
            UnknownFieldCount = unknownFieldCount,
            CorruptFieldCount = corruptFieldCount
        };
    }
}

public interface IDiagnosticSink
{
    void Write(DiagnosticEvent diagnosticEvent);
}

public sealed class CollectingDiagnosticSink : IDiagnosticSink
{
    private readonly List<DiagnosticEvent> _events = [];

    public IReadOnlyList<DiagnosticEvent> Events => _events;

    public void Write(DiagnosticEvent diagnosticEvent)
    {
        _events.Add(diagnosticEvent);
    }
}

public static class DiagnosticJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(DiagnosticEvent diagnosticEvent)
    {
        return JsonSerializer.Serialize(diagnosticEvent, Options);
    }
}
