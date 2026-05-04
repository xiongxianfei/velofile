using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;

namespace VeloFile.Core.Navigation;

public sealed class NavigationEntryPointService
{
    private readonly NavigationWorkspace _workspace;
    private readonly SidebarStateService _sidebar;
    private readonly Func<DateTimeOffset> _utcNow;

    public NavigationEntryPointService(
        NavigationWorkspace workspace,
        SidebarStateService sidebar,
        Func<DateTimeOffset> utcNow)
    {
        _workspace = workspace;
        _sidebar = sidebar;
        _utcNow = utcNow;
    }

    public void OpenTypedPath(string path)
    {
        NavigateAndRecord(path);
    }

    public void OpenPastedPath(string path)
    {
        NavigateAndRecord(path);
    }

    public void OpenBreadcrumbSegment(BreadcrumbSegment segment)
    {
        NavigateAndRecord(segment.FullPath);
    }

    public void OpenSidebarLocation(string path)
    {
        NavigateAndRecord(path);
    }

    public void OpenFavorite(PinnedLocationState favorite)
    {
        NavigateAndRecord(favorite.Path);
    }

    public void OpenRecent(RecentLocationState recentLocation)
    {
        NavigateAndRecord(recentLocation.Path);
    }

    public void OpenDrive(DriveEntry drive)
    {
        NavigateAndRecord(drive.RootPath);
    }

    private void NavigateAndRecord(string path)
    {
        _workspace.NavigateActive(path);
        _sidebar.RecordRecent(_workspace.ActiveTab.Path, _utcNow());
    }
}
