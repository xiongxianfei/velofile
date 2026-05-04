using VeloFile.Core.Diagnostics;
using VeloFile.Core.Persistence;
using VeloFile.Windows.Storage;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.Storage;

[TestClass]
[TestCategory("Persistence")]
public sealed class PersistenceWindowsDurableDocumentStorageTests
{
    [TestMethod]
    public void Windows_storage_writes_canonical_and_last_known_good()
    {
        using var workspace = TemporaryWorkspace.Create();
        var canonicalPath = Path.Combine(workspace.Root, "session.json");
        var storage = new WindowsDurableDocumentStorage();
        var repository = CreateRepository(canonicalPath, storage);

        repository.Write(CreateSession("D:\\projects\\velofile"));
        repository.Write(CreateSession("D:\\projects\\velofile2"));

        Assert.IsTrue(File.Exists(canonicalPath));
        Assert.IsTrue(File.Exists(storage.BackupPath(canonicalPath)));
        Assert.AreEqual("canonical", repository.Read().Source);
        Assert.AreEqual("D:\\projects\\velofile2", repository.Read().Payload.Tabs[0].Path);
    }

    [TestMethod]
    public void Windows_storage_recovers_backup_when_canonical_is_corrupt_and_ignores_stale_temp()
    {
        using var workspace = TemporaryWorkspace.Create();
        var canonicalPath = Path.Combine(workspace.Root, "session.json");
        var storage = new WindowsDurableDocumentStorage();
        var repository = CreateRepository(canonicalPath, storage);

        repository.Write(CreateSession("D:\\projects\\velofile"));
        File.WriteAllText(canonicalPath, "{ corrupt canonical");
        File.WriteAllText(Path.Combine(workspace.Root, ".session.json.stale.tmp"), "{ temp from interrupted write");

        var recovered = repository.Read();

        Assert.AreEqual("lastKnownGood", recovered.Source);
        Assert.AreEqual("D:\\projects\\velofile", recovered.Payload.Tabs[0].Path);
    }

    [TestMethod]
    public void Windows_storage_returns_safe_defaults_when_canonical_and_backup_are_corrupt()
    {
        using var workspace = TemporaryWorkspace.Create();
        var canonicalPath = Path.Combine(workspace.Root, "session.json");
        var storage = new WindowsDurableDocumentStorage();
        var repository = CreateRepository(canonicalPath, storage);

        repository.Write(CreateSession("D:\\projects\\velofile"));
        File.WriteAllText(canonicalPath, "{ corrupt canonical");
        File.WriteAllText(storage.BackupPath(canonicalPath), "{ corrupt backup");

        var recovered = repository.Read();

        Assert.AreEqual("safeDefaults", recovered.Source);
        Assert.AreEqual(0, recovered.Payload.Tabs.Count);
    }

    [TestMethod]
    public void Windows_storage_treats_missing_reads_as_recoverable_instead_of_using_check_then_read()
    {
        using var workspace = TemporaryWorkspace.Create();
        var path = Path.Combine(workspace.Root, "missing-session.json");
        var storage = new WindowsDurableDocumentStorage();

        var read = storage.ReadText(path);

        Assert.AreEqual(DurableDocumentStorageReadStatus.Missing, read.Status);
        Assert.IsNull(read.Content);
    }

    private static DurableDocumentRepository<SessionStatePayload> CreateRepository(string canonicalPath, WindowsDurableDocumentStorage storage)
    {
        return new DurableDocumentRepository<SessionStatePayload>(
            canonicalPath,
            SessionStateDocumentCodec.Instance,
            storage,
            () => SessionStatePayload.Empty,
            new CollectingDiagnosticSink());
    }

    private static DurableDocumentEnvelope<SessionStatePayload> CreateSession(string path)
    {
        var tab = new SessionTabState(
            Path: path,
            SortColumn: "name",
            SortDirection: "ascending",
            ViewMode: "details",
            ScrollAnchorName: "README.md",
            BackHistory: [],
            ForwardHistory: []);

        return DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Session,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            new SessionStatePayload([tab], ActiveTabIndex: 0, WindowPlacement: null));
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
            var root = Path.Combine(Path.GetTempPath(), "velofile-storage-tests", Guid.NewGuid().ToString("N"));
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
