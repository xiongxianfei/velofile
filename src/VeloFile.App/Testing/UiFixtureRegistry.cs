using VeloFile.App.ViewModels;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Preview;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

namespace VeloFile.App.Testing;

public enum UiFixtureRowState
{
    Normal,
    Folder,
    Selected,
    Focused,
    SelectedFocused,
    MultiSelected,
    Hidden,
    ProtectedSystem,
    ThumbnailFallback,
    LongName,
    MetadataHeavy
}

public sealed record UiFixtureDefinition(
    string Name,
    string Theme,
    string Density,
    string Viewport,
    IReadOnlyList<UiFixtureRow> Rows);

public sealed record UiFixtureRow(
    UiFixtureRowState State,
    ListedFileItem Item,
    ThumbnailState Thumbnail)
{
    public string FullPath => Item.FullPath;
}

public static class UiFixtureRegistry
{
    public const string FileListV1Name = "file-list-v1";

    public const string EmptyFolderName = "file-list-empty-folder";

    public const string FixtureRoot = @"C:\VeloFileFixture";

    public static IReadOnlyList<string> AllowlistedFixtureNames { get; } =
        [FileListV1Name, EmptyFolderName];

    public static bool IsAllowlisted(string fixtureName)
    {
        return AllowlistedFixtureNames.Contains(fixtureName, StringComparer.Ordinal);
    }

    public static UiFixtureDefinition? GetFixture(string fixtureName)
    {
        return fixtureName switch
        {
            FileListV1Name => CreateFileListV1Definition(),
            EmptyFolderName => new UiFixtureDefinition(EmptyFolderName, "dark", "comfortable", "1440x900", []),
            _ => null
        };
    }

    public static AppShellViewModel CreateFileListV1ViewModel()
    {
        return CreateViewModel(CreateFileListV1Definition());
    }

    public static AppShellViewModel CreateEmptyFolderViewModel()
    {
        return CreateViewModel(new UiFixtureDefinition(EmptyFolderName, "dark", "comfortable", "1440x900", []));
    }

    public static AppShellViewModel CreateViewModel(string fixtureName)
    {
        var fixture = GetFixture(fixtureName)
            ?? throw new ArgumentException("Fixture name is not allowlisted.", nameof(fixtureName));
        return CreateViewModel(fixture);
    }

    private static AppShellViewModel CreateViewModel(UiFixtureDefinition fixture)
    {
        var workspace = NavigationWorkspace.Create(FixtureRoot);
        var sidebar = SidebarStateService.Create(
            FavoritesStatePayload.Empty,
            RecentLocationsStatePayload.Empty,
            drives: []);
        var commandSurface = new AppShellCommandSurface(
            "VeloFile",
            workspace,
            sidebar,
            VisibilitySettingsService.FromPayload(SettingsStatePayload.Default with
            {
                ShowHiddenFiles = true,
                ShowProtectedOperatingSystemFiles = true
            }),
            CrashRecoveryState.None,
            new FixtureDefaultLaunchPathProvider(),
            new FixturePathExistenceProbe(),
            NoOpSettingsStateWriter.Instance,
            utcNow: () => new DateTimeOffset(2026, 5, 11, 0, 0, 0, TimeSpan.Zero),
            diagnostics: new CollectingDiagnosticSink());
        var startupState = new AppShellStartupState(
            "VeloFile",
            commandSurface,
            WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement));
        var listingCoordinator = new FolderListingCoordinator(
            new FolderListingService(new FixtureFolderEntrySource(fixture.Rows.Select(row => ToSnapshot(row.Item)).ToArray())));
        var thumbnailController = new ThumbnailController(
            new FixtureThumbnailProvider(fixture.Rows.ToDictionary(row => row.Item.FullPath, row => row.Thumbnail, StringComparer.OrdinalIgnoreCase)),
            PreviewTimeoutPolicy.Default);

        return new AppShellViewModel(
            startupState,
            listingCoordinator: listingCoordinator,
            thumbnailController: thumbnailController,
            viewportItemCount: 100);
    }

    private static UiFixtureDefinition CreateFileListV1Definition()
    {
        var rows = new[]
        {
            Row(UiFixtureRowState.Normal, "Document.pdf", "Document.pdf", FileSystemEntryKind.File, FileAttributes.Normal, "PDF", length: 192_000),
            Row(UiFixtureRowState.Folder, "src", "src", FileSystemEntryKind.Directory, FileAttributes.Directory, "DIR"),
            Row(UiFixtureRowState.Selected, "selected-report.docx", "selected-report.docx", FileSystemEntryKind.File, FileAttributes.Normal, "DOC", length: 81_920),
            Row(UiFixtureRowState.Focused, "keyboard-focus.md", "keyboard-focus.md", FileSystemEntryKind.File, FileAttributes.Normal, "MD", length: 12_288),
            Row(UiFixtureRowState.SelectedFocused, "selected-focused.xlsx", "selected-focused.xlsx", FileSystemEntryKind.File, FileAttributes.Normal, "XLS", length: 44_032),
            Row(UiFixtureRowState.MultiSelected, "multi-selected-a.txt", "multi-selected-a.txt", FileSystemEntryKind.File, FileAttributes.Normal, "TXT", length: 4_096),
            Row(UiFixtureRowState.MultiSelected, "multi-selected-b.txt", "multi-selected-b.txt", FileSystemEntryKind.File, FileAttributes.Normal, "TXT", length: 4_608),
            Row(UiFixtureRowState.Hidden, ".env", ".env", FileSystemEntryKind.File, FileAttributes.Hidden, "ENV", length: 512),
            Row(UiFixtureRowState.ProtectedSystem, "system.ini", "system.ini", FileSystemEntryKind.File, FileAttributes.Hidden | FileAttributes.System, "SYS", length: 2_048),
            Row(UiFixtureRowState.ThumbnailFallback, "preview-timeout.png", "preview-timeout.png", FileSystemEntryKind.File, FileAttributes.Normal, "PNG", length: 735_232),
            Row(UiFixtureRowState.LongName, "Very long filename with spaces and extension - final final v3 copy.pdf", "Very long filename with spaces and extension - final final v3 copy.pdf", FileSystemEntryKind.File, FileAttributes.Normal, "PDF", length: 1_240_000),
            Row(UiFixtureRowState.MetadataHeavy, "invoice.pdf.exe", "invoice.pdf.exe", FileSystemEntryKind.File, FileAttributes.Normal, "EXE", length: 98_304)
        };

        return new UiFixtureDefinition(FileListV1Name, "dark", "comfortable", "1440x900", rows);
    }

    private static UiFixtureRow Row(
        UiFixtureRowState state,
        string name,
        string displayName,
        FileSystemEntryKind kind,
        FileAttributes attributes,
        string thumbnailText,
        long? length = null)
    {
        var fullPath = Path.Combine(FixtureRoot, name);
        var item = new ListedFileItem(
            fullPath,
            name,
            displayName,
            kind,
            length,
            new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            attributes,
            IsHidden: attributes.HasFlag(FileAttributes.Hidden),
            IsProtectedOperatingSystemFile: attributes.HasFlag(FileAttributes.Hidden) && attributes.HasFlag(FileAttributes.System),
            IsVisuallyDimmed: attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System));

        return new UiFixtureRow(
            state,
            item,
            ThumbnailState.GenericIcon(ThumbnailArtifact.GenericIcon(thumbnailText), "ui-fixture"));
    }

    private static FileSystemEntrySnapshot ToSnapshot(ListedFileItem item)
    {
        return new FileSystemEntrySnapshot(
            item.FullPath,
            item.Name,
            item.Kind,
            item.Length,
            item.LastWriteTimeUtc,
            item.Attributes,
            item.CreationTimeUtc,
            item.LastAccessTimeUtc);
    }

    private sealed class FixtureFolderEntrySource : IFolderEntrySource
    {
        private readonly IReadOnlyList<FileSystemEntrySnapshot> _entries;

        public FixtureFolderEntrySource(IReadOnlyList<FileSystemEntrySnapshot> entries)
        {
            _entries = entries;
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(string path, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var entry in _entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
                await Task.Yield();
            }
        }
    }

    private sealed class FixtureThumbnailProvider : IThumbnailProvider
    {
        private readonly IReadOnlyDictionary<string, ThumbnailState> _states;

        public FixtureThumbnailProvider(IReadOnlyDictionary<string, ThumbnailState> states)
        {
            _states = states;
        }

        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var state = _states.TryGetValue(item.FullPath, out var configuredState)
                ? configuredState
                : ThumbnailState.GenericIcon(ThumbnailArtifact.GenericIcon("FILE"), "ui-fixture");

            var result = state.Status switch
            {
                ThumbnailStatus.Ready when state.Artifact is not null => ThumbnailProviderResult.Success(state.Artifact),
                ThumbnailStatus.GenericIcon when state.Artifact is not null => ThumbnailProviderResult.GenericIcon(state.Artifact, state.ReasonCode ?? "ui-fixture"),
                ThumbnailStatus.Failed => ThumbnailProviderResult.Failed(state.ReasonCode ?? "ui-fixture"),
                _ => ThumbnailProviderResult.GenericIcon(ThumbnailArtifact.GenericIcon("FILE"), "ui-fixture")
            };

            return ValueTask.FromResult(result);
        }
    }

    private sealed class FixtureDefaultLaunchPathProvider : IDefaultLaunchPathProvider
    {
        public string GetDefaultLaunchPath()
        {
            return FixtureRoot;
        }
    }

    private sealed class FixturePathExistenceProbe : IPathExistenceProbe
    {
        public bool Exists(string path)
        {
            return string.Equals(path, FixtureRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}
