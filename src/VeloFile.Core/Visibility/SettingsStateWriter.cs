using VeloFile.Core.Persistence;

namespace VeloFile.Core.Visibility;

public interface ISettingsStateWriter
{
    void Write(SettingsStatePayload payload);
}

public sealed class NoOpSettingsStateWriter : ISettingsStateWriter
{
    public static NoOpSettingsStateWriter Instance { get; } = new();

    private NoOpSettingsStateWriter()
    {
    }

    public void Write(SettingsStatePayload payload)
    {
        _ = payload;
    }
}

public sealed class DurableSettingsStateWriter : ISettingsStateWriter
{
    private readonly DurableDocumentRepository<SettingsStatePayload> _repository;
    private readonly string _appVersion;
    private readonly Func<DateTimeOffset> _utcNow;

    public DurableSettingsStateWriter(
        DurableDocumentRepository<SettingsStatePayload> repository,
        string appVersion,
        Func<DateTimeOffset> utcNow)
    {
        _repository = repository;
        _appVersion = appVersion;
        _utcNow = utcNow;
    }

    public void Write(SettingsStatePayload payload)
    {
        _repository.Write(DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Settings,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            _appVersion,
            _utcNow(),
            payload));
    }
}
