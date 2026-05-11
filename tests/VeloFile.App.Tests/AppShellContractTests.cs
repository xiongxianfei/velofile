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

        StringAssert.Contains(appCode, "AppCompositionRoot.CreateShellViewModel(shellDispatcher)");
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
        StringAssert.Contains(xaml, "x:Name=\"TerminalTargetComboBox\"");
        StringAssert.Contains(xaml, "DropDownOpened=\"TerminalTargetComboBox_DropDownOpened\"");
        StringAssert.Contains(xaml, "SelectionChanged=\"TerminalTargetComboBox_SelectionChanged\"");
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
        StringAssert.Contains(codeBehind, "ViewModel.LoadTerminalTargetsAsync()");
        StringAssert.Contains(codeBehind, "ViewModel.SelectTerminalTarget");
        StringAssert.Contains(xaml, "DoubleTapped=\"FileListSurface_DoubleTapped\"");
        StringAssert.Contains(codeBehind, "ExecuteAvailableBuiltInCommand(VeloFileCommandId.Open)");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public void Operations_shell_exposes_operation_status_and_permanent_delete_confirmation_routes()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);
        var dragDropRouteCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "Input", "AppDragDropRoute.cs").FullName);
        var compositionCode = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"FileOperationStatusText\"");
        StringAssert.Contains(xaml, "x:Name=\"LaunchStatusText\"");
        StringAssert.Contains(xaml, "x:Name=\"DropActionIndicatorText\"");
        StringAssert.Contains(xaml, "AllowDrop=\"True\"");
        StringAssert.Contains(xaml, "DragOver=\"FileListSurface_DragOver\"");
        StringAssert.Contains(xaml, "DragLeave=\"FileListSurface_DragLeave\"");
        StringAssert.Contains(xaml, "Drop=\"FileListSurface_Drop\"");
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
        StringAssert.Contains(codeBehind, "ViewModel.LaunchStatusText");
        StringAssert.Contains(codeBehind, "AppDragDropRoute");
        StringAssert.Contains(codeBehind, "WinUiFileDropPayloadExtractor");
        StringAssert.Contains(dragDropRouteCode, "CommitDropAsync");
        StringAssert.Contains(codeBehind, "ViewModel.DropActionIndicatorText");
        StringAssert.Contains(codeBehind, "ViewModel.DropActionIndicatorVisible");
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
        StringAssert.Contains(compositionCode, "WindowsTerminalTargetSource");
        StringAssert.Contains(compositionCode, "WindowsTerminalProcessLauncher");
        StringAssert.Contains(compositionCode, "WindowsFileAssociationLauncher");
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
    [TestCategory("DragDrop")]
    public void DragDrop_winui_extractor_uses_all_or_nothing_storage_item_projection()
    {
        var repoRoot = FindRepoRoot();
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(codeBehind, "WinUiStorageItemDropPayloadProjection.ProjectPaths");
        Assert.IsFalse(
            codeBehind.Contains(".Where(path => !string.IsNullOrWhiteSpace(path))", StringComparison.Ordinal),
            "WinUI storage item drops must reject the whole payload when any item lacks a filesystem path.");
    }

    [TestMethod]
    [TestCategory("PreviewContract")]
    public void PreviewContract_main_window_preview_pane_is_wired_to_view_model_preview_contract_state()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"PreviewPane\"");
        StringAssert.Contains(xaml, "x:Name=\"PreviewStatusText\"");
        StringAssert.Contains(xaml, "x:Name=\"PreviewContentText\"");
        StringAssert.Contains(xaml, "x:Name=\"PreviewImageSurface\"");
        StringAssert.Contains(xaml, "x:Name=\"PdfPageNavigationPanel\"");
        StringAssert.Contains(xaml, "x:Name=\"PdfPreviousPageButton\"");
        StringAssert.Contains(xaml, "x:Name=\"PdfNextPageButton\"");
        StringAssert.Contains(xaml, "Click=\"PdfPreviousPageButton_Click\"");
        StringAssert.Contains(xaml, "Click=\"PdfNextPageButton_Click\"");
        StringAssert.Contains(xaml, "x:Name=\"PreviewMetadataList\"");
        StringAssert.Contains(xaml, "Key=\"P\" Modifiers=\"Control\" Invoked=\"TogglePreviewAccelerator_Invoked\"");
        StringAssert.Contains(codeBehind, "ViewModel.TogglePreviewPane()");
        StringAssert.Contains(codeBehind, "ViewModel.PreviewStatusText");
        StringAssert.Contains(codeBehind, "ViewModel.PreviewContentText");
        StringAssert.Contains(codeBehind, "ViewModel.CanRequestPreviousPdfPage");
        StringAssert.Contains(codeBehind, "ViewModel.CanRequestNextPdfPage");
        StringAssert.Contains(codeBehind, "ViewModel.RequestPreviousPdfPage()");
        StringAssert.Contains(codeBehind, "ViewModel.RequestNextPdfPage()");
        StringAssert.Contains(codeBehind, "SetPreviewArtifactAsync");
        StringAssert.Contains(codeBehind, "ViewModel.PreviewDisplayContent");
        StringAssert.Contains(codeBehind, "ViewModel.DetailsMetadataFields");
    }

    [TestMethod]
    [TestCategory("PreviewUi")]
    public void PreviewUi_main_window_binds_file_rows_and_accessibility_preview_state()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var fileListResources = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "Resources", "Components", "VeloFile.FileList.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);
        var app = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "App.xaml.cs").FullName);
        var composition = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);
        var normalizedComposition = composition.Replace("\r\n", "\n", StringComparison.Ordinal);

        StringAssert.Contains(codeBehind, "FileListSurface.ItemsSource = ViewModel.FileListRows");
        StringAssert.Contains(xaml, "ItemTemplate=\"{StaticResource VfFileListRowTemplate}\"");
        StringAssert.Contains(fileListResources, "Text=\"{Binding ThumbnailDisplayText}\"");
        StringAssert.Contains(fileListResources, "Opacity=\"{Binding RowOpacity}\"");
        StringAssert.Contains(codeBehind, "AutomationProperties.SetName(PreviewPane, ViewModel.PreviewAccessibilityName)");
        StringAssert.Contains(codeBehind, "ViewModel.DetailsMetadataFields");
        StringAssert.Contains(codeBehind, "ViewModel.SetShellDispatcher(new WinUiShellDispatcher(DispatcherQueue))");
        StringAssert.Contains(app, "new WinUiShellDispatcher(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread())");
        StringAssert.Contains(app, "AppCompositionRoot.CreateShellViewModel(shellDispatcher)");
        StringAssert.Contains(composition, "CreateShellViewModel(IShellDispatcher? shellDispatcher = null)");
        StringAssert.Contains(normalizedComposition, "thumbnailController,\n            shellDispatcher");
    }

    [TestMethod]
    [TestCategory("PreviewProviders")]
    public void PreviewProviders_app_composition_uses_windows_content_providers_before_metadata_fallback()
    {
        var repoRoot = FindRepoRoot();
        var composition = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "AppCompositionRoot.cs").FullName);

        StringAssert.Contains(composition, "WindowsPreviewProviderFactory.CreateDefault()");
        Assert.IsFalse(
            composition.Contains("[new MetadataOnlyPreviewProvider()]", StringComparison.Ordinal),
            "Production composition must not stop at metadata-only preview after M12.");
    }

    [TestMethod]
    public void Main_window_file_list_binds_real_items_and_maps_selection_to_listed_file_models()
    {
        var repoRoot = FindRepoRoot();
        var xaml = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);
        var codeBehind = File.ReadAllText(repoRoot.Combine("src", "VeloFile.App", "MainWindow.xaml.cs").FullName);

        StringAssert.Contains(xaml, "x:Name=\"FileListSurface\"");
        StringAssert.Contains(xaml, "ItemTemplate=\"{StaticResource VfFileListRowTemplate}\"");
        Assert.IsFalse(xaml.Contains("<ListViewItem Content=", StringComparison.Ordinal));
        StringAssert.Contains(codeBehind, "FileListSurface.ItemsSource = ViewModel.FileListRows");
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

    [TestMethod]
    [TestCategory("Accessibility")]
    public void M15_shell_interactive_surfaces_have_keyboard_routes_and_accessible_names()
    {
        var xaml = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);

        foreach (var accelerator in new[]
        {
            "Key=\"L\" Modifiers=\"Control\"",
            "Key=\"P\" Modifiers=\"Control\"",
            "Key=\"A\" Modifiers=\"Control\"",
            "Key=\"Enter\"",
            "Key=\"F2\"",
            "Key=\"Delete\"",
            "Key=\"C\" Modifiers=\"Control,Shift\"",
            "Key=\"N\" Modifiers=\"Control,Shift\""
        })
        {
            StringAssert.Contains(xaml, accelerator);
        }

        foreach (var automationName in new[]
        {
            "AutomationProperties.Name=\"Run recursive search\"",
            "AutomationProperties.Name=\"Cancel recursive search\"",
            "AutomationProperties.Name=\"Clear recursive search\"",
            "AutomationProperties.Name=\"Commit rename\"",
            "AutomationProperties.Name=\"Cancel rename\"",
            "AutomationProperties.Name=\"Confirm permanent delete\"",
            "AutomationProperties.Name=\"Cancel permanent delete\"",
            "AutomationProperties.Name=\"Skip conflict\"",
            "AutomationProperties.Name=\"Replace conflict\"",
            "AutomationProperties.Name=\"Keep both conflict\"",
            "AutomationProperties.Name=\"Cancel file operation\"",
            "AutomationProperties.Name=\"Previous PDF page\"",
            "AutomationProperties.Name=\"Next PDF page\""
        })
        {
            StringAssert.Contains(xaml, automationName);
        }
    }

    [TestMethod]
    [TestCategory("Accessibility")]
    public void M15_shell_exposes_distinct_visible_states_for_empty_loading_failure_and_destructive_confirmation()
    {
        var xaml = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);

        foreach (var stateName in new[]
        {
            "x:Name=\"LoadingState\"",
            "x:Name=\"EmptyFolderState\"",
            "x:Name=\"FailedState\"",
            "x:Name=\"RecursiveSearchStatusText\"",
            "x:Name=\"DropActionIndicatorText\"",
            "x:Name=\"RenameErrorText\"",
            "x:Name=\"FileOperationStatusText\"",
            "x:Name=\"PreviewStatusText\"",
            "x:Name=\"PreviewMetadataList\""
        })
        {
            StringAssert.Contains(xaml, stateName);
        }

        StringAssert.Contains(xaml, "Permanently delete selected items?");
        StringAssert.Contains(xaml, "Delete permanently");
        StringAssert.Contains(xaml, "x:Name=\"PermanentDeleteConfirmationPanel\"");
    }

    [TestMethod]
    [TestCategory("Accessibility")]
    public void M15_accessibility_release_evidence_requires_manual_checklist_for_focus_keyboard_and_mixed_dpi()
    {
        var repoRoot = FindRepoRoot();
        var checklistPath = repoRoot.Combine("docs", "release", "accessibility-checklist.md").FullName;
        Assert.IsTrue(File.Exists(checklistPath), "M15 release evidence must include a tracked accessibility checklist, not only static XAML scans.");

        var checklist = File.ReadAllText(checklistPath);
        foreach (var requiredText in new[]
        {
            "keyboard-only navigation",
            "focus indicator",
            "focus order",
            "destructive delete",
            "permanent delete",
            "operation cancel",
            "recursive search cap",
            "skipped locations",
            "preview loading",
            "preview failed",
            "unsupported states",
            "mixed-DPI",
            "100%",
            "150%",
            "200%",
            "screen-reader",
            "automation name",
            "Status",
            "Tester",
            "Date",
            "Build",
            "Environment",
            "Pass",
            "Fail",
            "Blocked",
            "Notes",
            "Linked issue"
        })
        {
            Assert.IsTrue(
                checklist.Contains(requiredText, StringComparison.OrdinalIgnoreCase),
                $"Accessibility checklist must mention '{requiredText}'.");
        }

        var triagePolicy = File.ReadAllText(repoRoot.Combine("docs", "release", "preview-triage.md").FullName);
        StringAssert.Contains(triagePolicy, "docs/release/accessibility-checklist.md");
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
