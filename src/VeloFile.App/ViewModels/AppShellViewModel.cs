using VeloFile.Core.Foundation;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

namespace VeloFile.App.ViewModels;

public sealed class AppShellViewModel
{
    private readonly NavigationEntryPointService _entryPoints;

    private AppShellViewModel(
        string windowTitle,
        NavigationWorkspace workspace,
        SidebarStateService sidebar,
        VisibilitySettingsService visibility)
    {
        WindowTitle = windowTitle;
        Workspace = workspace;
        Sidebar = sidebar;
        Visibility = visibility;
        _entryPoints = new NavigationEntryPointService(Workspace, Sidebar, () => DateTimeOffset.UtcNow);
    }

    public string WindowTitle { get; }

    public NavigationWorkspace Workspace { get; }

    public SidebarStateService Sidebar { get; }

    public VisibilitySettingsService Visibility { get; }

    public static AppShellViewModel Create(InitialAppState initialState)
    {
        var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(initialPath))
        {
            initialPath = @"C:\";
        }

        return new AppShellViewModel(
            initialState.WindowTitle,
            NavigationWorkspace.Create(initialPath),
            SidebarStateService.Create(
                FavoritesStatePayload.Empty,
                RecentLocationsStatePayload.Empty,
                drives:
                [
                    new DriveEntry(
                        Name: @"C:\",
                        RootPath: @"C:\",
                        DriveType: DriveType.Fixed,
                        IsReady: false,
                        AvailableFreeSpaceBytes: null,
                        TotalSizeBytes: null,
                        HintStatus: DriveHintStatus.NotRequested)
                ]),
            VisibilitySettingsService.FromPayload(SettingsStatePayload.Default));
    }

    public void OpenTypedPath(string path)
    {
        _entryPoints.OpenTypedPath(path);
    }

    public void OpenPastedPath(string path)
    {
        _entryPoints.OpenPastedPath(path);
    }

    public void OpenBreadcrumbSegment(BreadcrumbSegment segment)
    {
        _entryPoints.OpenBreadcrumbSegment(segment);
    }

    public void OpenSidebarLocation(string path)
    {
        _entryPoints.OpenSidebarLocation(path);
    }

    public void OpenFavorite(PinnedLocationState favorite)
    {
        _entryPoints.OpenFavorite(favorite);
    }

    public void OpenRecent(RecentLocationState recentLocation)
    {
        _entryPoints.OpenRecent(recentLocation);
    }

    public void OpenDrive(DriveEntry drive)
    {
        _entryPoints.OpenDrive(drive);
    }
}
