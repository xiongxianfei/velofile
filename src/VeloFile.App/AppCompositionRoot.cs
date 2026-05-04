using VeloFile.App.ViewModels;
using VeloFile.App.Windowing;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Foundation;
using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Search;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Visibility;
using VeloFile.Windows.Clipboard;
using VeloFile.Windows.FileSystem;
using VeloFile.Windows.Storage;
using VeloFile.Windows.Windowing;

namespace VeloFile.App;

public static class AppCompositionRoot
{
    public static AppShellViewModel CreateShellViewModel()
    {
        var initialState = new AppBootstrapper().CreateInitialState();
        var appDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProductIdentity.Name);
        var stateRoot = Path.Combine(appDataRoot, "state");
        var diagnostics = new LocalDiagnosticLogStore(Path.Combine(appDataRoot, "diagnostics"), DiagnosticRetentionPolicy.Default);
        var storage = new WindowsDurableDocumentStorage();

        var sessionRepository = new DurableDocumentRepository<SessionStatePayload>(
            Path.Combine(stateRoot, "session.json"),
            SessionStateDocumentCodec.Instance,
            storage,
            () => SessionStatePayload.Empty,
            diagnostics);
        var settingsRepository = new DurableDocumentRepository<SettingsStatePayload>(
            Path.Combine(stateRoot, "settings.json"),
            SettingsStateDocumentCodec.Instance,
            storage,
            () => SettingsStatePayload.Default,
            diagnostics);
        var favoritesRepository = new DurableDocumentRepository<FavoritesStatePayload>(
            Path.Combine(stateRoot, "favorites.json"),
            FavoritesStateDocumentCodec.Instance,
            storage,
            () => FavoritesStatePayload.Empty,
            diagnostics);
        var recentLocationsRepository = new DurableDocumentRepository<RecentLocationsStatePayload>(
            Path.Combine(stateRoot, "recentLocations.json"),
            RecentLocationsStateDocumentCodec.Instance,
            storage,
            () => RecentLocationsStatePayload.Empty,
            diagnostics);

        var session = sessionRepository.Read().Payload;
        var settings = settingsRepository.Read().Payload;
        var favorites = favoritesRepository.Read().Payload;
        var recentLocations = recentLocationsRepository.Read().Payload;

        var defaultLaunchPathProvider = new DefaultLaunchPathProvider();
        var pathProbe = new FileSystemPathExistenceProbe();
        var monitorPlacementResolver = new MonitorWindowPlacementResolver(new WindowsMonitorLayoutSource());
        var startupService = new AppShellStartupService(
            new SessionRestoreService(
                pathProbe,
                monitorPlacementResolver,
                new FileSystemScrollAnchorResolver(),
                new CrashRecoverySignal(() => diagnostics.HasRepeatedCrashMarkers("startup", threshold: 1)),
                defaultLaunchPathProvider),
            defaultLaunchPathProvider,
            pathProbe,
            diagnostics,
            () => DateTimeOffset.UtcNow,
            new DurableSettingsStateWriter(settingsRepository, "1.0.0-dev", () => DateTimeOffset.UtcNow));

        var startupState = startupService.CreateStartupState(new AppShellStartupInput(
            initialState.WindowTitle,
            session,
            settings,
            favorites,
            recentLocations,
            ReadDriveEntries()));

        diagnostics.RecordLastActionMarker("startup", "session", DateTimeOffset.UtcNow);
        var folderEntrySource = new WindowsFolderEntrySource();
        var listingCoordinator = new FolderListingCoordinator(
            new FolderListingService(folderEntrySource));
        var searchService = new RecursiveSearchService(folderEntrySource);
        return new AppShellViewModel(startupState, new WindowsClipboardTextWriter(), listingCoordinator, searchService);
    }

    public static IWindowPlacementApplier CreateWindowPlacementApplier()
    {
        return new WinUiWindowPlacementApplier();
    }

    private static IReadOnlyList<DriveEntry> ReadDriveEntries()
    {
        return new WindowsDriveEntrySource().GetDrives();
    }
}
