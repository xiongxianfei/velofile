using VeloFile.Core.Foundation;
using VeloFile.Core.Commands;
using VeloFile.Core.DragDrop;
using VeloFile.Core.Filtering;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Operations;
using VeloFile.Core.Persistence;
using VeloFile.Core.Preview;
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
    private readonly DragDropActionResolver _dragDropActionResolver = new();
    private readonly CurrentFolderFilterService _filterService = new();
    private readonly FolderListingCoordinator? _listingCoordinator;
    private readonly IRecursiveSearchService? _recursiveSearchService;
    private readonly FileOperationService? _fileOperationService;
    private readonly PreviewController? _previewController;
    private readonly int _viewportItemCount;
    private IReadOnlyList<ListedFileItem> _activeListingItems = [];
    private IReadOnlyList<ListedFileItem> _fileItems = [];
    private FolderListingRequest? _activeListingRequest;
    private CancellationTokenSource? _recursiveSearchCancellation;
    private int _recursiveSearchGeneration;
    private ListedFileItem? _pendingRenameItem;
    private MutationListingTarget? _pendingRenameMutationTarget;
    private MutationListingTarget? _pendingPermanentDeleteMutationTarget;
    private PendingFileTransfer? _pendingFileTransfer;
    private MutationListingTarget? _pendingFileTransferMutationTarget;
    private string? _fileOperationRefreshWarning;

    public AppShellViewModel(
        AppShellStartupState startupState,
        IClipboardTextWriter? clipboardWriter = null,
        FolderListingCoordinator? listingCoordinator = null,
        IRecursiveSearchService? recursiveSearchService = null,
        FileOperationService? fileOperationService = null,
        PreviewController? previewController = null,
        int viewportItemCount = DefaultViewportItemCount)
    {
        CommandSurface = startupState.CommandSurface;
        WindowPlacementResolution = startupState.WindowPlacementResolution;
        _commandRegistry = BuiltInCommandRegistry.CreateDefault();
        _keyboardCommandRouter = KeyboardCommandRouter.CreateDefault();
        _clipboardCommands = new ClipboardCommandService(clipboardWriter ?? NoOpClipboardTextWriter.Instance);
        _listingCoordinator = listingCoordinator;
        _recursiveSearchService = recursiveSearchService;
        _fileOperationService = fileOperationService;
        _previewController = previewController;
        _viewportItemCount = viewportItemCount;
        if (_fileOperationService is not null)
        {
            _fileOperationService.StateChanged += (_, _) => ShellStateChanged?.Invoke(this, EventArgs.Empty);
        }
        if (_previewController is not null)
        {
            _previewController.StateChanged += (_, _) => ShellStateChanged?.Invoke(this, EventArgs.Empty);
        }

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

    public FileOperationState FileOperation => _fileOperationService?.State ?? FileOperationState.Idle;

    public PermanentDeleteConfirmationRequest? PendingPermanentDeleteConfirmation =>
        _fileOperationService?.PendingPermanentDeleteConfirmation;

    public FileOperationConflict? PendingFileOperationConflict =>
        _fileOperationService?.PendingConflict;

    public bool CanPasteFileOperation => _pendingFileTransfer is not null;

    public ListedFileItem? PendingRenameItem => _pendingRenameItem;

    public bool RenamePending => IsRenameActive;

    public bool IsRenameActive => _pendingRenameItem is not null;

    public string PendingRenameText { get; private set; } = "";

    public string? RenameError { get; private set; }

    public bool CanCommitRename => IsRenameActive && IsValidRenameText(PendingRenameText);

    public bool CanCancelFileOperation => _fileOperationService?.CanCancelCurrentOperation ?? false;

    public string FileOperationStatusText => FormatFileOperationStatus();

    public DropActionResolution CurrentDropAction { get; private set; } = DropActionResolution.None("drop-not-started");

    public bool DropActionIndicatorVisible => CurrentDropAction.CanDrop || DropFailureVisible(CurrentDropAction.ReasonCode);

    public string DropActionIndicatorText => CurrentDropAction.IndicatorText;

    public bool IsPreviewPaneOpen { get; private set; }

    public PreviewState Preview => _previewController?.State ?? PreviewState.Empty;

    public PreviewStatus PreviewStatus => Preview.Status;

    public string PreviewStatusText => FormatPreviewStatus();

    public string PreviewContentText => FormatPreviewContentText();

    public int? CurrentPdfPageNumber => Preview.Content?.PdfPageArtifact?.PageNumber;

    public int? PdfPageCount => Preview.Content?.PdfPageArtifact?.PageCount;

    public bool CanNavigatePdfPages => (PdfPageCount ?? 0) > 1;

    public bool IsPdfPreview => Preview.Content?.Kind is PreviewContentKind.Pdf;

    public bool CanRequestPreviousPdfPage =>
        PreviewStatus is PreviewStatus.Success
        && CurrentPdfPageNumber is > 1;

    public bool CanRequestNextPdfPage =>
        PreviewStatus is PreviewStatus.Success
        && CurrentPdfPageNumber is int currentPage
        && PdfPageCount is int pageCount
        && currentPage < pageCount;

    public IReadOnlyList<PreviewMetadataField> PreviewMetadataFields => Preview.Metadata?.Fields() ?? [];

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
        ClearPreviewSelection();
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void SwitchNextTab()
    {
        CommandSurface.SwitchNextTab();
        ClearPreviewSelection();
        ClearSearchForPathChange();
        RefreshActiveListing();
    }

    public void SwitchPreviousTab()
    {
        CommandSurface.SwitchPreviousTab();
        ClearPreviewSelection();
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
        ClearPreviewSelection();
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TogglePreviewPane()
    {
        IsPreviewPaneOpen = !IsPreviewPaneOpen;
        if (!IsPreviewPaneOpen)
        {
            _previewController?.Clear();
        }
        else
        {
            UpdatePreviewForSelection();
        }

        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool RequestPdfPage(int pageNumber)
    {
        if (_previewController?.RequestPreviewPage(pageNumber) is true)
        {
            ShellStateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }

    public bool RequestPreviousPdfPage()
    {
        return CurrentPdfPageNumber is int pageNumber
            && CanRequestPreviousPdfPage
            && RequestPdfPage(pageNumber - 1);
    }

    public bool RequestNextPdfPage()
    {
        return CurrentPdfPageNumber is int pageNumber
            && CanRequestNextPdfPage
            && RequestPdfPage(pageNumber + 1);
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
        _ = ExecuteBuiltInCommandAsync(commandId);
    }

    public async Task ExecuteBuiltInCommandAsync(VeloFileCommandId commandId)
    {
        switch (commandId)
        {
            case VeloFileCommandId.CopyPath:
                _clipboardCommands.CopyPath(SelectedFileItems);
                break;
            case VeloFileCommandId.CopyName:
                _clipboardCommands.CopyName(SelectedFileItems);
                break;
            case VeloFileCommandId.Copy:
                StageFileTransfer(FileOperationKind.Copy);
                break;
            case VeloFileCommandId.Cut:
                StageFileTransfer(FileOperationKind.Move);
                break;
            case VeloFileCommandId.Paste:
                await PasteStagedFileTransferAsync().ConfigureAwait(false);
                break;
            case VeloFileCommandId.Refresh:
                RefreshActiveTab();
                break;
            case VeloFileCommandId.ParentFolder:
                NavigateToParent();
                break;
            case VeloFileCommandId.Delete:
                if (_fileOperationService is not null && SelectedFileItems.Count > 0)
                {
                    var mutationTarget = CaptureMutationListingTarget();
                    _fileOperationRefreshWarning = null;
                    await _fileOperationService.DeleteToRecycleBinAsync(SelectedFileItems).ConfigureAwait(false);
                    _pendingPermanentDeleteMutationTarget = FileOperation.Status is FileOperationStatus.WaitingForConfirmation
                        ? mutationTarget
                        : null;
                    await RefreshActiveListingAfterCompletedMutationAsync(mutationTarget).ConfigureAwait(false);
                }

                break;
            case VeloFileCommandId.PermanentDelete:
                if (_fileOperationService is not null && SelectedFileItems.Count > 0)
                {
                    _pendingPermanentDeleteMutationTarget = CaptureMutationListingTarget();
                    _fileOperationRefreshWarning = null;
                    _fileOperationService.RequestPermanentDelete(SelectedFileItems, PermanentDeleteReason.UserGesture);
                }

                break;
            case VeloFileCommandId.Rename:
                _pendingRenameItem = SelectedFileItems.Count == 1 ? SelectedFileItems[0] : null;
                _pendingRenameMutationTarget = _pendingRenameItem is null ? null : CaptureMutationListingTarget();
                PendingRenameText = _pendingRenameItem?.Name ?? "";
                RenameError = null;
                ShellStateChanged?.Invoke(this, EventArgs.Empty);
                break;
            default:
                break;
        }
    }

    public async Task ResolveFileOperationConflictAsync(FileOperationConflictChoice choice)
    {
        if (_fileOperationService is null)
        {
            return;
        }

        var mutationTarget = _pendingFileTransferMutationTarget ?? CaptureMutationListingTarget();
        _fileOperationRefreshWarning = null;
        await _fileOperationService.ResolveConflictAsync(choice).ConfigureAwait(false);
        if (FileOperation.Status is FileOperationStatus.Completed)
        {
            _pendingFileTransfer = null;
        }

        if (FileOperation.Status is not FileOperationStatus.WaitingForConflict)
        {
            _pendingFileTransferMutationTarget = null;
        }

        await RefreshActiveListingAfterCompletedMutationAsync(mutationTarget).ConfigureAwait(false);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateDropAction(
        IReadOnlyList<DropItem> items,
        DragDropKeyModifiers modifiers,
        DropVolumeRelationship volumeRelationship,
        bool supportsShortcut = true)
    {
        CurrentDropAction = ResolveDropAction(items, modifiers, volumeRelationship, supportsShortcut);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearDropAction()
    {
        CurrentDropAction = DropActionResolution.None("drop-cleared");
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ReportDropFailure(string reasonCode)
    {
        CurrentDropAction = DropActionResolution.None(reasonCode);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task CommitDropAsync(
        IReadOnlyList<DropItem> items,
        DragDropKeyModifiers modifiers,
        DropVolumeRelationship volumeRelationship,
        bool supportsShortcut = true)
    {
        CurrentDropAction = ResolveDropAction(items, modifiers, volumeRelationship, supportsShortcut);
        if (!CurrentDropAction.CanDrop || _fileOperationService is null)
        {
            ShellStateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        var mutationTarget = CaptureMutationListingTarget();
        var operationItems = items.Select(ToListedFileItem).ToArray();
        _fileOperationRefreshWarning = null;

        if (CurrentDropAction.Action is DropAction.Move)
        {
            await _fileOperationService.MoveAsync(operationItems, ActivePath).ConfigureAwait(false);
        }
        else if (CurrentDropAction.Action is DropAction.Copy)
        {
            await _fileOperationService.CopyAsync(operationItems, ActivePath).ConfigureAwait(false);
        }
        else if (CurrentDropAction.Action is DropAction.Shortcut)
        {
            await _fileOperationService.CreateShortcutsAsync(operationItems, ActivePath).ConfigureAwait(false);
        }

        CurrentDropAction = DropActionResolution.None("drop-completed");
        await RefreshActiveListingAfterCompletedMutationAsync(mutationTarget).ConfigureAwait(false);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StageFileTransfer(FileOperationKind kind)
    {
        if (SelectedFileItems.Count == 0)
        {
            return;
        }

        _pendingFileTransfer = new PendingFileTransfer(kind, SelectedFileItems.ToArray());
        _pendingFileTransferMutationTarget = null;
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private DropActionResolution ResolveDropAction(
        IReadOnlyList<DropItem> items,
        DragDropKeyModifiers modifiers,
        DropVolumeRelationship volumeRelationship,
        bool supportsShortcut = true)
    {
        return _dragDropActionResolver.Resolve(new DragDropRequest(items, ActivePath, volumeRelationship, modifiers, supportsShortcut));
    }

    private static bool DropFailureVisible(string? reasonCode)
    {
        return !string.IsNullOrWhiteSpace(reasonCode)
            && reasonCode is not "drop-cleared" and not "drop-not-started" and not "drop-completed";
    }

    private static ListedFileItem ToListedFileItem(DropItem item)
    {
        return new ListedFileItem(
            item.FullPath,
            item.Name,
            item.Name,
            item.Kind,
            Length: null,
            LastWriteTimeUtc: null,
            FileAttributes.Normal,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private async Task PasteStagedFileTransferAsync()
    {
        if (_fileOperationService is null || _pendingFileTransfer is null)
        {
            return;
        }

        var transfer = _pendingFileTransfer;
        var mutationTarget = CaptureMutationListingTarget();
        _fileOperationRefreshWarning = null;

        if (transfer.Kind is FileOperationKind.Move)
        {
            await _fileOperationService.MoveAsync(transfer.Items, ActivePath).ConfigureAwait(false);
        }
        else
        {
            await _fileOperationService.CopyAsync(transfer.Items, ActivePath).ConfigureAwait(false);
        }

        if (FileOperation.Status is FileOperationStatus.Completed)
        {
            _pendingFileTransfer = null;
        }

        _pendingFileTransferMutationTarget = FileOperation.Status is FileOperationStatus.WaitingForConflict
            ? mutationTarget
            : null;

        await RefreshActiveListingAfterCompletedMutationAsync(mutationTarget).ConfigureAwait(false);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetPendingRenameText(string? targetName)
    {
        PendingRenameText = targetName ?? "";
        if (RenameError is not null && IsValidRenameText(PendingRenameText))
        {
            RenameError = null;
        }

        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task CommitPendingRenameAsync()
    {
        await CommitPendingRenameAsync(PendingRenameText).ConfigureAwait(false);
    }

    public async Task CommitPendingRenameAsync(string targetName)
    {
        if (_fileOperationService is null || _pendingRenameItem is null)
        {
            return;
        }

        if (!IsValidRenameText(targetName))
        {
            RenameError = "The file name is invalid.";
            ShellStateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        var item = _pendingRenameItem;
        var mutationTarget = _pendingRenameMutationTarget ?? CaptureMutationListingTarget();
        _pendingRenameItem = null;
        _pendingRenameMutationTarget = null;
        PendingRenameText = "";
        RenameError = null;
        _fileOperationRefreshWarning = null;
        await _fileOperationService.RenameAsync(item, targetName).ConfigureAwait(false);
        await RefreshActiveListingAfterCompletedMutationAsync(mutationTarget).ConfigureAwait(false);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CancelPendingRename()
    {
        _pendingRenameItem = null;
        _pendingRenameMutationTarget = null;
        PendingRenameText = "";
        RenameError = null;
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CancelFileOperation()
    {
        _fileOperationService?.CancelCurrentOperation();
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task ConfirmPermanentDeleteAsync(bool confirm)
    {
        if (_fileOperationService is null)
        {
            return;
        }

        var mutationTarget = _pendingPermanentDeleteMutationTarget ?? CaptureMutationListingTarget();
        _pendingPermanentDeleteMutationTarget = null;
        _fileOperationRefreshWarning = null;
        await _fileOperationService.ConfirmPermanentDeleteAsync(confirm).ConfigureAwait(false);
        await RefreshActiveListingAfterCompletedMutationAsync(mutationTarget).ConfigureAwait(false);
        ShellStateChanged?.Invoke(this, EventArgs.Empty);
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
        UpdatePreviewForSelection();
    }

    private CommandContext CreateCommandContext(bool canPaste)
    {
        return CommandContext.ForSelection(ActivePath, SelectedFileItems, canPaste || CanPasteFileOperation);
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

    private async Task RefreshActiveListingAfterCompletedMutationAsync(MutationListingTarget mutationTarget)
    {
        if (_listingCoordinator is null || FileOperation.Status is not FileOperationStatus.Completed)
        {
            return;
        }

        var operation = _listingCoordinator.StartLoad(
            mutationTarget.TabId,
            mutationTarget.Path,
            new FolderListingOptions(_viewportItemCount, VisibilitySettings));
        var result = await operation.Completion.ConfigureAwait(false);
        if (!result.Applied)
        {
            return;
        }

        if (result.State.Status is FolderListingStatus.Ready or FolderListingStatus.Empty)
        {
            _fileOperationRefreshWarning = null;
            if (IsActiveMutationTarget(mutationTarget))
            {
                ApplyListingState(result.State);
            }

            return;
        }

        _fileOperationRefreshWarning = "Could not refresh the folder";
        if (IsActiveMutationTarget(mutationTarget))
        {
            ShellStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private MutationListingTarget CaptureMutationListingTarget()
    {
        return new MutationListingTarget(ActiveTab.Id, ActivePath);
    }

    private bool IsActiveMutationTarget(MutationListingTarget mutationTarget)
    {
        return string.Equals(ActiveTab.Id, mutationTarget.TabId, StringComparison.Ordinal)
            && string.Equals(ActivePath, mutationTarget.Path, StringComparison.OrdinalIgnoreCase);
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
        ClearPreviewSelection();
    }

    private void UpdatePreviewForSelection()
    {
        if (!IsPreviewPaneOpen)
        {
            return;
        }

        _previewController?.StartPreview(SelectedFileItems.Count == 1 ? SelectedFileItems[0] : null);
    }

    private void ClearPreviewSelection()
    {
        _previewController?.Clear();
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

    private string FormatFileOperationStatus()
    {
        var state = FileOperation;
        if (state.Status is FileOperationStatus.Idle)
        {
            return "";
        }

        var operation = state.Kind switch
        {
            FileOperationKind.Copy => "Copy",
            FileOperationKind.Move => "Move",
            FileOperationKind.CreateShortcut => "Create shortcut",
            FileOperationKind.Rename => "Rename",
            FileOperationKind.RecycleBinDelete => "Recycle Bin delete",
            FileOperationKind.PermanentDelete => "Permanent delete",
            _ => "File operation"
        };

        var statusText = state.Status switch
        {
            FileOperationStatus.Running => $"{operation} running",
            FileOperationStatus.Cancelling => $"{operation} cancelling",
            FileOperationStatus.WaitingForConfirmation when !string.IsNullOrWhiteSpace(state.ReasonCode) => $"{operation} waiting for confirmation: {state.ReasonCode}",
            FileOperationStatus.WaitingForConfirmation => $"{operation} waiting for confirmation",
            FileOperationStatus.WaitingForConflict when !string.IsNullOrWhiteSpace(state.ReasonCode) => $"{operation} waiting for conflict resolution: {state.ReasonCode}",
            FileOperationStatus.WaitingForConflict => $"{operation} waiting for conflict resolution",
            FileOperationStatus.Completed => $"{operation} completed",
            FileOperationStatus.Cancelled => $"{operation} cancelled",
            FileOperationStatus.Failed when !string.IsNullOrWhiteSpace(state.ReasonCode) => $"{operation} failed: {state.ReasonCode}",
            FileOperationStatus.Failed => $"{operation} failed",
            _ => ""
        };

        if (state.UndoEligibility.CanUndo)
        {
            statusText = $"{statusText}. Undo available";
        }

        if (state.Status is FileOperationStatus.Running or FileOperationStatus.Cancelling or FileOperationStatus.Cancelled
            && state.Progress.TotalItemCount > 0)
        {
            statusText = $"{statusText} ({state.Progress.CompletedItemCount} of {state.Progress.TotalItemCount})";
        }

        return string.IsNullOrWhiteSpace(_fileOperationRefreshWarning)
            ? statusText
            : $"{statusText}. {_fileOperationRefreshWarning}.";
    }

    private string FormatPreviewStatus()
    {
        return Preview.Status switch
        {
            PreviewStatus.Loading => "Preview loading",
            PreviewStatus.Success => "Preview ready",
            PreviewStatus.Unsupported when !string.IsNullOrWhiteSpace(Preview.ReasonCode) => $"Preview unsupported: {Preview.ReasonCode}",
            PreviewStatus.Unsupported => "Preview unsupported",
            PreviewStatus.Failed when !string.IsNullOrWhiteSpace(Preview.ReasonCode) => $"Preview failed: {Preview.ReasonCode}",
            PreviewStatus.Failed => "Preview failed",
            _ => ""
        };
    }

    private string FormatPreviewContentText()
    {
        return Preview.Content switch
        {
            { Kind: PreviewContentKind.Text } content => content.TextContent ?? "",
            { ImageArtifact: { } image } => $"Image {image.PixelWidth} x {image.PixelHeight}",
            { PdfPageArtifact: { PageCount: > 0 } pdf } => $"PDF page {pdf.PageNumber} of {pdf.PageCount}",
            { PdfPageArtifact: { } pdf } => $"PDF page {pdf.PageNumber}",
            _ => ""
        };
    }

    private static bool IsValidRenameText(string? targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return false;
        }

        var trimmed = targetName.Trim();
        return trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && !trimmed.Contains('\\')
            && !trimmed.Contains('/');
    }

    private sealed class NoOpClipboardTextWriter : IClipboardTextWriter
    {
        public static NoOpClipboardTextWriter Instance { get; } = new();

        public void SetText(string text)
        {
        }
    }

    private sealed record MutationListingTarget(string TabId, string Path);

    private sealed record PendingFileTransfer(
        FileOperationKind Kind,
        IReadOnlyList<ListedFileItem> Items);
}
