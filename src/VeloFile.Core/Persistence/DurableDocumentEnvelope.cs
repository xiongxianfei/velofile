namespace VeloFile.Core.Persistence;

public static class DurableDocumentTypes
{
    public const string Session = "session";
    public const string Settings = "settings";
    public const string Favorites = "favorites";
    public const string RecentLocations = "recentLocations";
}

public sealed record DurableDocumentEnvelope<TPayload>(
    string DocumentType,
    int SchemaVersion,
    int MinimumReaderVersion,
    string AppVersion,
    DateTimeOffset WrittenAtUtc,
    TPayload Payload);

public static class DurableDocumentEnvelope
{
    public static DurableDocumentEnvelope<TPayload> Create<TPayload>(
        string documentType,
        int schemaVersion,
        int minimumReaderVersion,
        string appVersion,
        DateTimeOffset writtenAtUtc,
        TPayload payload)
    {
        return new DurableDocumentEnvelope<TPayload>(
            documentType,
            schemaVersion,
            minimumReaderVersion,
            appVersion,
            writtenAtUtc,
            payload);
    }
}

public sealed record PersistenceFallbackEvent(string FieldName, string Reason);

public sealed record DocumentReadResult<TPayload>(
    bool Success,
    DurableDocumentEnvelope<TPayload>? Document,
    IReadOnlyList<PersistenceFallbackEvent> Fallbacks,
    int UnknownFieldCount,
    int CorruptFieldCount,
    string? FailureReason);

public interface IDurableDocumentCodec<TPayload>
{
    string Serialize(DurableDocumentEnvelope<TPayload> envelope);

    DocumentReadResult<TPayload> Read(string json);
}
