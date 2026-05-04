using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Navigation;

[TestClass]
[TestCategory("Navigation")]
public sealed class NavigationWorkspaceTests
{
    [TestMethod]
    public void Tabs_support_lifecycle_reorder_duplicate_reopen_and_keyboard_switching()
    {
        var workspace = NavigationWorkspace.Create(@"D:\one");

        var second = workspace.OpenTab(@"D:\two");
        Assert.AreEqual(second.Id, workspace.ActiveTab.Id);

        workspace.SwitchPreviousTab();
        Assert.AreEqual("tab-0001", workspace.ActiveTab.Id);
        workspace.SwitchNextTab();
        Assert.AreEqual(second.Id, workspace.ActiveTab.Id);

        var duplicate = workspace.DuplicateTab(second.Id);
        Assert.AreEqual(@"D:\two", duplicate.Path);
        Assert.AreEqual(3, workspace.Tabs.Count);

        workspace.ReorderTab(duplicate.Id, 0);
        Assert.AreEqual(duplicate.Id, workspace.Tabs[0].Id);
        Assert.AreEqual(0, workspace.ActiveTabIndex);

        workspace.CloseTab(duplicate.Id);
        Assert.AreEqual(2, workspace.Tabs.Count);
        Assert.AreEqual("tab-0001", workspace.ActiveTab.Id);

        var reopened = workspace.ReopenClosedTab();
        Assert.IsNotNull(reopened);
        Assert.AreEqual(@"D:\two", reopened!.Path);
        Assert.AreEqual(reopened.Id, workspace.ActiveTab.Id);

        workspace.SwitchPreviousTab();
        Assert.AreEqual(second.Id, workspace.ActiveTab.Id);

        workspace.SwitchNextTab();
        Assert.AreEqual(reopened.Id, workspace.ActiveTab.Id);
    }

    [TestMethod]
    public void Each_tab_keeps_independent_back_and_forward_history()
    {
        var workspace = NavigationWorkspace.Create(@"D:\projects");

        workspace.NavigateActive(@"D:\projects\velofile");
        var firstTab = workspace.ActiveTab.Id;
        var secondTab = workspace.OpenTab(@"C:\Users");
        workspace.NavigateActive(@"C:\Users\Public");

        workspace.SwitchToTab(firstTab);
        Assert.AreEqual(@"D:\projects\velofile", workspace.ActiveTab.Path);

        workspace.NavigateBack();
        Assert.AreEqual(@"D:\projects", workspace.ActiveTab.Path);
        Assert.AreEqual(0, workspace.ActiveTab.BackHistory.Count);
        Assert.AreEqual(@"D:\projects\velofile", workspace.ActiveTab.ForwardHistory.Single());

        workspace.SwitchToTab(secondTab.Id);
        Assert.AreEqual(@"C:\Users\Public", workspace.ActiveTab.Path);
        Assert.AreEqual(@"C:\Users", workspace.ActiveTab.BackHistory.Single());
        Assert.AreEqual(0, workspace.ActiveTab.ForwardHistory.Count);

        workspace.NavigateBack();
        workspace.NavigateForward();
        Assert.AreEqual(@"C:\Users\Public", workspace.ActiveTab.Path);
    }

    [TestMethod]
    public void Breadcrumb_segments_and_raw_path_navigation_update_the_active_tab()
    {
        var workspace = NavigationWorkspace.Create(@"D:\Data\Projects\VeloFile");
        var segments = BreadcrumbPath.Parse(workspace.ActiveTab.Path);

        Assert.AreEqual(@"D:\", segments[0].FullPath);
        Assert.AreEqual("Data", segments[1].DisplayName);
        Assert.AreEqual(@"D:\Data", segments[1].FullPath);
        Assert.AreEqual("VeloFile", segments[^1].DisplayName);

        workspace.NavigateActive(segments[1].FullPath);
        Assert.AreEqual(@"D:\Data", workspace.ActiveTab.Path);

        workspace.NavigateFromRawPathInput("  E:\\Scratch\\Work  ");
        Assert.AreEqual(@"E:\Scratch\Work", workspace.ActiveTab.Path);
        Assert.AreEqual(@"D:\Data", workspace.ActiveTab.BackHistory[^1]);
    }

    [TestMethod]
    public void Folder_open_entry_points_route_to_active_tab_and_record_recents()
    {
        var workspace = NavigationWorkspace.Create(@"D:\start");
        var favorite = new PinnedLocationState("Projects", @"D:\projects");
        var recent = new RecentLocationState(@"D:\recent", DateTimeOffset.Parse("2026-05-03T00:00:00Z"));
        var drive = new DriveEntry(
            Name: @"E:\",
            RootPath: @"E:\",
            DriveType: DriveType.Fixed,
            IsReady: false,
            AvailableFreeSpaceBytes: null,
            TotalSizeBytes: null);
        var sidebar = SidebarStateService.Create(
            new FavoritesStatePayload([favorite]),
            RecentLocationsStatePayload.Create([recent]),
            drives: [drive]);
        var now = DateTimeOffset.Parse("2026-05-04T00:00:00Z");
        var entryPoints = new NavigationEntryPointService(workspace, sidebar, () => now);

        entryPoints.OpenTypedPath(@"D:\typed");
        entryPoints.OpenPastedPath(@"D:\pasted");
        entryPoints.OpenBreadcrumbSegment(BreadcrumbPath.Parse(@"D:\Data\Projects")[1]);
        entryPoints.OpenSidebarLocation(@"D:\sidebar");
        entryPoints.OpenFavorite(favorite);
        entryPoints.OpenRecent(recent);
        entryPoints.OpenDrive(drive);

        Assert.AreEqual(@"E:\", workspace.ActiveTab.Path);
        Assert.IsTrue(workspace.ActiveTab.BackHistory.Contains(@"D:\typed"));
        Assert.IsTrue(workspace.ActiveTab.BackHistory.Contains(@"D:\pasted"));
        Assert.IsTrue(workspace.ActiveTab.BackHistory.Contains(@"D:\Data"));
        Assert.IsTrue(sidebar.State.RecentLocations.Any(item => item.Path == @"E:\" && item.LastVisitedUtc == now));
        Assert.IsTrue(sidebar.State.RecentLocations.Any(item => item.Path == @"D:\projects"));
    }

    [TestMethod]
    public void Active_tab_view_mode_and_scroll_anchor_are_navigation_state()
    {
        var workspace = NavigationWorkspace.Create(@"D:\folder");

        workspace.SetActiveViewMode(FileListViewMode.LargeIcons);
        workspace.SetActiveSort("lastWriteTimeUtc", "descending");
        workspace.SetActiveScrollAnchor("README.md");

        Assert.AreEqual(FileListViewMode.LargeIcons, workspace.ActiveTab.ViewMode);
        Assert.AreEqual("lastWriteTimeUtc", workspace.ActiveTab.SortColumn);
        Assert.AreEqual("descending", workspace.ActiveTab.SortDirection);
        Assert.AreEqual("README.md", workspace.ActiveTab.ScrollAnchorName);
    }

    [TestMethod]
    public void Empty_restored_tabs_and_close_last_tab_keep_a_safe_default_active_tab()
    {
        var workspace = NavigationWorkspace.FromRestoredTabs([], activeTabIndex: 0, defaultPath: @"C:\Users\alice");

        Assert.AreEqual(1, workspace.Tabs.Count);
        Assert.AreEqual(0, workspace.ActiveTabIndex);
        Assert.AreEqual(@"C:\Users\alice", workspace.ActiveTab.Path);

        workspace.CloseTab(workspace.ActiveTab.Id);

        Assert.AreEqual(1, workspace.Tabs.Count);
        Assert.AreEqual(0, workspace.ActiveTabIndex);
        Assert.AreEqual(@"C:\Users\alice", workspace.ActiveTab.Path);
    }
}
