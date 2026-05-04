using VeloFile.Core.Diagnostics;

namespace VeloFile.Core.Persistence;

public interface IDurableDocumentStorage
{
    string BackupPath(string canonicalPath);

    DurableDocumentStorageReadResult ReadText(string path);

    void WriteAtomic(string canonicalPath, string content);
}

public enum DurableDocumentStorageReadStatus
{
    Found,
    Missing,
    RecoverableFailure
}

public sealed record DurableDocumentStorageReadResult(
    DurableDocumentStorageReadStatus Status,
    string? Content,
    string? ReasonCode)
{
    public static DurableDocumentStorageReadResult Found(string content)
    {
        return new DurableDocumentStorageReadResult(DurableDocumentStorageReadStatus.Found, content, ReasonCode: null);
    }

    public static DurableDocumentStorageReadResult Missing()
    {
        return new DurableDocumentStorageReadResult(DurableDocumentStorageReadStatus.Missing, Content: null, ReasonCode: "missing");
    }

    public static DurableDocumentStorageReadResult RecoverableFailure(string reasonCode)
    {
        return new DurableDocumentStorageReadResult(DurableDocumentStorageReadStatus.RecoverableFailure, Content: null, reasonCode);
    }
}

public sealed class DurableDocumentRepository<TPayload>
{
    private readonly string _canonicalPath;
    private readonly IDurableDocumentCodec<TPayload> _codec;
    private readonly IDurableDocumentStorage _storage;
    private readonly Func<TPayload> _safeDefaultFactory;
    private readonly IDiagnosticSink _diagnostics;

    public DurableDocumentRepository(
        string canonicalPath,
        IDurableDocumentCodec<TPayload> codec,
        IDurableDocumentStorage storage,
        Func<TPayload> safeDefaultFactory,
        IDiagnosticSink diagnostics)
    {
        _canonicalPath = canonicalPath;
        _codec = codec;
        _storage = storage;
        _safeDefaultFactory = safeDefaultFactory;
        _diagnostics = diagnostics;
    }

    public void Write(DurableDocumentEnvelope<TPayload> envelope)
    {
        var content = _codec.Serialize(envelope);
        var validation = _codec.Read(content);
        if (!validation.Success)
        {
            throw new InvalidOperationException($"Durable document serialization produced invalid content: {validation.FailureReason}");
        }

        _storage.WriteAtomic(_canonicalPath, content);
    }

    public DurableDocumentRepositoryReadResult<TPayload> Read()
    {
        if (TryRead(_canonicalPath, "canonical", out var canonical))
        {
            WriteFieldFallbackDiagnosticIfNeeded(canonical);
            return canonical;
        }

        var backupPath = _storage.BackupPath(_canonicalPath);
        if (TryRead(backupPath, "lastKnownGood", out var backup))
        {
            TryWriteDiagnostic(DiagnosticEvent.CreatePersistenceFallback(
                documentType: backup.DocumentType,
                fallbackSource: "lastKnownGood",
                reasonCode: "canonical-unreadable",
                unknownFieldCount: backup.UnknownFieldCount,
                corruptFieldCount: backup.CorruptFieldCount));
            WriteFieldFallbackDiagnosticIfNeeded(backup);
            return backup;
        }

        var safeDefault = _safeDefaultFactory();
        TryWriteDiagnostic(DiagnosticEvent.CreatePersistenceFallback(
            documentType: "unknown",
            fallbackSource: "safeDefaults",
            reasonCode: "canonical-and-backup-unreadable",
            unknownFieldCount: 0,
            corruptFieldCount: 1));

        return new DurableDocumentRepositoryReadResult<TPayload>(
            Payload: safeDefault,
            Source: "safeDefaults",
            DocumentType: "unknown",
            SchemaVersion: 0,
            Fallbacks: [],
            UnknownFieldCount: 0,
            CorruptFieldCount: 1);
    }

    private bool TryRead(string path, string source, out DurableDocumentRepositoryReadResult<TPayload> result)
    {
        result = default!;

        var storageRead = ReadFromStorage(path);
        if (storageRead.Status is not DurableDocumentStorageReadStatus.Found || storageRead.Content is null)
        {
            return false;
        }

        var read = _codec.Read(storageRead.Content);
        if (!read.Success || read.Document is null)
        {
            return false;
        }

        result = new DurableDocumentRepositoryReadResult<TPayload>(
            Payload: read.Document.Payload,
            Source: source,
            DocumentType: read.Document.DocumentType,
            SchemaVersion: read.Document.SchemaVersion,
            Fallbacks: read.Fallbacks,
            UnknownFieldCount: read.UnknownFieldCount,
            CorruptFieldCount: read.CorruptFieldCount);
        return true;
    }

    private DurableDocumentStorageReadResult ReadFromStorage(string path)
    {
        try
        {
            return _storage.ReadText(path);
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return DurableDocumentStorageReadResult.RecoverableFailure(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
    }

    private void WriteFieldFallbackDiagnosticIfNeeded(DurableDocumentRepositoryReadResult<TPayload> result)
    {
        if (result.Fallbacks.Count == 0 && result.CorruptFieldCount == 0)
        {
            return;
        }

        var fieldCodes = result.Fallbacks
            .Select(fallback => fallback.FieldName)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        TryWriteDiagnostic(DiagnosticEvent.CreatePersistenceFieldFallback(
            documentType: result.DocumentType,
            schemaVersion: result.SchemaVersion,
            fallbackSource: result.Source,
            fallbackFieldCodes: fieldCodes,
            unknownFieldCount: result.UnknownFieldCount,
            corruptFieldCount: result.CorruptFieldCount));
    }

    private void TryWriteDiagnostic(DiagnosticEvent diagnosticEvent)
    {
        try
        {
            _diagnostics.Write(diagnosticEvent);
        }
        catch
        {
            // Diagnostics are best-effort; persistence reads must not fail because logging failed.
        }
    }
}

public sealed record DurableDocumentRepositoryReadResult<TPayload>(
    TPayload Payload,
    string Source,
    string DocumentType,
    int SchemaVersion,
    IReadOnlyList<PersistenceFallbackEvent> Fallbacks,
    int UnknownFieldCount,
    int CorruptFieldCount);
