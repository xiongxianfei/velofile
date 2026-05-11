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
    IReadOnlyList<UiFixtureRow> Rows,
    UiFixturePresentationState PresentationState);

public sealed record UiFixtureRow(
    string Id,
    UiFixtureRowState State,
    ListedFileItem Item,
    ThumbnailState Thumbnail)
{
    public string FullPath => Item.FullPath;
}

public sealed record UiFixturePresentationState(
    IReadOnlyList<string> SelectedRowIds,
    string? FocusedRowId,
    string? SelectedFocusedRowId,
    IReadOnlyList<string> MultiSelectedRowIds,
    string? InitialKeyboardFocusTarget,
    IReadOnlyDictionary<string, string> FullPathByRowId)
{
    public static UiFixturePresentationState Empty { get; } =
        new([], null, null, [], null, new Dictionary<string, string>(StringComparer.Ordinal));

    public IReadOnlyList<string> AllSelectedRowIds =>
        SelectedRowIds
            .Concat(MultiSelectedRowIds)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    public bool HasTargets =>
        AllSelectedRowIds.Count > 0
        || FocusedRowId is not null
        || SelectedFocusedRowId is not null
        || InitialKeyboardFocusTarget is not null;

    public string? GetFullPath(string? rowId)
    {
        return rowId is not null && FullPathByRowId.TryGetValue(rowId, out var fullPath)
            ? fullPath
            : null;
    }
}

public sealed record UiFixtureShellState(
    AppShellViewModel ViewModel,
    UiFixturePresentationState PresentationState);

public sealed record UiFixturePresentationPlan(
    IReadOnlyList<FileListRowViewModel> SelectedRows,
    FileListRowViewModel? FocusedRow,
    FileListRowViewModel? SelectedFocusedRow);

public static class UiFixturePresentationPlanner
{
    public static UiFixturePresentationPlan Create(
        UiFixturePresentationState presentationState,
        IReadOnlyList<FileListRowViewModel> rows)
    {
        var rowsByPath = rows.ToDictionary(row => row.FullPath, StringComparer.OrdinalIgnoreCase);
        var selectedRows = presentationState.AllSelectedRowIds
            .Select(presentationState.GetFullPath)
            .Where(path => path is not null)
            .Select(path => rowsByPath.TryGetValue(path!, out var row) ? row : null)
            .Where(row => row is not null)
            .Cast<FileListRowViewModel>()
            .ToArray();
        var focusedRow = ResolveRow(presentationState, rowsByPath, presentationState.FocusedRowId);
        var selectedFocusedRow = ResolveRow(presentationState, rowsByPath, presentationState.SelectedFocusedRowId);

        return new UiFixturePresentationPlan(selectedRows, focusedRow, selectedFocusedRow);
    }

    private static FileListRowViewModel? ResolveRow(
        UiFixturePresentationState presentationState,
        IReadOnlyDictionary<string, FileListRowViewModel> rowsByPath,
        string? rowId)
    {
        var path = presentationState.GetFullPath(rowId);
        return path is not null && rowsByPath.TryGetValue(path, out var row)
            ? row
            : null;
    }
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
            EmptyFolderName => new UiFixtureDefinition(EmptyFolderName, "dark", "comfortable", "1440x900", [], UiFixturePresentationState.Empty),
            _ => null
        };
    }

    public static UiFixtureShellState CreateFileListV1ShellState(IShellDispatcher? shellDispatcher = null)
    {
        return CreateShellState(CreateFileListV1Definition(), shellDispatcher);
    }

    public static AppShellViewModel CreateFileListV1ViewModel()
    {
        return CreateFileListV1ShellState().ViewModel;
    }

    public static UiFixtureShellState CreateEmptyFolderShellState(IShellDispatcher? shellDispatcher = null)
    {
        return CreateShellState(new UiFixtureDefinition(EmptyFolderName, "dark", "comfortable", "1440x900", [], UiFixturePresentationState.Empty), shellDispatcher);
    }

    public static AppShellViewModel CreateEmptyFolderViewModel()
    {
        return CreateEmptyFolderShellState().ViewModel;
    }

    public static UiFixtureShellState CreateShellState(string fixtureName, IShellDispatcher? shellDispatcher = null)
    {
        var fixture = GetFixture(fixtureName)
            ?? throw new ArgumentException("Fixture name is not allowlisted.", nameof(fixtureName));
        return CreateShellState(fixture, shellDispatcher);
    }

    public static AppShellViewModel CreateViewModel(string fixtureName)
    {
        return CreateShellState(fixtureName).ViewModel;
    }

    private static UiFixtureShellState CreateShellState(UiFixtureDefinition fixture, IShellDispatcher? shellDispatcher)
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

        var viewModel = new AppShellViewModel(
            startupState,
            listingCoordinator: listingCoordinator,
            thumbnailController: thumbnailController,
            shellDispatcher: shellDispatcher,
            viewportItemCount: 100);

        return new UiFixtureShellState(viewModel, fixture.PresentationState);
    }

    private static UiFixtureDefinition CreateFileListV1Definition()
    {
        var rows = new[]
        {
            Row("row-document", UiFixtureRowState.Normal, "Document.pdf", "Document.pdf", FileSystemEntryKind.File, FileAttributes.Normal, "PDF", length: 192_000),
            Row("row-src-folder", UiFixtureRowState.Folder, "src", "src", FileSystemEntryKind.Directory, FileAttributes.Directory, "DIR"),
            Row("row-report-selected", UiFixtureRowState.Selected, "selected-report.docx", "selected-report.docx", FileSystemEntryKind.File, FileAttributes.Normal, "DOC", length: 81_920),
            Row("row-keyboard-focus", UiFixtureRowState.Focused, "keyboard-focus.md", "keyboard-focus.md", FileSystemEntryKind.File, FileAttributes.Normal, "MD", length: 12_288),
            Row("row-selected-focused", UiFixtureRowState.SelectedFocused, "selected-focused.xlsx", "selected-focused.xlsx", FileSystemEntryKind.File, FileAttributes.Normal, "XLS", length: 44_032),
            Row("row-multi-a", UiFixtureRowState.MultiSelected, "multi-selected-a.txt", "multi-selected-a.txt", FileSystemEntryKind.File, FileAttributes.Normal, "TXT", length: 4_096),
            Row("row-multi-b", UiFixtureRowState.MultiSelected, "multi-selected-b.txt", "multi-selected-b.txt", FileSystemEntryKind.File, FileAttributes.Normal, "TXT", length: 4_608),
            Row("row-hidden-env", UiFixtureRowState.Hidden, ".env", ".env", FileSystemEntryKind.File, FileAttributes.Hidden, "ENV", length: 512),
            Row("row-protected-system", UiFixtureRowState.ProtectedSystem, "system.ini", "system.ini", FileSystemEntryKind.File, FileAttributes.Hidden | FileAttributes.System, "SYS", length: 2_048),
            Row("row-thumbnail-fallback", UiFixtureRowState.ThumbnailFallback, "preview-timeout.png", "preview-timeout.png", FileSystemEntryKind.File, FileAttributes.Normal, "PNG", length: 735_232),
            Row("row-long-name", UiFixtureRowState.LongName, "Very long filename with spaces and extension - final final v3 copy.pdf", "Very long filename with spaces and extension - final final v3 copy.pdf", FileSystemEntryKind.File, FileAttributes.Normal, "PDF", length: 1_240_000),
            Row("row-metadata-heavy", UiFixtureRowState.MetadataHeavy, "invoice.pdf.exe", "invoice.pdf.exe", FileSystemEntryKind.File, FileAttributes.Normal, "EXE", length: 98_304)
        };

        return new UiFixtureDefinition(FileListV1Name, "dark", "comfortable", "1440x900", rows, CreateFileListV1PresentationState(rows));
    }

    private static UiFixturePresentationState CreateFileListV1PresentationState(IReadOnlyList<UiFixtureRow> rows)
    {
        return new UiFixturePresentationState(
            SelectedRowIds: ["row-report-selected", "row-selected-focused"],
            FocusedRowId: "row-selected-focused",
            SelectedFocusedRowId: "row-selected-focused",
            MultiSelectedRowIds: ["row-multi-a", "row-multi-b"],
            InitialKeyboardFocusTarget: "FileListSurface",
            FullPathByRowId: rows.ToDictionary(row => row.Id, row => row.FullPath, StringComparer.Ordinal));
    }

    private static UiFixtureRow Row(
        string id,
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
            id,
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
