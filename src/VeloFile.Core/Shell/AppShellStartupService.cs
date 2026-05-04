using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Shell;

public sealed record AppShellStartupInput(
    string WindowTitle,
    SessionStatePayload Session,
    SettingsStatePayload Settings,
    FavoritesStatePayload Favorites,
    RecentLocationsStatePayload RecentLocations,
    IReadOnlyList<DriveEntry> Drives);

public sealed record AppShellStartupState(
    string WindowTitle,
    AppShellCommandSurface CommandSurface,
    WindowPlacementState? WindowPlacement);

public sealed class AppShellStartupService
{
    private readonly SessionRestoreService _sessionRestoreService;
    private readonly IDefaultLaunchPathProvider _defaultLaunchPathProvider;
    private readonly IPathExistenceProbe _pathExistenceProbe;
    private readonly IDiagnosticSink _diagnostics;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly ISettingsStateWriter _settingsStateWriter;

    public AppShellStartupService(
        SessionRestoreService sessionRestoreService,
        IDefaultLaunchPathProvider defaultLaunchPathProvider,
        IPathExistenceProbe pathExistenceProbe,
        IDiagnosticSink diagnostics,
        Func<DateTimeOffset> utcNow,
        ISettingsStateWriter? settingsStateWriter = null)
    {
        _sessionRestoreService = sessionRestoreService;
        _defaultLaunchPathProvider = defaultLaunchPathProvider;
        _pathExistenceProbe = pathExistenceProbe;
        _diagnostics = diagnostics;
        _utcNow = utcNow;
        _settingsStateWriter = settingsStateWriter ?? NoOpSettingsStateWriter.Instance;
    }

    public AppShellStartupState CreateStartupState(AppShellStartupInput input)
    {
        var restore = _sessionRestoreService.Restore(
            input.Session,
            input.Favorites,
            input.RecentLocations,
            input.Settings,
            input.Drives);

        var commandSurface = new AppShellCommandSurface(
            input.WindowTitle,
            restore.Workspace,
            SidebarStateService.Create(input.Favorites, input.RecentLocations, input.Drives),
            VisibilitySettingsService.FromPayload(input.Settings),
            restore.CrashRecovery,
            _defaultLaunchPathProvider,
            _pathExistenceProbe,
            _settingsStateWriter,
            _utcNow,
            _diagnostics);

        return new AppShellStartupState(input.WindowTitle, commandSurface, restore.WindowPlacement);
    }
}
