using VeloFile.Core.Foundation;
using VeloFile.Core.Commands;
using VeloFile.Core.Filtering;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Search;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Visibility;

namespace VeloFile.App.ViewModels;

public sealed class AppShellViewModel
{
    private const int DefaultViewportItemCount = 500;
    private readonly BuiltInCommandRegistry _commandRegistry;
    private readonly KeyboardCommandRouter _keyboardCommandRouter;
    private readonly ClipboardCommandService _clipboardCommands;
    private readonly CurrentFolderFilterService _filterService = new();
    private readonly FolderListingCoordinator? _listingCoordinator;
    private readonly IRecursiveSearchService? _recursiveSearchService;
    private readonly int _viewportItemCount;
    private IReadOnlyList<ListedFileItem> _activeListingItems = [];
    private IReadOnlyList<ListedFileItem> _fileItems = [];
    private FolderListingRequest? _activeListingRequest;
    private CancellationTokenSource? _recursiveSearchCancellation;
    private int _recursiveSearchGeneration;

    public AppShellViewModel(
        AppShellStartupState startupState,
        IClipboardTextWriter? clipboardWriter = null,
        FolderListingCoordinator? listingCoordinator = null,
        IRecursiveSearchService? recursiveSearchService = null,
        int viewportItemCount = DefaultViewportItemCount)
    {
        CommandSurface = startupState.CommandSurface;
        WindowPlacementResolution = startupState.WindowPlacementResolution;
        _commandRegistry = BuiltInCommandRegistry.CreateDefault();
        _keyboardCommandRouter = KeyboardCommandRouter.CreateDefault();
        _clipboardCommands = new ClipboardCommandService(clipboardWriter ?? NoOpClipboardTextWriter.Instance);
        _listingCoordinator = listingCoordinator;
        _recursiveSearchService = recursiveSearchService;
        _viewportItemCount = viewportItemCount;
        RefreshActiveListing();
    }

    public event EventHandler? ShellStateChanged;

    public AppShellCommandSurface CommandSurface { get; }

    public WindowPlacementResolution WindowPlacementResolution { get; }

    public WindowPlacementState? WindowPlacement => WindowPlacementResolution.Placement;

    public string WindowTitle => CommandSurface.WindowTitle;

    public IReadOnlyList<NavigationTab> Tabs => CommandSurface.Tabs;

    public int ActiveTabIndex => CommandSurface.ActiveTabIndex;

    public NavigationTab ActiveTab => CommandSurface.ActiveTab;

    public string ActivePath => CommandSurface.ActivePath;

    public IReadOnlyList<BreadcrumbSegment> BreadcrumbSegments => CommandSurface.BreadcrumbSegments;

    public IReadOnlyList<ShellNavigationTarget> SidebarNavigationTargets => CommandSurface.SidebarNavigationTargets;

    public VisibilitySettings VisibilitySettings => CommandSurface.VisibilitySettings;

    public CrashRecoveryState CrashRecovery => CommandSurface.CrashRecovery;

    public bool MissingLocationVisible => CommandSurface.MissingLocationVisible;

    public string? MissingLocationPath => CommandSurface.MissingLocationPath;

    public bool PathEntryErrorVisible => CommandSurface.PathEntryErrorVisible;

    public PathEntryError? PathEntryError => CommandSurface.PathEntryError;

    public IReadOnlyList<ListedFileItem> FileItems => _fileItems;

    public IReadOnlyList<ListedFileItem> VisibleItems =>
        IsRecursiveSearchDisplayActive ? RecursiveSearch.Results : _fileItems;

    public IReadOnlyList<ListedFileItem> SelectedFileItems { get; private set; } = [];

    public string CurrentFolderFilterText { get; private set; } = "";

    public RecursiveSearchState RecursiveSearch { get; private set; } = RecursiveSearchState.NotStarted;

    public bool IsRecursiveSearchDisplayActive => RecursiveSearch.Status is not RecursiveSearchStatus.NotStarted;

    public IReadOnlyList<RecursiveSearchSkippedLocation> SearchSkippedLocations => RecursiveSearch.SkippedLocations;

    public bool SearchSkippedLocationsVisible => SearchSkippedLocations.Count > 0;

    public string RecursiveSearchStatusText => FormatRecursiveSearchStatus();

    public PathSubmissionResult SubmitPath(string path)
    {
        var result = CommandSurface.SubmitPath(path);
        if (result.Accepted)
        {
            ClearSearchForPathChange();
            RefreshActiveListing(forceReload: true);
        }

        return result;
    }

    public PathSubmissionResult ActivateSidebarTarget(ShellNavigationTarget target)
    {
        var result = CommandSurface.ActivateSidebarTarget(target);
        if (result.Accepted)
        {
            ClearSearchForPathChange();
            RefreshActiveListing(forceReload: true);
        }

        return result;
    }

    public void OpenBreadcrumbSegment(BreadcrumbSegment segment)
    {
        SubmitPath(segment.FullPath);
    }

    public void ClearPathEntryError()
    {
        CommandSurface.ClearPathEntryError();
    }

    public bool NavigateBack()
    {
        var navigated = CommandSurface.NavigateBack();
        if (navigated)
        {
            ClearSearchForPathChange();
            RefreshActiveListing();
        }

        return navigated;
    }

    public bool NavigateForward()
    {
        var navigated = CommandSurface.NavigateForward();
        if (navigated)
        {
            ClearSearchForPathChange();
            RefreshActiveListing();
        }

        return navigated;
    }

    public bool NavigateToParent()
    {
        var navigated = CommandSurface.NavigateToParent();
        if (navigated)
        {
            ClearSearchForPathChange();
            RefreshActiveListing(forceReload: true);
        }

        return navigated;
    }

    public void RefreshActiveTab()
    {
        CommandSurface.RefreshActiveTab();
        RefreshActiveListing(forceReload: true);
    }

    public void NewTab()
    {
        CommandSurface.NewTab();
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void DuplicateActiveTab()
    {
        CommandSurface.DuplicateActiveTab();
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void CloseActiveTab()
    {
        var closedTabId = ActiveTab.Id;
        CommandSurface.CloseActiveTab();
        _listingCoordinator?.CloseTab(closedTabId);
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void ReopenClosedTab()
    {
        if (CommandSurface.ReopenClosedTab() is not null)
        {
            ClearSearchForPathChange();
            RefreshActiveListing();
        }
    }

    public void SwitchToTab(int index)
    {
        CommandSurface.SwitchToTab(index);
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void SwitchNextTab()
    {
        CommandSurface.SwitchNextTab();
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void SwitchPreviousTab()
    {
        CommandSurface.SwitchPreviousTab();
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void SetShowHiddenFiles(bool show)
    {
        CommandSurface.SetShowHiddenFiles(show);
        RefreshActiveListing(forceReload: true);
    }

    public void SetShowFileExtensions(bool show)
    {
        CommandSurface.SetShowFileExtensions(show);
        RefreshActiveListing(forceReload: true);
    }

    public VisibilityChangeStatus SetShowProtectedOperatingSystemFiles(bool show, bool confirmed)
    {
        var status = CommandSurface.SetShowProtectedOperatingSystemFiles(show, confirmed);
        if (status is VisibilityChangeStatus.Applied)
        {
            RefreshActiveListing(forceReload: true);
        }

        return status;
    }

    public void StartFresh()
    {
        CommandSurface.StartFresh();
        ClearSearchForPathChange();
        RefreshActiveListing(forceReload: true);
    }

    public void SetCurrentFolderFilter(string? filterText)
    {
        CurrentFolderFilterText = filterText?.Trim() ?? "";
        ApplyCurrentFolderFilter();
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartRecursiveSearch(string query, int resultLimit = RecursiveSearchOptions.DefaultResultLimit)
    {
        if (_recursiveSearchService is null || string.IsNullOrWhiteSpace(query))
        {
            RecursiveSearch = RecursiveSearchState.NotStarted;
            ShellStateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        _recursiveSearchCancellation?.Cancel();
        var generation = ++_recursiveSearchGeneration;
        var cancellation = new CancellationTokenSource();
        _recursiveSearchCancellation = cancellation;
        var trimmedQuery = query.Trim();
        var options = new RecursiveSearchOptions(resultLimit, VisibilitySettings);
        RecursiveSearch = RecursiveSearchState.Running(ActivePath, trimmedQuery, resultLimit);
        SelectedFileItems = [];
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
        _ = CompleteRecursiveSearchAsync(generation, ActivePath, trimmedQuery, options, cancellation.Token);
    }

    public void CancelRecursiveSearch()
    {
        if (!RecursiveSearch.CanCancel)
        {
            return;
        }

        _recursiveSearchCancellation?.Cancel();
        _recursiveSearchCancellation = null;
        _recursiveSearchGeneration++;
        RecursiveSearch = RecursiveSearch.Cancelled();
        SelectedFileItems = [];
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearRecursiveSearch()
    {
        if (RecursiveSearch.Status is RecursiveSearchStatus.NotStarted)
        {
            return;
        }

        _recursiveSearchCancellation?.Cancel();
        _recursiveSearchCancellation = null;
        _recursiveSearchGeneration++;
        RecursiveSearch = RecursiveSearchState.NotStarted;
        SelectedFileItems = [];
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<BuiltInContextMenuItem> BuildFileContextMenu(bool canPaste)
    {
        return _commandRegistry.BuildContextMenu(CreateCommandContext(canPaste));
    }

    public bool IsBuiltInCommandAvailable(VeloFileCommandId commandId, bool canPaste)
    {
        return _commandRegistry.GetCommand(commandId).IsAvailable(CreateCommandContext(canPaste));
    }

    public KeyboardRouteResult HandleFileListShortcut(KeyGesture gesture, bool textInputHasFocus = false)
    {
        var route = _keyboardCommandRouter.Route(
            gesture,
            textInputHasFocus ? KeyboardCommandContext.TextInput : KeyboardCommandContext.FileList);

        if (route.Status is not KeyboardRouteStatus.Routed)
        {
            return route;
        }

        if (route.CommandId is { } commandId)
        {
            ExecuteBuiltInCommand(commandId);
        }

        return route;
    }

    public void ExecuteBuiltInCommand(VeloFileCommandId commandId)
    {
        switch (commandId)
        {
            case VeloFileCommandId.CopyPath:
                _clipboardCommands.CopyPath(SelectedFileItems);
                break;
            case VeloFileCommandId.CopyName:
                _clipboardCommands.CopyName(SelectedFileItems);
                break;
            case VeloFileCommandId.Refresh:
                RefreshActiveTab();
                break;
            case VeloFileCommandId.ParentFolder:
                NavigateToParent();
                break;
            default:
                break;
        }
    }

    public void SetFileItems(IReadOnlyList<ListedFileItem> items)
    {
        _activeListingItems = items;
        ApplyCurrentFolderFilter();
    }

    public void SetSelectedFileItems(IReadOnlyList<ListedFileItem> items)
    {
        var selectedPaths = items
            .Select(item => item.FullPath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        SelectedFileItems = VisibleItems
            .Where(item => selectedPaths.Contains(item.FullPath))
            .ToArray();
    }

    private CommandContext CreateCommandContext(bool canPaste)
    {
        return CommandContext.ForSelection(ActivePath, SelectedFileItems, canPaste);
    }

    private void RefreshActiveListing(bool forceReload = false)
    {
        if (_listingCoordinator is null)
        {
            return;
        }

        if (ActiveTab.LocationState is not NavigationTabLocationState.Available)
        {
            ApplyListingState(null);
            return;
        }

        if (!forceReload)
        {
            var existingState = _listingCoordinator.GetState(ActiveTab.Id);
            if (existingState is not null
                && string.Equals(existingState.Path, ActivePath, StringComparison.OrdinalIgnoreCase)
                && existingState.Status is FolderListingStatus.Ready or FolderListingStatus.Empty)
            {
                ApplyListingState(existingState);
                return;
            }
        }

        var operation = _listingCoordinator.StartLoad(
            ActiveTab.Id,
            ActivePath,
            new FolderListingOptions(_viewportItemCount, VisibilitySettings));
        _activeListingRequest = operation.Request;
        ApplyListingState(operation.InitialState);
        _ = CompleteListingAsync(operation);
    }

    private async Task CompleteListingAsync(FolderListingOperation operation)
    {
        var result = await operation.Completion.ConfigureAwait(false);
        if (!result.Applied || !IsActiveListingResult(result))
        {
            return;
        }

        ApplyListingState(result.State);
    }

    private bool IsActiveListingResult(FolderListingLoadResult result)
    {
        return _activeListingRequest == result.Request
            && string.Equals(ActiveTab.Id, result.Request.TabId, StringComparison.Ordinal);
    }

    private void ApplyListingState(FolderListingState? state)
    {
        SetFileItems(state?.Status is FolderListingStatus.Ready ? state.FirstViewport : []);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task CompleteRecursiveSearchAsync(
        int generation,
        string rootPath,
        string query,
        RecursiveSearchOptions options,
        CancellationToken cancellationToken)
    {
        if (_recursiveSearchService is null)
        {
            return;
        }

        await foreach (var update in _recursiveSearchService
            .SearchAsync(rootPath, query, options, cancellationToken)
            .ConfigureAwait(false))
        {
            if (!IsActiveSearch(generation, rootPath))
            {
                return;
            }

            RecursiveSearch = RecursiveSearch.Apply(update);
            SelectedFileItems = [];
            ShellStateChanged?.Invoke(this, EventArgs.Empty);
        }

        if (IsActiveSearch(generation, rootPath))
        {
            _recursiveSearchCancellation = null;
        }
    }

    private bool IsActiveSearch(int generation, string rootPath)
    {
        return generation == _recursiveSearchGeneration
            && string.Equals(ActivePath, rootPath, StringComparison.OrdinalIgnoreCase);
    }

    private void ClearSearchForPathChange()
    {
        if (RecursiveSearch.Status is RecursiveSearchStatus.NotStarted)
        {
            return;
        }

        _recursiveSearchCancellation?.Cancel();
        _recursiveSearchCancellation = null;
        _recursiveSearchGeneration++;
        RecursiveSearch = RecursiveSearchState.NotStarted;
        SelectedFileItems = [];
    }

    private void ApplyCurrentFolderFilter()
    {
        _fileItems = _filterService.Apply(_activeListingItems, CurrentFolderFilterText);
        SelectedFileItems = [];
    }

    private string FormatRecursiveSearchStatus()
    {
        if (RecursiveSearch.Status is RecursiveSearchStatus.NotStarted)
        {
            return "";
        }

        var resultText = RecursiveSearch.Status switch
        {
            RecursiveSearchStatus.Running => $"Searching... {RecursiveSearch.Results.Count} found",
            RecursiveSearchStatus.ResultLimitReached => $"Result limit reached: {RecursiveSearch.Results.Count} found; refine or start a new search",
            RecursiveSearchStatus.Completed => $"{RecursiveSearch.Results.Count} found",
            RecursiveSearchStatus.Cancelled => $"Cancelled: {RecursiveSearch.Results.Count} found",
            _ => $"{RecursiveSearch.Results.Count} found"
        };

        return RecursiveSearch.SkippedLocations.Count > 0
            ? $"{resultText} - {RecursiveSearch.SkippedLocations.Count} skipped locations"
            : resultText;
    }

    private sealed class NoOpClipboardTextWriter : IClipboardTextWriter
    {
        public static NoOpClipboardTextWriter Instance { get; } = new();

        public void SetText(string text)
        {
        }
    }
}
