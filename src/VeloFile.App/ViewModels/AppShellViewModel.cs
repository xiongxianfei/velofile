using VeloFile.Core.Foundation;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Visibility;

namespace VeloFile.App.ViewModels;

public sealed class AppShellViewModel
{
    public AppShellViewModel(AppShellStartupState startupState)
    {
        CommandSurface = startupState.CommandSurface;
        WindowPlacement = startupState.WindowPlacement;
    }

    public AppShellCommandSurface CommandSurface { get; }

    public WindowPlacementState? WindowPlacement { get; }

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
}
