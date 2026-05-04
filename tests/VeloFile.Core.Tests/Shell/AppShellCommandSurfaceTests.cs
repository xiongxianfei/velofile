using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Shell;

[TestClass]
[TestCategory("Navigation")]
[TestCategory("Session")]
public sealed class AppShellCommandSurfaceTests
{
    [TestMethod]
    public void Navigation_entry_points_mutate_the_active_tab_through_one_command_surface()
    {
        var favorite = new PinnedLocationState("Projects", @"D:\projects");
        var recent = new RecentLocationState(@"D:\recent", DateTimeOffset.Parse("2026-05-03T00:00:00Z"));
        var drive = new DriveEntry(@"E:\", @"E:\", DriveType.Fixed, IsReady: false, AvailableFreeSpaceBytes: null, TotalSizeBytes: null);
        var shell = CreateShell(
            initialPath: @"D:\start",
            favorites: [favorite],
            recents: [recent],
            drives: [drive],
            existingPaths: [@"D:\typed", @"D:\projects", @"D:\recent", @"E:\"]);

        shell.SubmitPath(@"D:\typed");
        Assert.AreEqual(@"D:\typed", shell.ActivePath);

        shell.ActivateSidebarTarget(shell.SidebarNavigationTargets.Single(target => target.Kind == ShellNavigationTargetKind.Favorite));
        Assert.AreEqual(@"D:\projects", shell.ActivePath);

        shell.ActivateSidebarTarget(shell.SidebarNavigationTargets.First(target => target.Kind == ShellNavigationTargetKind.RecentLocation && target.Path == @"D:\recent"));
        Assert.AreEqual(@"D:\recent", shell.ActivePath);

        shell.ActivateSidebarTarget(shell.SidebarNavigationTargets.Single(target => target.Kind == ShellNavigationTargetKind.Drive));
        Assert.AreEqual(@"E:\", shell.ActivePath);
        Assert.IsTrue(shell.RecentLocations.Any(item => item.Path == @"E:\"));
    }

    [TestMethod]
    public void History_parent_refresh_tab_lifecycle_and_reopen_are_command_surface_behavior()
    {
        var shell = CreateShell(
            initialPath: @"D:\start",
            existingPaths: [@"D:\start", @"D:\start\child", @"D:\other"]);

        shell.SubmitPath(@"D:\start\child");
        Assert.AreEqual(@"D:\start\child", shell.ActivePath);

        shell.NavigateBack();
        Assert.AreEqual(@"D:\start", shell.ActivePath);

        shell.NavigateForward();
        Assert.AreEqual(@"D:\start\child", shell.ActivePath);

        shell.NavigateToParent();
        Assert.AreEqual(@"D:\start", shell.ActivePath);

        var historyBeforeRefresh = shell.ActiveTab.BackHistory.ToArray();
        var reloadVersionBefore = shell.ActiveTab.ReloadVersion;
        shell.RefreshActiveTab();
        CollectionAssert.AreEqual(historyBeforeRefresh, shell.ActiveTab.BackHistory.ToArray());
        Assert.AreEqual(reloadVersionBefore + 1, shell.ActiveTab.ReloadVersion);

        shell.NewTab();
        Assert.AreEqual(2, shell.Tabs.Count);
        Assert.AreEqual(@"D:\start", shell.ActivePath);

        shell.SubmitPath(@"D:\other");
        shell.DuplicateActiveTab();
        Assert.AreEqual(3, shell.Tabs.Count);
        Assert.AreEqual(@"D:\other", shell.ActivePath);

        shell.CloseActiveTab();
        Assert.AreEqual(2, shell.Tabs.Count);
        Assert.AreEqual(@"D:\other", shell.ActivePath);

        shell.ReopenClosedTab();
        Assert.AreEqual(3, shell.Tabs.Count);
        Assert.AreEqual(@"D:\other", shell.ActivePath);
    }

    [TestMethod]
    public void Typed_missing_path_preserves_active_tab_history_listing_and_recents()
    {
        var shell = CreateShell(
            initialPath: @"D:\start",
            recents: [new RecentLocationState(@"D:\start", DateTimeOffset.Parse("2026-05-03T00:00:00Z"))],
            existingPaths: [@"D:\start"]);

        var historyBefore = shell.ActiveTab.BackHistory.ToArray();
        var recentsBefore = shell.RecentLocations.Select(item => item.Path).ToArray();
        var result = shell.SubmitPath(@"Z:\missing");

        Assert.IsFalse(result.Accepted);
        Assert.AreEqual(@"D:\start", shell.ActivePath);
        CollectionAssert.AreEqual(historyBefore, shell.ActiveTab.BackHistory.ToArray());
        CollectionAssert.AreEqual(recentsBefore, shell.RecentLocations.Select(item => item.Path).ToArray());
        Assert.AreEqual(NavigationTabLocationState.Available, shell.ActiveTab.LocationState);
        Assert.IsFalse(shell.MissingLocationVisible);
        Assert.IsNotNull(shell.PathEntryError);
        Assert.AreEqual(@"Z:\missing", shell.PathEntryError!.SubmittedPath);
        Assert.AreEqual("missing", shell.PathEntryError.ReasonCode);
    }

    [TestMethod]
    public void Typed_invalid_or_empty_path_preserves_active_tab_and_history()
    {
        var shell = CreateShell(initialPath: @"D:\start", existingPaths: [@"D:\start"]);

        var emptyResult = shell.SubmitPath("   ");

        Assert.IsFalse(emptyResult.Accepted);
        Assert.AreEqual(@"D:\start", shell.ActivePath);
        Assert.AreEqual(0, shell.ActiveTab.BackHistory.Count);
        Assert.AreEqual(NavigationTabLocationState.Available, shell.ActiveTab.LocationState);
        Assert.AreEqual("empty-path", shell.PathEntryError!.ReasonCode);

        var invalidResult = shell.SubmitPath("relative-folder");

        Assert.IsFalse(invalidResult.Accepted);
        Assert.AreEqual(@"D:\start", shell.ActivePath);
        Assert.AreEqual(0, shell.ActiveTab.BackHistory.Count);
        Assert.AreEqual(NavigationTabLocationState.Available, shell.ActiveTab.LocationState);
        Assert.AreEqual("invalid-path", shell.PathEntryError!.ReasonCode);
    }

    [TestMethod]
    public void Visibility_toggles_write_durable_settings_without_resetting_unrelated_flags()
    {
        var writer = new CollectingSettingsStateWriter();
        var shell = CreateShell(
            initialPath: @"D:\start",
            existingPaths: [@"D:\start"],
            settingsWriter: writer);

        shell.SetShowHiddenFiles(true);
        shell.SetShowProtectedOperatingSystemFiles(true, confirmed: true);
        shell.SetShowFileExtensions(false);

        Assert.AreEqual(3, writer.Writes.Count);
        Assert.AreEqual(new SettingsStatePayload(true, false, true), writer.Writes[0]);
        Assert.AreEqual(new SettingsStatePayload(true, true, true), writer.Writes[1]);
        Assert.AreEqual(new SettingsStatePayload(true, true, false), writer.Writes[2]);
        Assert.AreEqual(new VisibilitySettings(true, true, false), shell.VisibilitySettings);
    }

    [TestMethod]
    public void Visibility_toggle_write_failure_does_not_crash_or_revert_current_session()
    {
        var shell = CreateShell(
            initialPath: @"D:\start",
            existingPaths: [@"D:\start"],
            settingsWriter: new ThrowingSettingsStateWriter());

        shell.SetShowHiddenFiles(true);

        Assert.IsTrue(shell.VisibilitySettings.ShowHiddenFiles);
    }

    [TestMethod]
    public void Start_fresh_replaces_restored_state_with_a_safe_default_workspace()
    {
        var shell = CreateShell(
            initialPath: @"D:\restored",
            defaultPath: @"C:\Users\alice",
            crashRecovery: new CrashRecoveryState(StartFreshOffered: true, ReasonCode: "repeated-crash-marker"),
            existingPaths: [@"D:\restored", @"C:\Users\alice"]);

        shell.StartFresh();

        Assert.IsFalse(shell.CrashRecovery.StartFreshOffered);
        Assert.AreEqual(1, shell.Tabs.Count);
        Assert.AreEqual(@"C:\Users\alice", shell.ActivePath);
    }

    private static AppShellCommandSurface CreateShell(
        string initialPath,
        string? defaultPath = null,
        IReadOnlyList<PinnedLocationState>? favorites = null,
        IReadOnlyList<RecentLocationState>? recents = null,
        IReadOnlyList<DriveEntry>? drives = null,
        CrashRecoveryState? crashRecovery = null,
        IReadOnlyList<string>? existingPaths = null,
        ISettingsStateWriter? settingsWriter = null)
    {
        var workspace = NavigationWorkspace.Create(initialPath);
        var sidebar = SidebarStateService.Create(
            new FavoritesStatePayload(favorites ?? []),
            RecentLocationsStatePayload.Create(recents ?? []),
            drives ?? []);
        var visibility = VisibilitySettingsService.FromPayload(SettingsStatePayload.Default);
        var pathProbe = new SetPathExistenceProbe(existingPaths ?? [initialPath]);

        return new AppShellCommandSurface(
            windowTitle: "VeloFile",
            workspace,
            sidebar,
            visibility,
            crashRecovery ?? CrashRecoveryState.None,
            new TestDefaultLaunchPathProvider(defaultPath ?? initialPath),
            pathProbe,
            settingsWriter ?? NoOpSettingsStateWriter.Instance,
            utcNow: () => DateTimeOffset.Parse("2026-05-04T00:00:00Z"));
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

    private sealed class CollectingSettingsStateWriter : ISettingsStateWriter
    {
        public List<SettingsStatePayload> Writes { get; } = [];

        public void Write(SettingsStatePayload payload)
        {
            Writes.Add(payload);
        }
    }

    private sealed class ThrowingSettingsStateWriter : ISettingsStateWriter
    {
        public void Write(SettingsStatePayload payload)
        {
            throw new IOException("settings storage unavailable");
        }
    }
}
