using VeloFile.App.ViewModels;
using VeloFile.App.Windowing;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.FileAssociations;
using VeloFile.Core.Foundation;
using VeloFile.Core.Listing;
using VeloFile.Core.Operations;
using VeloFile.Core.Persistence;
using VeloFile.Core.Preview;
using VeloFile.Core.Search;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Terminal;
using VeloFile.Core.Visibility;
using VeloFile.Windows.Clipboard;
using VeloFile.Windows.FileSystem;
using VeloFile.Windows.Preview;
using VeloFile.Windows.Shell;
using VeloFile.Windows.ShellExecute;
using VeloFile.Windows.Storage;
using VeloFile.Windows.Terminal;
using VeloFile.Windows.Windowing;

namespace VeloFile.App;

public static class AppCompositionRoot
{
    public static AppShellViewModel CreateShellViewModel(IShellDispatcher? shellDispatcher = null)
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
        var fileOperationService = new FileOperationService(new WindowsShellFileOperationAdapter());
        var previewController = new PreviewController(
            WindowsPreviewProviderFactory.CreateDefault(),
            new PreviewMetadataProvider(),
            diagnostics: diagnostics,
            pathRedactor: CreatePathRedactor(appDataRoot));
        var thumbnailController = new ThumbnailController(
            new WindowsThumbnailProvider(),
            PreviewTimeoutPolicy.Default);
        var terminalLaunchService = new TerminalLaunchService(
            new TerminalDiscoveryService(new WindowsTerminalTargetSource()),
            new TerminalWorkingDirectoryProbe(pathProbe),
            new WindowsTerminalProcessLauncher(),
            diagnostics: diagnostics);
        var fileAssociationLaunchService = new FileAssociationLaunchService(new WindowsFileAssociationLauncher());
        return new AppShellViewModel(
            startupState,
            new WindowsClipboardTextWriter(),
            listingCoordinator,
            searchService,
            fileOperationService,
            previewController,
            thumbnailController,
            shellDispatcher,
            terminalLaunchService,
            fileAssociationLaunchService);
    }

    public static IWindowPlacementApplier CreateWindowPlacementApplier()
    {
        return new WinUiWindowPlacementApplier();
    }

    private static IReadOnlyList<DriveEntry> ReadDriveEntries()
    {
        return new WindowsDriveEntrySource().GetDrives();
    }

    private static PathRedactor CreatePathRedactor(string appDataRoot)
    {
        var saltPath = Path.Combine(appDataRoot, "diagnostics", "path-redaction-salt.bin");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(saltPath)!);
            if (File.Exists(saltPath))
            {
                var existing = File.ReadAllBytes(saltPath);
                if (existing.Length >= 16)
                {
                    return new PathRedactor(existing);
                }
            }

            var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            File.WriteAllBytes(saltPath, salt);
            return new PathRedactor(salt);
        }
        catch
        {
            return new PathRedactor(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }
    }

    private sealed class TerminalWorkingDirectoryProbe : IWorkingDirectoryProbe
    {
        private readonly IPathExistenceProbe _pathExistenceProbe;

        public TerminalWorkingDirectoryProbe(IPathExistenceProbe pathExistenceProbe)
        {
            _pathExistenceProbe = pathExistenceProbe;
        }

        public bool Exists(string path)
        {
            return _pathExistenceProbe.Exists(path);
        }
    }
}
