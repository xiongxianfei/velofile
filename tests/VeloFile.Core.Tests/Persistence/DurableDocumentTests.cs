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

        var settingsRoundTrip = SettingsStateDocumentCodec.Instance.Read(SettingsStateDocumentCodec.Instance.Serialize(settings));
        var favoritesRoundTrip = FavoritesStateDocumentCodec.Instance.Read(FavoritesStateDocumentCodec.Instance.Serialize(favorites));
        var recentRoundTrip = RecentLocationsStateDocumentCodec.Instance.Read(RecentLocationsStateDocumentCodec.Instance.Serialize(
            DurableDocumentEnvelope.Create(
                DurableDocumentTypes.RecentLocations,
                schemaVersion: 1,
                minimumReaderVersion: 1,
                appVersion: "1.0.0-test",
                writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
                recentLocations)));

        Assert.IsTrue(settingsRoundTrip.Success, settingsRoundTrip.FailureReason);
        Assert.IsTrue(favoritesRoundTrip.Success, favoritesRoundTrip.FailureReason);
        Assert.IsTrue(recentRoundTrip.Success, recentRoundTrip.FailureReason);
        Assert.IsTrue(settingsRoundTrip.Document!.Payload.ShowFileExtensions);
        Assert.AreEqual("Projects", favoritesRoundTrip.Document!.Payload.PinnedLocations[0].DisplayName);
        Assert.AreEqual(20, recentRoundTrip.Document!.Payload.Entries.Count);
    }

    [TestMethod]
    [TestCategory("Terminal")]
    public void Settings_document_round_trips_preferred_terminal_target()
    {
        var envelope = DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Settings,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            SettingsStatePayload.Default with { PreferredTerminalTargetId = "git-bash" });

        var roundTrip = SettingsStateDocumentCodec.Instance.Read(SettingsStateDocumentCodec.Instance.Serialize(envelope));

        Assert.IsTrue(roundTrip.Success, roundTrip.FailureReason);
        Assert.AreEqual("git-bash", roundTrip.Document!.Payload.PreferredTerminalTargetId);
    }

    [TestMethod]
    public void Session_document_round_trips_window_placement_and_falls_back_per_malformed_placement_field()
    {
        var payload = new SessionStatePayload(
            Tabs: [],
            ActiveTabIndex: 0,
            WindowPlacement: new WindowPlacementState(Left: 10, Top: 20, Width: 1200, Height: 800, MonitorDeviceName: @"\\.\DISPLAY2"));

        var envelope = DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Session,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            payload);

        var roundTrip = SessionStateDocumentCodec.Instance.Read(SessionStateDocumentCodec.Instance.Serialize(envelope));

        Assert.IsTrue(roundTrip.Success, roundTrip.FailureReason);
        Assert.AreEqual(10, roundTrip.Document!.Payload.WindowPlacement!.Left);
        Assert.AreEqual(20, roundTrip.Document.Payload.WindowPlacement.Top);
        Assert.AreEqual(1200, roundTrip.Document.Payload.WindowPlacement.Width);
        Assert.AreEqual(800, roundTrip.Document.Payload.WindowPlacement.Height);
        Assert.AreEqual(@"\\.\DISPLAY2", roundTrip.Document.Payload.WindowPlacement.MonitorDeviceName);

        var malformed = """
            {
              "documentType": "session",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "payload": {
                "tabs": [],
                "activeTabIndex": 0,
                "windowPlacement": {
                  "left": 10,
                  "top": "not-an-int",
                  "width": 1200,
                  "height": 800,
                  "monitorDeviceName": "\\\\.\\DISPLAY2"
                }
              }
            }
            """;

        var fallback = SessionStateDocumentCodec.Instance.Read(malformed);

        Assert.IsTrue(fallback.Success, fallback.FailureReason);
        Assert.IsNull(fallback.Document!.Payload.WindowPlacement);
        Assert.IsTrue(fallback.Fallbacks.Any(item => item.FieldName == "windowPlacement"));
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

    [TestMethod]
    public void Repository_recovers_when_primary_storage_read_is_recoverable_failure()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var diagnostics = new CollectingDiagnosticSink();
        var repository = new DurableDocumentRepository<SessionStatePayload>(
            "session.json",
            SessionStateDocumentCodec.Instance,
            storage,
            () => SessionStatePayload.Empty,
            diagnostics);

        storage.Files["session.json.bak"] = SessionStateDocumentCodec.Instance.Serialize(DurableDocumentEnvelope.Create(
            DurableDocumentTypes.Session,
            schemaVersion: 1,
            minimumReaderVersion: 1,
            appVersion: "1.0.0-test",
            writtenAtUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            new SessionStatePayload(
                [new SessionTabState("D:\\projects\\velofile", "name", "ascending", "details", null, [], [])],
                ActiveTabIndex: 0,
                WindowPlacement: null)));
        storage.ReadFailures["session.json"] = DurableDocumentStorageReadResult.RecoverableFailure("sharing-violation");

        var recovered = repository.Read();

        Assert.AreEqual("lastKnownGood", recovered.Source);
        Assert.AreEqual("D:\\projects\\velofile", recovered.Payload.Tabs[0].Path);
        Assert.IsTrue(diagnostics.Events.Any(e => e.EventType == "persistence.fallback" && e.FallbackSource == "lastKnownGood"));
    }

    [TestMethod]
    public void Repository_logs_field_fallback_when_canonical_session_optional_field_is_malformed()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var diagnostics = new CollectingDiagnosticSink();
        var repository = new DurableDocumentRepository<SessionStatePayload>(
            "session.json",
            SessionStateDocumentCodec.Instance,
            storage,
            () => SessionStatePayload.Empty,
            diagnostics);

        storage.Files["session.json"] = """
            {
              "documentType": "session",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "payload": {
                "tabs": [
                  {
                    "path": "D:\\Users\\alice\\Documents\\secret-plan",
                    "sortColumn": "name",
                    "sortDirection": "ascending",
                    "viewMode": "details",
                    "backHistory": [],
                    "forwardHistory": []
                  }
                ],
                "activeTabIndex": 0,
                "windowPlacement": {
                  "left": 10,
                  "top": "not-an-int",
                  "width": 1200,
                  "height": 800
                }
              }
            }
            """;

        var result = repository.Read();

        Assert.AreEqual("canonical", result.Source);
        Assert.IsNull(result.Payload.WindowPlacement);
        var diagnosticEvent = AssertSingleFieldFallback(diagnostics, DurableDocumentTypes.Session, "canonical", "windowPlacement");
        var json = DiagnosticJsonSerializer.Serialize(diagnosticEvent);
        Assert.IsFalse(json.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("secret-plan", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains(@"D:\\Users", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Repository_logs_field_fallback_when_canonical_settings_optional_field_is_malformed()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var diagnostics = new CollectingDiagnosticSink();
        var repository = new DurableDocumentRepository<SettingsStatePayload>(
            "settings.json",
            SettingsStateDocumentCodec.Instance,
            storage,
            () => SettingsStatePayload.Default,
            diagnostics);

        storage.Files["settings.json"] = """
            {
              "documentType": "settings",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "payload": {
                "showHiddenFiles": true,
                "showProtectedOperatingSystemFiles": false,
                "showFileExtensions": "private notes"
              }
            }
            """;

        var result = repository.Read();

        Assert.AreEqual("canonical", result.Source);
        Assert.IsTrue(result.Payload.ShowHiddenFiles);
        Assert.IsTrue(result.Payload.ShowFileExtensions);
        var diagnosticEvent = AssertSingleFieldFallback(diagnostics, DurableDocumentTypes.Settings, "canonical", "showFileExtensions");
        var json = DiagnosticJsonSerializer.Serialize(diagnosticEvent);
        Assert.IsFalse(json.Contains("private notes", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Repository_logs_field_fallback_when_favorites_or_recent_location_entries_are_malformed()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var diagnostics = new CollectingDiagnosticSink();
        var favoritesRepository = new DurableDocumentRepository<FavoritesStatePayload>(
            "favorites.json",
            FavoritesStateDocumentCodec.Instance,
            storage,
            () => FavoritesStatePayload.Empty,
            diagnostics);
        var recentRepository = new DurableDocumentRepository<RecentLocationsStatePayload>(
            "recentLocations.json",
            RecentLocationsStateDocumentCodec.Instance,
            storage,
            () => RecentLocationsStatePayload.Empty,
            diagnostics);

        storage.Files["favorites.json"] = """
            {
              "documentType": "favorites",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "payload": {
                "pinnedLocations": [
                  { "displayName": "Projects", "path": "D:\\projects" },
                  { "displayName": "secret-plan", "path": 42 }
                ]
              }
            }
            """;
        storage.Files["recentLocations.json"] = """
            {
              "documentType": "recentLocations",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "payload": {
                "entries": [
                  { "path": "D:\\projects", "lastVisitedUtc": "2026-05-04T00:00:00Z" },
                  { "path": "D:\\Users\\alice\\Documents\\secret-plan", "lastVisitedUtc": "not-a-date" }
                ]
              }
            }
            """;

        var favorites = favoritesRepository.Read();
        var recent = recentRepository.Read();

        Assert.AreEqual(1, favorites.Payload.PinnedLocations.Count);
        Assert.AreEqual(1, recent.Payload.Entries.Count);
        AssertSingleFieldFallback(diagnostics, DurableDocumentTypes.Favorites, "canonical", "pinnedLocations[]");
        var recentDiagnostic = AssertSingleFieldFallback(diagnostics, DurableDocumentTypes.RecentLocations, "canonical", "entries[]");
        var json = DiagnosticJsonSerializer.Serialize(recentDiagnostic);
        Assert.IsFalse(json.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("secret-plan", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Repository_read_succeeds_when_field_fallback_diagnostic_sink_throws()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var repository = new DurableDocumentRepository<SessionStatePayload>(
            "session.json",
            SessionStateDocumentCodec.Instance,
            storage,
            () => SessionStatePayload.Empty,
            new ThrowingDiagnosticSink());

        storage.Files["session.json"] = """
            {
              "documentType": "session",
              "schemaVersion": 1,
              "minimumReaderVersion": 1,
              "appVersion": "1.0.0-test",
              "writtenAtUtc": "2026-05-04T00:00:00Z",
              "payload": {
                "tabs": [],
                "activeTabIndex": 0,
                "windowPlacement": {
                  "left": "bad",
                  "top": 0,
                  "width": 1200,
                  "height": 800
                }
              }
            }
            """;

        var result = repository.Read();

        Assert.AreEqual("canonical", result.Source);
        Assert.IsNull(result.Payload.WindowPlacement);
    }

    private sealed class InMemoryDurableDocumentStorage : IDurableDocumentStorage
    {
        public Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, DurableDocumentStorageReadResult> ReadFailures { get; } = new(StringComparer.OrdinalIgnoreCase);

        public string BackupPath(string canonicalPath)
        {
            return canonicalPath + ".bak";
        }

        public DurableDocumentStorageReadResult ReadText(string path)
        {
            if (ReadFailures.TryGetValue(path, out var failure))
            {
                return failure;
            }

            return Files.TryGetValue(path, out var content)
                ? DurableDocumentStorageReadResult.Found(content)
                : DurableDocumentStorageReadResult.Missing();
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

    private static DiagnosticEvent AssertSingleFieldFallback(
        CollectingDiagnosticSink diagnostics,
        string documentType,
        string fallbackSource,
        string expectedFieldCode)
    {
        var diagnosticEvent = diagnostics.Events.Single(e =>
            e.EventType == "persistence.field-fallback"
            && e.DocumentType == documentType
            && e.FallbackSource == fallbackSource
            && e.FallbackFieldCodes?.Contains(expectedFieldCode) == true);

        Assert.AreEqual("persistence", diagnosticEvent.Component);
        Assert.AreEqual("field-fallback", diagnosticEvent.ReasonCode);
        Assert.AreEqual(1, diagnosticEvent.FallbackCount);
        Assert.IsTrue(diagnosticEvent.CorruptFieldCount > 0);

        return diagnosticEvent;
    }

    private sealed class ThrowingDiagnosticSink : IDiagnosticSink
    {
        public void Write(DiagnosticEvent diagnosticEvent)
        {
            throw new IOException("diagnostic storage unavailable");
        }
    }
}
