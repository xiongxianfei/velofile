namespace VeloFile.App.Tests;

[TestClass]
[TestCategory("Navigation")]
[TestCategory("Sidebar")]
[TestCategory("Session")]
public sealed class AppShellContractTests
{
    [TestMethod]
    public void Main_window_shell_exposes_navigation_sidebar_tabs_breadcrumb_and_file_view_regions()
    {
        var xaml = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);

        StringAssert.Contains(xaml, "x:Name=\"TabStrip\"");
        StringAssert.Contains(xaml, "x:Name=\"SidebarPane\"");
        StringAssert.Contains(xaml, "x:Name=\"BreadcrumbPathBar\"");
        StringAssert.Contains(xaml, "x:Name=\"RawPathBox\"");
        StringAssert.Contains(xaml, "x:Name=\"FileViewModeSelector\"");
        StringAssert.Contains(xaml, "x:Name=\"FileListSurface\"");
        StringAssert.Contains(xaml, "x:Name=\"MissingLocationState\"");
        StringAssert.Contains(xaml, "x:Name=\"PathEntryFailureState\"");
        StringAssert.Contains(xaml, "x:Name=\"VisibilityControls\"");
    }

    [TestMethod]
    public void Main_window_shell_declares_keyboard_paths_for_tabs_navigation_and_visibility()
    {
        var xaml = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);

        StringAssert.Contains(xaml, "<KeyboardAccelerator");
        StringAssert.Contains(xaml, "Key=\"T\"");
        StringAssert.Contains(xaml, "Key=\"W\"");
        StringAssert.Contains(xaml, "Key=\"Tab\"");
        StringAssert.Contains(xaml, "Key=\"L\"");
        StringAssert.Contains(xaml, "Key=\"P\"");
        StringAssert.Contains(xaml, "AccessKey=\"H\"");
        StringAssert.Contains(xaml, "AccessKey=\"S\"");
        StringAssert.Contains(xaml, "AccessKey=\"E\"");
    }

    [TestMethod]
    public void Main_window_shell_wires_navigation_controls_to_code_behind_command_routes()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "Click=\"BackButton_Click\"");
        StringAssert.Contains(xaml, "KeyDown=\"RawPathBox_KeyDown\"");
        StringAssert.Contains(xaml, "ItemClick=\"SidebarLocationsList_ItemClick\"");
        StringAssert.Contains(xaml, "SelectionChanged=\"TabList_SelectionChanged\"");
        StringAssert.Contains(xaml, "Click=\"NewTabButton_Click\"");
        StringAssert.Contains(xaml, "Invoked=\"NewTabAccelerator_Invoked\"");

        StringAssert.Contains(codeBehind, "ViewModel.NavigateBack()");
        StringAssert.Contains(codeBehind, "ViewModel.SubmitPath");
        StringAssert.Contains(codeBehind, "ViewModel.ActivateSidebarTarget");
        StringAssert.Contains(codeBehind, "ViewModel.SwitchToTab");
        StringAssert.Contains(codeBehind, "ViewModel.NewTab()");
        StringAssert.Contains(codeBehind, "NewTabAccelerator_Invoked");
    }

    [TestMethod]
    public void App_launch_uses_composition_root_instead_of_hardcoded_main_window_state()
    {
        var repoRoot = FindRepoRoot();
        var appCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "App.xaml.cs").FullName);
        var compositionCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);

        StringAssert.Contains(appCode, "AppCompositionRoot.CreateShellViewModel()");
        StringAssert.Contains(compositionCode, "DurableDocumentRepository<SessionStatePayload>");
        StringAssert.Contains(compositionCode, "DurableDocumentRepository<SettingsStatePayload>");
        StringAssert.Contains(compositionCode, "DurableDocumentRepository<FavoritesStatePayload>");
        StringAssert.Contains(compositionCode, "DurableDocumentRepository<RecentLocationsStatePayload>");
        StringAssert.Contains(compositionCode, "SessionRestoreService");
        StringAssert.Contains(compositionCode, "WindowsDurableDocumentStorage");
        StringAssert.Contains(compositionCode, "LocalDiagnosticLogStore");
    }

    [TestMethod]
    public void App_launch_uses_real_monitor_resolver_and_applies_restored_window_placement()
    {
        var repoRoot = FindRepoRoot();
        var appCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "App.xaml.cs").FullName);
        var mainWindowCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);
        var compositionCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);

        StringAssert.Contains(appCode, "AppCompositionRoot.CreateWindowPlacementApplier()");
        StringAssert.Contains(mainWindowCode, "windowPlacementApplier.Apply(this, ViewModel.WindowPlacement)");
        StringAssert.Contains(compositionCode, "WindowsMonitorLayoutSource");
        StringAssert.Contains(compositionCode, "MonitorWindowPlacementResolver");
        Assert.IsFalse(compositionCode.Contains("new PassThroughMonitorPlacementResolver()", StringComparison.Ordinal));
    }

    private static DirectoryInfo FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "VeloFile.sln")))
            {
                return current;
            }

            current = current.Parent;
        }

        Assert.Fail("Could not find repository root from test output directory.");
        throw new InvalidOperationException("Could not find repository root from test output directory.");
    }
}
