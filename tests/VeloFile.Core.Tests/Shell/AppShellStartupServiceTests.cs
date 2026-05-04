using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Shell;

[TestClass]
[TestCategory("Session")]
public sealed class AppShellStartupServiceTests
{
    [TestMethod]
    public void Normal_bootstrap_restores_session_settings_favorites_recents_drives_and_window_placement()
    {
        var startup = CreateStartup(existingPaths: [@"D:\projects"]);
        var tab = new SessionTabState(
            Path: @"D:\projects",
            SortColumn: "name",
            SortDirection: "ascending",
            ViewMode: "details",
            ScrollAnchorName: null,
            BackHistory: [@"D:\"],
            ForwardHistory: []);
        var input = new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: new SessionStatePayload([tab], ActiveTabIndex: 0, WindowPlacement: new WindowPlacementState(10, 20, 1200, 800, @"\\.\DISPLAY1")),
            Settings: new SettingsStatePayload(ShowHiddenFiles: true, ShowProtectedOperatingSystemFiles: false, ShowFileExtensions: false),
            Favorites: new FavoritesStatePayload([new PinnedLocationState("Projects", @"D:\projects")]),
            RecentLocations: RecentLocationsStatePayload.Create([new RecentLocationState(@"D:\projects", DateTimeOffset.Parse("2026-05-04T00:00:00Z"))]),
            Drives: [new DriveEntry(@"E:\", @"E:\", DriveType.Fixed, IsReady: false, AvailableFreeSpaceBytes: null, TotalSizeBytes: null)]);

        var state = startup.CreateStartupState(input);

        Assert.AreEqual("VeloFile", state.WindowTitle);
        Assert.AreEqual(@"D:\projects", state.CommandSurface.ActivePath);
        Assert.AreEqual(new VisibilitySettings(true, false, false), state.CommandSurface.VisibilitySettings);
        Assert.AreEqual("Projects", state.CommandSurface.Favorites.Single().DisplayName);
        Assert.AreEqual(@"D:\projects", state.CommandSurface.RecentLocations.Single().Path);
        Assert.AreEqual(@"E:\", state.CommandSurface.Drives.Single().RootPath);
        Assert.AreEqual(10, state.WindowPlacement!.Left);
    }

    [TestMethod]
    public void Safe_default_bootstrap_launches_with_valid_default_tab_when_session_is_empty()
    {
        var startup = CreateStartup(existingPaths: [@"C:\Users\alice"], defaultPath: @"C:\Users\alice");
        var input = new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: SessionStatePayload.Empty,
            Settings: SettingsStatePayload.Default,
            Favorites: FavoritesStatePayload.Empty,
            RecentLocations: RecentLocationsStatePayload.Empty,
            Drives: []);

        var state = startup.CreateStartupState(input);

        Assert.AreEqual(1, state.CommandSurface.Tabs.Count);
        Assert.AreEqual(@"C:\Users\alice", state.CommandSurface.ActivePath);
        Assert.AreEqual(0, state.CommandSurface.Workspace.ActiveTabIndex);
    }

    [TestMethod]
    public void Crash_marker_bootstrap_surfaces_start_fresh_and_start_fresh_remains_non_crashing()
    {
        var startup = CreateStartup(
            existingPaths: [@"D:\projects", @"C:\Users\alice"],
            defaultPath: @"C:\Users\alice",
            crashRecovery: true);
        var input = new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: new SessionStatePayload([new SessionTabState(@"D:\projects", "name", "ascending", "details", null, [], [])], 0, null),
            Settings: SettingsStatePayload.Default,
            Favorites: FavoritesStatePayload.Empty,
            RecentLocations: RecentLocationsStatePayload.Empty,
            Drives: []);

        var state = startup.CreateStartupState(input);

        Assert.IsTrue(state.CommandSurface.CrashRecovery.StartFreshOffered);

        state.CommandSurface.StartFresh();

        Assert.AreEqual(@"C:\Users\alice", state.CommandSurface.ActivePath);
        Assert.IsFalse(state.CommandSurface.CrashRecovery.StartFreshOffered);
    }

    [TestMethod]
    public void Missing_restored_path_reaches_shell_as_visible_recoverable_state()
    {
        var startup = CreateStartup(existingPaths: []);
        var input = new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: new SessionStatePayload([new SessionTabState(@"Z:\missing", "name", "ascending", "details", null, [], [])], 0, null),
            Settings: SettingsStatePayload.Default,
            Favorites: FavoritesStatePayload.Empty,
            RecentLocations: RecentLocationsStatePayload.Empty,
            Drives: []);

        var state = startup.CreateStartupState(input);

        Assert.IsTrue(state.CommandSurface.MissingLocationVisible);
        Assert.AreEqual(@"Z:\missing", state.CommandSurface.MissingLocationPath);
        Assert.IsTrue(state.CommandSurface.ActiveTab.CloseTabActionAvailable);
    }

    [TestMethod]
    public void Startup_state_exposes_safe_window_placement_resolution_for_app_applier()
    {
        var startup = CreateStartup(
            existingPaths: [@"D:\projects"],
            monitorPlacementResolver: new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
                new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
            ])));
        var input = new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: new SessionStatePayload(
                [new SessionTabState(@"D:\projects", "name", "ascending", "details", null, [], [])],
                ActiveTabIndex: 0,
                WindowPlacement: new WindowPlacementState(Left: 200, Top: 200, Width: 100, Height: 100, MonitorDeviceName: @"\\.\DISPLAY1")),
            Settings: SettingsStatePayload.Default,
            Favorites: FavoritesStatePayload.Empty,
            RecentLocations: RecentLocationsStatePayload.Empty,
            Drives: []);

        var state = startup.CreateStartupState(input);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseInvalidSize, state.WindowPlacementResolution.Status);
        Assert.IsTrue(state.WindowPlacementResolution.ShouldApply);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackWidth, state.WindowPlacement!.Width);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackHeight, state.WindowPlacement.Height);
    }

    [TestMethod]
    public void Visibility_settings_survive_durable_write_and_restart_bootstrap()
    {
        var storage = new InMemoryDurableDocumentStorage();
        var diagnostics = new CollectingDiagnosticSink();
        var settingsRepository = new DurableDocumentRepository<SettingsStatePayload>(
            "settings.json",
            SettingsStateDocumentCodec.Instance,
            storage,
            () => SettingsStatePayload.Default,
            diagnostics);
        var writer = new DurableSettingsStateWriter(
            settingsRepository,
            "1.0.0-test",
            () => DateTimeOffset.Parse("2026-05-04T00:00:00Z"));
        var initialStartup = CreateStartup(
            existingPaths: [@"D:\projects"],
            settingsStateWriter: writer);
        var initialState = initialStartup.CreateStartupState(new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: new SessionStatePayload([new SessionTabState(@"D:\projects", "name", "ascending", "details", null, [], [])], 0, null),
            Settings: SettingsStatePayload.Default,
            Favorites: FavoritesStatePayload.Empty,
            RecentLocations: RecentLocationsStatePayload.Empty,
            Drives: []));

        initialState.CommandSurface.SetShowHiddenFiles(true);
        initialState.CommandSurface.SetShowFileExtensions(false);

        var restoredSettings = settingsRepository.Read().Payload;
        var restartedStartup = CreateStartup(existingPaths: [@"D:\projects"]);
        var restartedState = restartedStartup.CreateStartupState(new AppShellStartupInput(
            WindowTitle: "VeloFile",
            Session: new SessionStatePayload([new SessionTabState(@"D:\projects", "name", "ascending", "details", null, [], [])], 0, null),
            Settings: restoredSettings,
            Favorites: FavoritesStatePayload.Empty,
            RecentLocations: RecentLocationsStatePayload.Empty,
            Drives: []));

        Assert.IsTrue(restartedState.CommandSurface.VisibilitySettings.ShowHiddenFiles);
        Assert.IsFalse(restartedState.CommandSurface.VisibilitySettings.ShowFileExtensions);
        Assert.IsFalse(restartedState.CommandSurface.VisibilitySettings.ShowProtectedOperatingSystemFiles);
    }

    private static AppShellStartupService CreateStartup(
        IReadOnlyList<string> existingPaths,
        string defaultPath = @"C:\Users\alice",
        bool crashRecovery = false,
        ISettingsStateWriter? settingsStateWriter = null,
        IMonitorPlacementResolver? monitorPlacementResolver = null)
    {
        return new AppShellStartupService(
            new SessionRestoreService(
                new SetPathExistenceProbe(existingPaths),
                monitorPlacementResolver ?? new TestMonitorPlacementResolver(),
                new NoScrollAnchorResolver(),
                new TestCrashRecoverySignal(crashRecovery),
                new TestDefaultLaunchPathProvider(defaultPath)),
            new TestDefaultLaunchPathProvider(defaultPath),
            new SetPathExistenceProbe(existingPaths),
            new CollectingDiagnosticSink(),
            utcNow: () => DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            settingsStateWriter);
    }

    private sealed class SetPathExistenceProbe : IPathExistenceProbe
    {
        private readonly HashSet<string> _existingPaths;

        public SetPathExistenceProbe(IEnumerable<string> existingPaths)
        {
            _existingPaths = new HashSet<string>(existingPaths, StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string path)
        {
            return _existingPaths.Contains(path);
        }
    }

    private sealed class NoScrollAnchorResolver : IScrollAnchorResolver
    {
        public bool Exists(string path, string anchorName)
        {
            return false;
        }
    }

    private sealed class TestMonitorPlacementResolver : IMonitorPlacementResolver
    {
        public bool IsAvailable(string? monitorDeviceName)
        {
            return string.Equals(monitorDeviceName, @"\\.\DISPLAY1", StringComparison.OrdinalIgnoreCase);
        }

        public WindowPlacementState? Fallback(WindowPlacementState? requestedPlacement)
        {
            return null;
        }
    }

    private sealed class TestCrashRecoverySignal : ICrashRecoverySignal
    {
        public TestCrashRecoverySignal(bool shouldOfferStartFresh)
        {
            ShouldOfferStartFresh = shouldOfferStartFresh;
        }

        public bool ShouldOfferStartFresh { get; }
    }

    private sealed class TestDefaultLaunchPathProvider : IDefaultLaunchPathProvider
    {
        private readonly string _path;

        public TestDefaultLaunchPathProvider(string path)
        {
            _path = path;
        }

        public string GetDefaultLaunchPath()
        {
            return _path;
        }
    }

    private sealed class InMemoryDurableDocumentStorage : IDurableDocumentStorage
    {
        public Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public string BackupPath(string canonicalPath)
        {
            return canonicalPath + ".bak";
        }

        public DurableDocumentStorageReadResult ReadText(string path)
        {
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

    private sealed class FakeMonitorLayoutSource : IMonitorLayoutSource
    {
        private readonly IReadOnlyList<MonitorWorkArea> _monitors;

        public FakeMonitorLayoutSource(IReadOnlyList<MonitorWorkArea> monitors)
        {
            _monitors = monitors;
        }

        public IReadOnlyList<MonitorWorkArea> GetCurrentWorkAreas()
        {
            return _monitors;
        }
    }
}
