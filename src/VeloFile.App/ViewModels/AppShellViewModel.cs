using VeloFile.Core.Foundation;
using VeloFile.Core.Commands;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
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
    private readonly FolderListingCoordinator? _listingCoordinator;
    private readonly int _viewportItemCount;
    private IReadOnlyList<ListedFileItem> _fileItems = [];
    private FolderListingRequest? _activeListingRequest;

    public AppShellViewModel(
        AppShellStartupState startupState,
        IClipboardTextWriter? clipboardWriter = null,
        FolderListingCoordinator? listingCoordinator = null,
        int viewportItemCount = DefaultViewportItemCount)
    {
        CommandSurface = startupState.CommandSurface;
        WindowPlacementResolution = startupState.WindowPlacementResolution;
        _commandRegistry = BuiltInCommandRegistry.CreateDefault();
        _keyboardCommandRouter = KeyboardCommandRouter.CreateDefault();
        _clipboardCommands = new ClipboardCommandService(clipboardWriter ?? NoOpClipboardTextWriter.Instance);
        _listingCoordinator = listingCoordinator;
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

    public IReadOnlyList<ListedFileItem> SelectedFileItems { get; private set; } = [];

    public PathSubmissionResult SubmitPath(string path)
    {
        var result = CommandSurface.SubmitPath(path);
        if (result.Accepted)
        {
            RefreshActiveListing(forceReload: true);
        }

        return result;
    }

    public PathSubmissionResult ActivateSidebarTarget(ShellNavigationTarget target)
    {
        var result = CommandSurface.ActivateSidebarTarget(target);
        if (result.Accepted)
        {
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
            RefreshActiveListing();
        }

        return navigated;
    }

    public bool NavigateForward()
    {
        var navigated = CommandSurface.NavigateForward();
        if (navigated)
        {
            RefreshActiveListing();
        }

        return navigated;
    }

    public bool NavigateToParent()
    {
        var navigated = CommandSurface.NavigateToParent();
        if (navigated)
        {
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
        RefreshActiveListing();
    }

    public void DuplicateActiveTab()
    {
        CommandSurface.DuplicateActiveTab();
        RefreshActiveListing();
    }

    public void CloseActiveTab()
    {
        var closedTabId = ActiveTab.Id;
        CommandSurface.CloseActiveTab();
        _listingCoordinator?.CloseTab(closedTabId);
        RefreshActiveListing();
    }

    public void ReopenClosedTab()
    {
        if (CommandSurface.ReopenClosedTab() is not null)
        {
            RefreshActiveListing();
        }
    }

    public void SwitchToTab(int index)
    {
        CommandSurface.SwitchToTab(index);
        RefreshActiveListing();
    }

    public void SwitchNextTab()
    {
        CommandSurface.SwitchNextTab();
        RefreshActiveListing();
    }

    public void SwitchPreviousTab()
    {
        CommandSurface.SwitchPreviousTab();
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
        RefreshActiveListing(forceReload: true);
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
        _fileItems = items;
        SelectedFileItems = [];
    }

    public void SetSelectedFileItems(IReadOnlyList<ListedFileItem> items)
    {
        var selectedPaths = items
            .Select(item => item.FullPath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        SelectedFileItems = _fileItems
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

    private sealed class NoOpClipboardTextWriter : IClipboardTextWriter
    {
        public static NoOpClipboardTextWriter Instance { get; } = new();

        public void SetText(string text)
        {
        }
    }
}
