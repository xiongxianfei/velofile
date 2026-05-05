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
        StringAssert.Contains(compositionCode, "new FolderListingCoordinator");
        StringAssert.Contains(compositionCode, "new RecursiveSearchService");
        StringAssert.Contains(compositionCode, "new WindowsFolderEntrySource()");
    }

    [TestMethod]
    public void App_launch_uses_real_monitor_resolver_and_applies_restored_window_placement()
    {
        var repoRoot = FindRepoRoot();
        var appCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "App.xaml.cs").FullName);
        var mainWindowCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);
        var compositionCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);

        StringAssert.Contains(appCode, "AppCompositionRoot.CreateWindowPlacementApplier()");
        StringAssert.Contains(mainWindowCode, "RootShell.MinWidth = WindowPlacementPolicy.Default.MinimumRestorableWidth");
        StringAssert.Contains(mainWindowCode, "RootShell.MinHeight = WindowPlacementPolicy.Default.MinimumRestorableHeight");
        StringAssert.Contains(mainWindowCode, "windowPlacementApplier.Apply(this, ViewModel.WindowPlacementResolution)");
        StringAssert.Contains(compositionCode, "WindowsMonitorLayoutSource");
        StringAssert.Contains(compositionCode, "MonitorWindowPlacementResolver");
        Assert.IsFalse(compositionCode.Contains("new PassThroughMonitorPlacementResolver()", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Main_window_shell_exposes_built_in_context_menu_and_file_command_accelerators()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"BuiltInFileContextMenu\"");
        StringAssert.Contains(xaml, "Opening=\"BuiltInFileContextMenu_Opening\"");
        StringAssert.Contains(xaml, "Text=\"Open\"");
        StringAssert.Contains(xaml, "Text=\"Open with\"");
        StringAssert.Contains(xaml, "Text=\"Cut\"");
        StringAssert.Contains(xaml, "Text=\"Copy\"");
        StringAssert.Contains(xaml, "Text=\"Paste\"");
        StringAssert.Contains(xaml, "Text=\"Rename\"");
        StringAssert.Contains(xaml, "Text=\"Delete\"");
        StringAssert.Contains(xaml, "Text=\"Properties\"");
        StringAssert.Contains(xaml, "Text=\"Copy path\"");
        StringAssert.Contains(xaml, "Text=\"Copy name\"");
        StringAssert.Contains(xaml, "Text=\"Open terminal here\"");
        Assert.IsFalse(xaml.Contains("ShellExtension", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(xaml.Contains("Show more options", StringComparison.OrdinalIgnoreCase));

        StringAssert.Contains(xaml, "Key=\"A\" Modifiers=\"Control\"");
        StringAssert.Contains(xaml, "Key=\"F2\"");
        StringAssert.Contains(xaml, "Key=\"Delete\"");
        StringAssert.Contains(xaml, "Key=\"C\" Modifiers=\"Control,Shift\"");
        StringAssert.Contains(xaml, "Key=\"N\" Modifiers=\"Control,Shift\"");
        StringAssert.Contains(codeBehind, "AppFileCommandAcceleratorRouter");
        StringAssert.Contains(codeBehind, "RefreshFileContextMenuAvailability");
        StringAssert.Contains(codeBehind, "ViewModel.IsBuiltInCommandAvailable");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public void Operations_shell_exposes_operation_status_and_permanent_delete_confirmation_routes()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);
        var compositionCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"FileOperationStatusText\"");
        StringAssert.Contains(xaml, "x:Name=\"CancelFileOperationButton\"");
        StringAssert.Contains(xaml, "Click=\"CancelFileOperationButton_Click\"");
        StringAssert.Contains(xaml, "x:Name=\"PermanentDeleteConfirmationPanel\"");
        StringAssert.Contains(xaml, "Click=\"ConfirmPermanentDeleteButton_Click\"");
        StringAssert.Contains(xaml, "Click=\"CancelPermanentDeleteButton_Click\"");
        StringAssert.Contains(xaml, "x:Name=\"FileOperationConflictPanel\"");
        StringAssert.Contains(xaml, "x:Name=\"SkipConflictButton\"");
        StringAssert.Contains(xaml, "x:Name=\"ReplaceConflictButton\"");
        StringAssert.Contains(xaml, "x:Name=\"KeepBothConflictButton\"");
        StringAssert.Contains(codeBehind, "ViewModel.FileOperationStatusText");
        StringAssert.Contains(codeBehind, "ViewModel.CanCancelFileOperation");
        StringAssert.Contains(codeBehind, "ViewModel.CancelFileOperation()");
        StringAssert.Contains(codeBehind, "ViewModel.PendingPermanentDeleteConfirmation");
        StringAssert.Contains(codeBehind, "ViewModel.ConfirmPermanentDeleteAsync(confirm: true)");
        StringAssert.Contains(codeBehind, "ViewModel.PendingFileOperationConflict");
        StringAssert.Contains(codeBehind, "ViewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Skip)");
        StringAssert.Contains(codeBehind, "ViewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Replace)");
        StringAssert.Contains(codeBehind, "ViewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.KeepBoth)");
        StringAssert.Contains(codeBehind, "Recycle Bin delete is unavailable.");
        StringAssert.Contains(compositionCode, "new FileOperationService");
        StringAssert.Contains(compositionCode, "new WindowsShellFileOperationAdapter()");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public void Operations_shell_exposes_rename_commit_and_cancel_routes()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"RenamePanel\"");
        StringAssert.Contains(xaml, "x:Name=\"RenameTextBox\"");
        StringAssert.Contains(xaml, "KeyDown=\"RenameTextBox_KeyDown\"");
        StringAssert.Contains(xaml, "Click=\"CommitRenameButton_Click\"");
        StringAssert.Contains(xaml, "Click=\"CancelRenameButton_Click\"");
        StringAssert.Contains(codeBehind, "ViewModel.SetPendingRenameText(RenameTextBox.Text)");
        StringAssert.Contains(codeBehind, "ViewModel.CommitPendingRenameAsync()");
        StringAssert.Contains(codeBehind, "ViewModel.CancelPendingRename()");
    }

    [TestMethod]
    public void Main_window_file_list_binds_real_items_and_maps_selection_to_listed_file_models()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"FileListSurface\"");
        StringAssert.Contains(xaml, "<ListView.ItemTemplate>");
        Assert.IsFalse(xaml.Contains("<ListViewItem Content=", StringComparison.Ordinal));
        StringAssert.Contains(codeBehind, "FileListSurface.ItemsSource = ViewModel.VisibleItems");
        StringAssert.Contains(codeBehind, "FileListSelectionMapper.ToListedFileItems(FileListSurface.SelectedItems, ViewModel.VisibleItems)");
        Assert.IsFalse(codeBehind.Contains("FileListSurface.SelectedItems.OfType<ListedFileItem>()", StringComparison.Ordinal));
    }

    [TestMethod]
    [TestCategory("Filtering")]
    [TestCategory("Search")]
    public void Main_window_shell_wires_current_filter_and_recursive_search_routes()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"CurrentFolderFilterBox\"");
        StringAssert.Contains(xaml, "TextChanged=\"CurrentFolderFilterBox_TextChanged\"");
        StringAssert.Contains(xaml, "x:Name=\"RecursiveSearchBox\"");
        StringAssert.Contains(xaml, "KeyDown=\"RecursiveSearchBox_KeyDown\"");
        StringAssert.Contains(xaml, "x:Name=\"RecursiveSearchButton\"");
        StringAssert.Contains(xaml, "Click=\"RecursiveSearchButton_Click\"");
        StringAssert.Contains(xaml, "x:Name=\"CancelSearchButton\"");
        StringAssert.Contains(xaml, "Click=\"CancelSearchButton_Click\"");
        StringAssert.Contains(xaml, "x:Name=\"ClearSearchButton\"");
        StringAssert.Contains(xaml, "Click=\"ClearSearchButton_Click\"");
        StringAssert.Contains(xaml, "x:Name=\"RecursiveSearchStatusText\"");
        StringAssert.Contains(xaml, "x:Name=\"SkippedLocationsList\"");

        StringAssert.Contains(codeBehind, "ViewModel.SetCurrentFolderFilter");
        StringAssert.Contains(codeBehind, "ViewModel.StartRecursiveSearch");
        StringAssert.Contains(codeBehind, "ViewModel.CancelRecursiveSearch");
        StringAssert.Contains(codeBehind, "ViewModel.ClearRecursiveSearch");
        StringAssert.Contains(codeBehind, "ViewModel.RecursiveSearch.CanCancel");
        StringAssert.Contains(codeBehind, "ViewModel.SearchSkippedLocations");
    }

    [TestMethod]
    public void Main_window_file_command_accelerators_use_focus_context_before_routing()
    {
        var codeBehind = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(codeBehind, "IKeyboardFocusContextProvider");
        StringAssert.Contains(codeBehind, "WinUiKeyboardFocusContextProvider");
        StringAssert.Contains(codeBehind, "AppFileCommandAcceleratorRouter");
        StringAssert.Contains(codeBehind, "InvokeFileListShortcut");
        StringAssert.Contains(codeBehind, "KeyboardRouteStatus.Routed");
        Assert.IsFalse(codeBehind.Contains("ViewModel.HandleFileListShortcut(gesture);", StringComparison.Ordinal));
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
