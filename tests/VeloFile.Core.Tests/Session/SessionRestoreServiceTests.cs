using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Session;

[TestClass]
[TestCategory("Session")]
public sealed class SessionRestoreServiceTests
{
    [TestMethod]
    public void Restore_keeps_missing_paths_visible_and_restores_included_navigation_sidebar_and_visibility_state()
    {
        var restore = new SessionRestoreService(
            new SetPathExistenceProbe(existingPaths: [@"D:\projects"]),
            new TestMonitorPlacementResolver(availableMonitor: @"\\.\DISPLAY1", fallbackPlacement: new WindowPlacementState(0, 0, 1000, 700, @"\\.\DISPLAY1")),
            new SetScrollAnchorResolver(existingAnchors: [(@"D:\projects", "README.md")]),
            new TestCrashRecoverySignal(shouldOfferStartFresh: false));

        var session = new SessionStatePayload(
            Tabs:
            [
                new SessionTabState(
                    Path: @"D:\projects",
                    SortColumn: "lastWriteTimeUtc",
                    SortDirection: "descending",
                    ViewMode: "largeIcons",
                    ScrollAnchorName: "README.md",
                    BackHistory: [@"D:\"],
                    ForwardHistory: [@"D:\archive"]),
                new SessionTabState(
                    Path: @"Z:\missing",
                    SortColumn: "name",
                    SortDirection: "ascending",
                    ViewMode: "details",
                    ScrollAnchorName: "gone.txt",
                    BackHistory: [],
                    ForwardHistory: [])
            ],
            ActiveTabIndex: 1,
            WindowPlacement: new WindowPlacementState(10, 20, 1200, 800, @"\\.\DISPLAY1"));
        var favorites = new FavoritesStatePayload([new PinnedLocationState("Projects", @"D:\projects")]);
        var recents = RecentLocationsStatePayload.Create([new RecentLocationState(@"D:\projects", DateTimeOffset.Parse("2026-05-04T00:00:00Z"))]);
        var settings = new SettingsStatePayload(
            ShowHiddenFiles: true,
            ShowProtectedOperatingSystemFiles: false,
            ShowFileExtensions: false);

        var result = restore.Restore(session, favorites, recents, settings);

        Assert.AreEqual(2, result.Workspace.Tabs.Count);
        Assert.AreEqual(1, result.Workspace.ActiveTabIndex);
        Assert.AreEqual(@"Z:\missing", result.Workspace.ActiveTab.Path);
        Assert.AreEqual(NavigationTabLocationState.MissingLocation, result.Workspace.ActiveTab.LocationState);
        Assert.AreEqual(@"Z:\missing", result.Workspace.ActiveTab.MissingPath);
        Assert.IsTrue(result.Workspace.ActiveTab.CloseTabActionAvailable);

        var restoredFirstTab = result.Workspace.Tabs[0];
        Assert.AreEqual(FileListViewMode.LargeIcons, restoredFirstTab.ViewMode);
        Assert.AreEqual("lastWriteTimeUtc", restoredFirstTab.SortColumn);
        Assert.AreEqual("descending", restoredFirstTab.SortDirection);
        Assert.AreEqual("README.md", restoredFirstTab.ScrollAnchorName);
        Assert.AreEqual(@"D:\", restoredFirstTab.BackHistory.Single());
        Assert.AreEqual(@"D:\archive", restoredFirstTab.ForwardHistory.Single());

        Assert.AreEqual("Projects", result.Sidebar.Favorites.Single().DisplayName);
        Assert.AreEqual(@"D:\projects", result.Sidebar.RecentLocations.Single().Path);
        Assert.AreEqual(new VisibilitySettings(true, false, false), result.Visibility);
        Assert.AreEqual(10, result.WindowPlacement!.Left);
        Assert.IsFalse(result.RestoresSelection);
        Assert.IsFalse(result.RestoresFilterText);
        Assert.IsFalse(result.RestoresSearchState);
        Assert.IsFalse(result.RestoresClipboard);
        Assert.IsFalse(result.RestoresInFlightOperations);
    }

    [TestMethod]
    public void Restore_falls_back_window_placement_when_stored_monitor_is_removed()
    {
        var fallback = new WindowPlacementState(0, 0, 1024, 768, @"\\.\DISPLAY1");
        var restore = new SessionRestoreService(
            new SetPathExistenceProbe(existingPaths: [@"D:\projects"]),
            new TestMonitorPlacementResolver(availableMonitor: @"\\.\DISPLAY1", fallbackPlacement: fallback),
            new SetScrollAnchorResolver(existingAnchors: []),
            new TestCrashRecoverySignal(shouldOfferStartFresh: false));

        var session = new SessionStatePayload(
            Tabs: [new SessionTabState(@"D:\projects", "name", "ascending", "details", null, [], [])],
            ActiveTabIndex: 0,
            WindowPlacement: new WindowPlacementState(4000, 100, 1200, 800, @"\\.\REMOVED"));

        var result = restore.Restore(session, FavoritesStatePayload.Empty, RecentLocationsStatePayload.Empty, SettingsStatePayload.Default);

        Assert.AreEqual(fallback, result.WindowPlacement);
    }

    [TestMethod]
    public void Restore_drops_scroll_anchor_when_first_visible_item_no_longer_exists()
    {
        var restore = new SessionRestoreService(
            new SetPathExistenceProbe(existingPaths: [@"D:\projects"]),
            new TestMonitorPlacementResolver(availableMonitor: @"\\.\DISPLAY1", fallbackPlacement: null),
            new SetScrollAnchorResolver(existingAnchors: []),
            new TestCrashRecoverySignal(shouldOfferStartFresh: false));
        var session = new SessionStatePayload(
            Tabs: [new SessionTabState(@"D:\projects", "name", "ascending", "details", "deleted.txt", [], [])],
            ActiveTabIndex: 0,
            WindowPlacement: null);

        var result = restore.Restore(session, FavoritesStatePayload.Empty, RecentLocationsStatePayload.Empty, SettingsStatePayload.Default);

        Assert.IsNull(result.Workspace.ActiveTab.ScrollAnchorName);
    }

    [TestMethod]
    public void Repeated_crash_marker_state_offers_start_fresh_without_blocking_restore()
    {
        var restore = new SessionRestoreService(
            new SetPathExistenceProbe(existingPaths: [@"D:\projects"]),
            new TestMonitorPlacementResolver(availableMonitor: @"\\.\DISPLAY1", fallbackPlacement: null),
            new SetScrollAnchorResolver(existingAnchors: []),
            new TestCrashRecoverySignal(shouldOfferStartFresh: true));
        var session = new SessionStatePayload(
            Tabs: [new SessionTabState(@"D:\projects", "name", "ascending", "details", null, [], [])],
            ActiveTabIndex: 0,
            WindowPlacement: null);

        var result = restore.Restore(session, FavoritesStatePayload.Empty, RecentLocationsStatePayload.Empty, SettingsStatePayload.Default);

        Assert.IsTrue(result.CrashRecovery.StartFreshOffered);
        Assert.AreEqual("repeated-crash-marker", result.CrashRecovery.ReasonCode);
        Assert.AreEqual(@"D:\projects", result.Workspace.ActiveTab.Path);
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

    private sealed class SetScrollAnchorResolver : IScrollAnchorResolver
    {
        private readonly HashSet<(string Path, string AnchorName)> _existingAnchors;

        public SetScrollAnchorResolver(IEnumerable<(string Path, string AnchorName)> existingAnchors)
        {
            _existingAnchors = new HashSet<(string Path, string AnchorName)>(existingAnchors);
        }

        public bool Exists(string path, string anchorName)
        {
            return _existingAnchors.Contains((path, anchorName));
        }
    }

    private sealed class TestMonitorPlacementResolver : IMonitorPlacementResolver
    {
        private readonly string _availableMonitor;
        private readonly WindowPlacementState? _fallbackPlacement;

        public TestMonitorPlacementResolver(string availableMonitor, WindowPlacementState? fallbackPlacement)
        {
            _availableMonitor = availableMonitor;
            _fallbackPlacement = fallbackPlacement;
        }

        public bool IsAvailable(string? monitorDeviceName)
        {
            return string.Equals(_availableMonitor, monitorDeviceName, StringComparison.OrdinalIgnoreCase);
        }

        public WindowPlacementState? Fallback(WindowPlacementState? requestedPlacement)
        {
            return _fallbackPlacement ?? requestedPlacement;
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
}
