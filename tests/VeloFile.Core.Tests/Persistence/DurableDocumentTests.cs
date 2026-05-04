using System.Text.Json.Nodes;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Persistence;

#pragma warning disable MSTEST0032, MSTEST0037

namespace VeloFile.Core.Tests.Persistence;

[TestClass]
[TestCategory("Persistence")]
public sealed class DurableDocumentTests
{
    [TestMethod]
    public void Session_document_envelope_has_required_header()
    {
        var payload = SessionStatePayload.Empty;
        var envelope = DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Session,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            payload);

        var json = SessionStateDocumentCodec.Instance.Serialize(envelope);
        var root = JsonNode.Parse(json)!.AsObject();

        Assert.AreEqual("session", (string?)root["documentType"]);
        Assert.AreEqual(1, (int?)root["schemaVersion"]);
        Assert.AreEqual(1, (int?)root["minimumReaderVersion"]);
        Assert.AreEqual("1.0.0-test", (string?)root["appVersion"]);
        Assert.AreEqual("2026-05-04T00:00:00.0000000+00:00", (string?)root["writtenAtUtc"]);
        Assert.IsNotNull(root["payload"]);
    }

    [TestMethod]
    public void Durable_payloads_cover_settings_favorites_and_recent_locations()
    {
        var settings = DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Settings,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            SettingsStatePayload.Default);

        var favorites = DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Favorites,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            new FavoritesStatePayload([new PinnedLocationState("Projects", "D:\\projects")]));

        var recentLocations = RecentLocationsStatePayload.Create(
            Enumerable.Range(1, 25)
                .Select(i => new RecentLocationState($"D:\\projects\\{i:D2}", DateTimeOffset.Parse("2026-05-04T00:00:00Z").AddMinutes(i))));

        Assert.AreEqual("settings", settings.DocumentType);
        Assert.IsTrue(settings.Payload.ShowFileExtensions);
        Assert.IsFalse(settings.Payload.ShowProtectedOperatingSystemFiles);
        Assert.AreEqual("favorites", favorites.DocumentType);
        Assert.AreEqual("recentLocations", DurableDocumentTypes.RecentLocations);
        Assert.AreEqual(20, recentLocations.Entries.Count, "Recent locations are capped at the V1 storage boundary.");
        Assert.AreEqual("D:\\projects\\25", recentLocations.Entries[0].Path);
    }

    [TestMethod]
    public void Session_reader_ignores_unknown_fields_and_falls_back_per_optional_field()
    {
        var json = """
            {
              "documentType": "session",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "unknownRoot": true,
              "payload": {
                "tabs": [
                  {
                    "path": "D:\\projects\\velofile",
                    "sortColumn": "name",
                    "sortDirection": "ascending",
                    "viewMode": "details",
                    "scrollAnchorName": "README.md",
                    "backHistory": ["D:\\projects"],
                    "forwardHistory": []
                  }
                ],
                "activeTabIndex": "not-an-int",
                "selection": ["must-not-be-restored.txt"],
                "filterText": "must-not-be-restored",
                "unknownPayload": "ignored"
              }
            }
            """;

        var result = SessionStateDocumentCodec.Instance.Read(json);

        Assert.IsTrue(result.Success, result.FailureReason);
        Assert.AreEqual(1, result.Document!.Payload.Tabs.Count);
        Assert.AreEqual("D:\\projects\\velofile", result.Document.Payload.Tabs[0].Path);
        Assert.AreEqual(0, result.Document.Payload.ActiveTabIndex, "Malformed optional active tab index must fall back per field.");
        Assert.AreEqual("README.md", result.Document.Payload.Tabs[0].ScrollAnchorName);
        Assert.IsFalse(result.Document.Payload.RestoresSelection);
        Assert.IsFalse(result.Document.Payload.RestoresFilterText);
        Assert.IsTrue(result.Fallbacks.Any(fallback => fallback.FieldName == "activeTabIndex"));
        Assert.IsTrue(result.UnknownFieldCount >= 2);
    }

    [TestMethod]
    public void Repository_recovers_last_known_good_then_safe_defaults()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var diagnostics = new CollectingDiagnosticSink();
        var repository = new DurableDocumentRepository<SessionStatePayload>(
            "session.json",
            SessionStateDocumentCodec.Instance,
            storage,
            () => SessionStatePayload.Empty,
            diagnostics);

        var tab = new SessionTabState(
            Path: "D:\\projects\\velofile",
            SortColumn: "name",
            SortDirection: "ascending",
            ViewMode: "details",
            ScrollAnchorName: "README.md",
            BackHistory: [],
            ForwardHistory: []);

        repository.Write(DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Session,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            new SessionStatePayload([tab], ActiveTabIndex: 0, WindowPlacement: null)));

        storage.Files["session.json"] = "{ corrupt canonical";

        var recovered = repository.Read();

        Assert.AreEqual("lastKnownGood", recovered.Source);
        Assert.AreEqual("D:\\projects\\velofile", recovered.Payload.Tabs[0].Path);
        Assert.IsTrue(diagnostics.Events.Any(e => e.EventType == "persistence.fallback" && e.FallbackSource == "lastKnownGood"));

        storage.Files["session.json.bak"] = "{ corrupt backup";

        var safeDefault = repository.Read();

        Assert.AreEqual("safeDefaults", safeDefault.Source);
        Assert.AreEqual(0, safeDefault.Payload.Tabs.Count);
        Assert.IsTrue(diagnostics.Events.Any(e => e.EventType == "persistence.fallback" && e.FallbackSource == "safeDefaults"));
    }

    private sealed class InMemoryDurableDocumentStorage : IDurableDocumentStorage
    {
        public Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public string BackupPath(string canonicalPath)
        {
            return canonicalPath + ".bak";
        }

        public bool TryReadText(string path, out string content)
        {
            return Files.TryGetValue(path, out content!);
        }

        public void WriteAtomic(string canonicalPath, string content)
        {
            if (Files.TryGetValue(canonicalPath, out var previous))
            {
                Files[BackupPath(canonicalPath)] = previous;
            }

            Files[canonicalPath] = content;
            Files[BackupPath(canonicalPath)] = content;
        }
    }
}
