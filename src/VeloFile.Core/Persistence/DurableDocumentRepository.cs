using VeloFile.Core.Diagnostics;

namespace VeloFile.Core.Persistence;

public interface IDurableDocumentStorage
{
    string BackupPath(string canonicalPath);

    bool TryReadText(string path, out string content);

    void WriteAtomic(string canonicalPath, string content);
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
            return canonical;
        }

        var backupPath = _storage.BackupPath(_canonicalPath);
        if (TryRead(backupPath, "lastKnownGood", out var backup))
        {
            _diagnostics.Write(DiagnosticEvent.CreatePersistenceFallback(
                documentType: backup.DocumentType,
                fallbackSource: "lastKnownGood",
                reasonCode: "canonical-unreadable",
                unknownFieldCount: backup.UnknownFieldCount,
                corruptFieldCount: backup.CorruptFieldCount));
            return backup;
        }

        var safeDefault = _safeDefaultFactory();
        _diagnostics.Write(DiagnosticEvent.CreatePersistenceFallback(
            documentType: "unknown",
            fallbackSource: "safeDefaults",
            reasonCode: "canonical-and-backup-unreadable",
            unknownFieldCount: 0,
            corruptFieldCount: 1));

        return new DurableDocumentRepositoryReadResult<TPayload>(
            Payload: safeDefault,
            Source: "safeDefaults",
            DocumentType: "unknown",
            UnknownFieldCount: 0,
            CorruptFieldCount: 1);
    }

    private bool TryRead(string path, string source, out DurableDocumentRepositoryReadResult<TPayload> result)
    {
        result = default!;

        if (!_storage.TryReadText(path, out var content))
        {
            return false;
        }

        var read = _codec.Read(content);
        if (!read.Success || read.Document is null)
        {
            return false;
        }

        result = new DurableDocumentRepositoryReadResult<TPayload>(
            Payload: read.Document.Payload,
            Source: source,
            DocumentType: read.Document.DocumentType,
            UnknownFieldCount: read.UnknownFieldCount,
            CorruptFieldCount: read.CorruptFieldCount);
        return true;
    }
}

public sealed record DurableDocumentRepositoryReadResult<TPayload>(
    TPayload Payload,
    string Source,
    string DocumentType,
    int UnknownFieldCount,
    int CorruptFieldCount);
