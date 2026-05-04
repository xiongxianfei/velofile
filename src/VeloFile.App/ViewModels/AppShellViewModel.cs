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
    private readonly BuiltInCommandRegistry _commandRegistry;
    private readonly KeyboardCommandRouter _keyboardCommandRouter;
    private readonly ClipboardCommandService _clipboardCommands;
    private IReadOnlyList<ListedFileItem> _fileItems = [];

    public AppShellViewModel(AppShellStartupState startupState, IClipboardTextWriter? clipboardWriter = null)
    {
        CommandSurface = startupState.CommandSurface;
        WindowPlacementResolution = startupState.WindowPlacementResolution;
        _commandRegistry = BuiltInCommandRegistry.CreateDefault();
        _keyboardCommandRouter = KeyboardCommandRouter.CreateDefault();
        _clipboardCommands = new ClipboardCommandService(clipboardWriter ?? NoOpClipboardTextWriter.Instance);
    }

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
        return CommandSurface.SubmitPath(path);
    }

    public PathSubmissionResult ActivateSidebarTarget(ShellNavigationTarget target)
    {
        return CommandSurface.ActivateSidebarTarget(target);
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
        return CommandSurface.NavigateBack();
    }

    public bool NavigateForward()
    {
        return CommandSurface.NavigateForward();
    }

    public bool NavigateToParent()
    {
        return CommandSurface.NavigateToParent();
    }

    public void RefreshActiveTab()
    {
        CommandSurface.RefreshActiveTab();
    }

    public void NewTab()
    {
        CommandSurface.NewTab();
    }

    public void DuplicateActiveTab()
    {
        CommandSurface.DuplicateActiveTab();
    }

    public void CloseActiveTab()
    {
        CommandSurface.CloseActiveTab();
    }

    public void ReopenClosedTab()
    {
        CommandSurface.ReopenClosedTab();
    }

    public void SwitchToTab(int index)
    {
        CommandSurface.SwitchToTab(index);
    }

    public void SwitchNextTab()
    {
        CommandSurface.SwitchNextTab();
    }

    public void SwitchPreviousTab()
    {
        CommandSurface.SwitchPreviousTab();
    }

    public void SetShowHiddenFiles(bool show)
    {
        CommandSurface.SetShowHiddenFiles(show);
    }

    public void SetShowFileExtensions(bool show)
    {
        CommandSurface.SetShowFileExtensions(show);
    }

    public VisibilityChangeStatus SetShowProtectedOperatingSystemFiles(bool show, bool confirmed)
    {
        return CommandSurface.SetShowProtectedOperatingSystemFiles(show, confirmed);
    }

    public void StartFresh()
    {
        CommandSurface.StartFresh();
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
        SelectedFileItems = items;
    }

    private CommandContext CreateCommandContext(bool canPaste)
    {
        return CommandContext.ForSelection(ActivePath, SelectedFileItems, canPaste);
    }

    private sealed class NoOpClipboardTextWriter : IClipboardTextWriter
    {
        public static NoOpClipboardTextWriter Instance { get; } = new();

        public void SetText(string text)
        {
        }
    }
}
