using VeloFile.App.Input;
using VeloFile.App.ViewModels;
using VeloFile.Core.Commands;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Search;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.App.Tests;

[TestClass]
[TestCategory("Commands")]
[TestCategory("Selection")]
public sealed class AppShellCommandRouteTests
{
    [TestMethod]
    public void Copy_path_uses_selected_listed_file_models()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        var first = Item(@"D:\projects\alpha.txt", "alpha.txt");
        var second = Item(@"D:\projects\docs", "docs", FileSystemEntryKind.Directory);

        viewModel.SetFileItems([first, second]);
        viewModel.SetSelectedFileItems([first, second]);

        Assert.IsTrue(viewModel.IsBuiltInCommandAvailable(VeloFileCommandId.CopyPath, canPaste: false));
        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyPath);

        StringAssert.Contains(clipboard.Text!, first.FullPath);
        StringAssert.Contains(clipboard.Text!, second.FullPath);
        Assert.IsFalse(clipboard.Text!.Contains(first.Name + Environment.NewLine + second.Name, StringComparison.Ordinal));
    }

    [TestMethod]
    public void Copy_name_uses_selected_listed_file_model_names()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        var first = Item(@"D:\projects\alpha.txt", "alpha.txt");
        var second = Item(@"D:\projects\docs", "docs", FileSystemEntryKind.Directory);

        viewModel.SetFileItems([first, second]);
        viewModel.SetSelectedFileItems([first, second]);

        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyName);

        StringAssert.Contains(clipboard.Text!, "alpha.txt");
        StringAssert.Contains(clipboard.Text!, "docs");
        Assert.IsFalse(clipboard.Text!.Contains(@"D:\projects", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Selection_mapper_preserves_real_listed_file_items_from_rows_wrappers_and_containers()
    {
        var direct = Item(@"D:\projects\direct.txt", "direct.txt");
        var wrapped = Item(@"D:\projects\wrapped.txt", "wrapped.txt");
        var container = Item(@"D:\projects\container.txt", "container.txt");

        var mapped = FileListSelectionMapper.ToListedFileItems(
            [
                new TestSelectionContainer(container),
                new TestFileListRow(wrapped),
                direct,
                new object()
            ],
            [direct, wrapped, container]);

        CollectionAssert.AreEqual(new[] { direct, wrapped, container }, mapped.ToArray());
    }

    [TestMethod]
    public void App_file_accelerator_route_suppresses_file_commands_when_text_input_has_focus()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        var item = Item(@"D:\projects\alpha.txt", "alpha.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);
        var router = new AppFileCommandAcceleratorRouter(
            viewModel,
            new TestKeyboardFocusContextProvider(AppKeyboardFocusScope.TextInput));

        var result = router.Route(KeyGesture.CtrlShift("C"));

        Assert.AreEqual(KeyboardRouteStatus.SuppressedByTextInputFocus, result.Status);
        Assert.IsNull(clipboard.Text);
    }

    [TestMethod]
    public void App_file_accelerator_route_runs_file_commands_when_file_list_has_focus()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        var item = Item(@"D:\projects\alpha.txt", "alpha.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);
        var router = new AppFileCommandAcceleratorRouter(
            viewModel,
            new TestKeyboardFocusContextProvider(AppKeyboardFocusScope.FileList));

        var result = router.Route(KeyGesture.CtrlShift("C"));

        Assert.AreEqual(KeyboardRouteStatus.Routed, result.Status);
        Assert.AreEqual(VeloFileCommandId.CopyPath, result.CommandId);
        StringAssert.Contains(clipboard.Text!, item.FullPath);
    }

    [TestMethod]
    public void App_file_accelerator_route_leaves_file_commands_unhandled_outside_file_list_scope()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        var item = Item(@"D:\projects\alpha.txt", "alpha.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);
        var router = new AppFileCommandAcceleratorRouter(
            viewModel,
            new TestKeyboardFocusContextProvider(AppKeyboardFocusScope.Other));

        var result = router.Route(KeyGesture.CtrlShift("C"));

        Assert.AreEqual(KeyboardRouteStatus.NotHandled, result.Status);
        Assert.IsNull(clipboard.Text);
    }

    [TestMethod]
    public async Task Startup_listing_populates_visible_file_items_and_copy_path_uses_shell_selection()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var first = Item(@"D:\projects\alpha.txt", "alpha.txt");
        var second = Item(@"D:\projects\beta.txt", "beta.txt");
        source.SetEntries(@"D:\projects", first, second);
        var viewModel = CreateViewModel(clipboard, listingSource: source);

        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);
        viewModel.SetSelectedFileItems(FileListSelectionMapper.ToListedFileItems([second], viewModel.FileItems));
        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyPath);

        Assert.AreEqual(second.FullPath, clipboard.Text);
    }

    [TestMethod]
    public async Task Successful_navigation_reloads_visible_file_items_for_copy_name()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var a1 = Item(@"D:\projects\a1.txt", "a1.txt");
        var b1 = Item(@"D:\other\b1.txt", "b1.txt");
        var b2 = Item(@"D:\other\b2.txt", "b2.txt");
        source.SetEntries(@"D:\projects", a1);
        source.SetEntries(@"D:\other", b1, b2);
        var viewModel = CreateViewModel(
            clipboard,
            listingSource: source,
            existingPaths: [@"D:\projects", @"D:\other"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].FullPath == a1.FullPath);

        var result = viewModel.SubmitPath(@"D:\other");

        Assert.IsTrue(result.Accepted);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2 && viewModel.FileItems[0].FullPath == b1.FullPath);
        viewModel.SetSelectedFileItems(FileListSelectionMapper.ToListedFileItems([b2], viewModel.FileItems));
        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyName);

        Assert.AreEqual("b2.txt", clipboard.Text);
    }

    [TestMethod]
    public async Task Active_tab_switch_replaces_visible_file_items_for_that_tab()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var a1 = Item(@"D:\projects\a1.txt", "a1.txt");
        var b1 = Item(@"D:\other\b1.txt", "b1.txt");
        source.SetEntries(@"D:\projects", a1);
        source.SetEntries(@"D:\other", b1);
        var viewModel = CreateViewModel(
            clipboard,
            defaultPath: @"D:\other",
            listingSource: source,
            existingPaths: [@"D:\projects", @"D:\other"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].FullPath == a1.FullPath);

        viewModel.NewTab();
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].FullPath == b1.FullPath);

        viewModel.SwitchToTab(0);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].FullPath == a1.FullPath);

        viewModel.SwitchToTab(1);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].FullPath == b1.FullPath);
        viewModel.SetSelectedFileItems(FileListSelectionMapper.ToListedFileItems([b1], viewModel.FileItems));
        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyName);

        Assert.AreEqual("b1.txt", clipboard.Text);
    }

    [TestMethod]
    public void Selection_mapping_orders_selected_rows_by_current_visible_order()
    {
        var first = Item(@"D:\projects\a.txt", "a.txt");
        var second = Item(@"D:\projects\b.txt", "b.txt");
        var third = Item(@"D:\projects\c.txt", "c.txt");

        var mapped = FileListSelectionMapper.ToListedFileItems([third, first], [first, second, third]);

        CollectionAssert.AreEqual(new[] { first, third }, mapped.ToArray());
    }

    [TestMethod]
    public void Selection_mapping_respects_sorted_or_filtered_visible_order()
    {
        var first = Item(@"D:\projects\a.txt", "a.txt");
        var second = Item(@"D:\projects\b.txt", "b.txt");
        var third = Item(@"D:\projects\c.txt", "c.txt");

        var mapped = FileListSelectionMapper.ToListedFileItems([first, third], [third, first]);

        CollectionAssert.AreEqual(new[] { third, first }, mapped.ToArray());
        CollectionAssert.DoesNotContain(mapped.ToArray(), second);
    }

    [TestMethod]
    public void Selection_mapping_ignores_stale_selected_rows()
    {
        var visible = Item(@"D:\projects\a.txt", "a.txt");
        var stale = Item(@"D:\old\z.txt", "z.txt");

        var mapped = FileListSelectionMapper.ToListedFileItems([stale, visible], [visible]);

        CollectionAssert.AreEqual(new[] { visible }, mapped.ToArray());
    }

    [TestMethod]
    public void View_model_selected_items_are_ordered_by_current_visible_file_items()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        var first = Item(@"D:\projects\a.txt", "a.txt");
        var second = Item(@"D:\projects\b.txt", "b.txt");
        var third = Item(@"D:\projects\c.txt", "c.txt");
        viewModel.SetFileItems([first, second, third]);

        viewModel.SetSelectedFileItems([third, first]);
        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyPath);

        Assert.AreEqual(
            string.Join(Environment.NewLine, first.FullPath, third.FullPath),
            clipboard.Text);
    }

    [TestMethod]
    [TestCategory("Filtering")]
    public async Task Filtering_current_folder_narrows_visible_file_items_and_clear_restores_listing()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var readme = Item(@"D:\projects\README.md", "README.md");
        var report = Item(@"D:\projects\report.pdf", "report.pdf");
        var src = Item(@"D:\projects\src", "src", FileSystemEntryKind.Directory);
        source.SetEntries(@"D:\projects", readme, report, src);
        var viewModel = CreateViewModel(clipboard, listingSource: source);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 3);

        viewModel.SetCurrentFolderFilter("read");

        CollectionAssert.AreEqual(new[] { "README.md" }, viewModel.FileItems.Select(item => item.Name).ToArray());

        viewModel.SetCurrentFolderFilter("");

        CollectionAssert.AreEqual(
            new[] { "README.md", "report.pdf", "src" },
            viewModel.FileItems.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Filtering")]
    public async Task Filtering_current_folder_does_not_start_recursive_search()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var searchSource = new FakeFolderEntrySource();
        source.SetEntries(@"D:\projects", Item(@"D:\projects\README.md", "README.md"));
        var viewModel = CreateViewModel(clipboard, listingSource: source, searchSource: searchSource);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1);

        viewModel.SetCurrentFolderFilter("read");

        Assert.AreEqual(RecursiveSearchStatus.NotStarted, viewModel.RecursiveSearch.Status);
        Assert.AreEqual(0, searchSource.SearchEnumerationCount);
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_is_explicit_and_updates_limit_reached_state()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        source.SetEntries(@"D:\projects",
            Item(@"D:\projects\match-1.txt", "match-1.txt"),
            Item(@"D:\projects\match-2.txt", "match-2.txt"),
            Item(@"D:\projects\match-3.txt", "match-3.txt"));
        var viewModel = CreateViewModel(clipboard, listingSource: source, searchSource: source);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 3);

        viewModel.StartRecursiveSearch("match", resultLimit: 2);

        await WaitUntilAsync(() => viewModel.RecursiveSearch.Status is RecursiveSearchStatus.ResultLimitReached);
        CollectionAssert.AreEqual(
            new[] { "match-1.txt", "match-2.txt" },
            viewModel.RecursiveSearch.Results.Select(item => item.Name).ToArray());
        Assert.IsTrue(viewModel.RecursiveSearch.ResultLimitReached);
        Assert.IsTrue(viewModel.RecursiveSearch.CanCancel);
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_streams_results_into_visible_items_and_copy_commands_use_search_rows()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var listingSource = new FakeFolderEntrySource();
        var searchSource = new GateFolderEntrySource();
        listingSource.SetEntries(@"D:\projects", Item(@"D:\projects\folder-row.txt", "folder-row.txt"));
        searchSource.SetEntries(@"D:\projects",
            Item(@"D:\projects\match-1.txt", "match-1.txt"),
            Item(@"D:\projects\nested\match-2.txt", "match-2.txt"));
        var viewModel = CreateViewModel(clipboard, listingSource: listingSource, searchSource: searchSource);
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1 && viewModel.VisibleItems[0].Name == "folder-row.txt");

        viewModel.StartRecursiveSearch("match");

        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1 && viewModel.VisibleItems[0].Name == "match-1.txt");
        CollectionAssert.AreEqual(new[] { "match-1.txt" }, viewModel.VisibleItems.Select(item => item.Name).ToArray());
        viewModel.SetSelectedFileItems([viewModel.VisibleItems[0]]);
        viewModel.ExecuteBuiltInCommand(VeloFileCommandId.CopyPath);
        Assert.AreEqual(@"D:\projects\match-1.txt", clipboard.Text);

        searchSource.Release();
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 2);
        CollectionAssert.AreEqual(
            new[] { "match-1.txt", "match-2.txt" },
            viewModel.VisibleItems.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_skipped_locations_are_visible_in_status_and_details()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var listingSource = new FakeFolderEntrySource();
        var searchSource = new FakeFolderEntrySource();
        listingSource.SetEntries(@"D:\projects", Item(@"D:\projects\folder-row.txt", "folder-row.txt"));
        searchSource.SetEntries(@"D:\projects",
            Item(@"D:\projects\denied", "denied", FileSystemEntryKind.Directory),
            Item(@"D:\projects\loop", "loop", FileSystemEntryKind.Directory, FileAttributes.Directory | FileAttributes.ReparsePoint),
            Item(@"D:\projects\match.txt", "match.txt"));
        searchSource.SetException(@"D:\projects\denied", new UnauthorizedAccessException());
        var viewModel = CreateViewModel(clipboard, listingSource: listingSource, searchSource: searchSource);
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1);

        viewModel.StartRecursiveSearch("match");

        await WaitUntilAsync(() => viewModel.RecursiveSearch.Status is RecursiveSearchStatus.Completed);
        Assert.AreEqual(2, viewModel.SearchSkippedLocations.Count);
        Assert.IsTrue(viewModel.SearchSkippedLocationsVisible);
        StringAssert.Contains(viewModel.RecursiveSearchStatusText, "2 skipped locations");
        CollectionAssert.AreEquivalent(
            new[] { "access-denied", "reparse-point" },
            viewModel.SearchSkippedLocations.Select(location => location.ReasonCode).ToArray());
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_can_be_cancelled_after_result_limit_is_reached()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        source.SetEntries(@"D:\projects",
            Item(@"D:\projects\match-1.txt", "match-1.txt"),
            Item(@"D:\projects\match-2.txt", "match-2.txt"),
            Item(@"D:\projects\match-3.txt", "match-3.txt"));
        var viewModel = CreateViewModel(clipboard, listingSource: source, searchSource: source);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 3);
        viewModel.StartRecursiveSearch("match", resultLimit: 2);
        await WaitUntilAsync(() => viewModel.RecursiveSearch.Status is RecursiveSearchStatus.ResultLimitReached);

        viewModel.CancelRecursiveSearch();

        Assert.AreEqual(RecursiveSearchStatus.Cancelled, viewModel.RecursiveSearch.Status);
        CollectionAssert.AreEqual(
            new[] { "match-1.txt", "match-2.txt" },
            viewModel.RecursiveSearch.Results.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_cancel_preserves_streamed_results()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var listingSource = new FakeFolderEntrySource();
        var searchSource = new GateFolderEntrySource();
        listingSource.SetEntries(@"D:\projects",
            Item(@"D:\projects\match-1.txt", "match-1.txt"),
            Item(@"D:\projects\match-2.txt", "match-2.txt"));
        searchSource.SetEntries(@"D:\projects",
            Item(@"D:\projects\match-1.txt", "match-1.txt"),
            Item(@"D:\projects\match-2.txt", "match-2.txt"));
        var viewModel = CreateViewModel(clipboard, listingSource: listingSource, searchSource: searchSource);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);

        viewModel.StartRecursiveSearch("match");
        await WaitUntilAsync(() => viewModel.RecursiveSearch.Results.Count >= 1);

        viewModel.CancelRecursiveSearch();
        searchSource.Release();

        Assert.AreEqual(RecursiveSearchStatus.Cancelled, viewModel.RecursiveSearch.Status);
        Assert.IsTrue(viewModel.RecursiveSearch.Results.Count >= 1);
        Assert.IsFalse(viewModel.RecursiveSearch.CanCancel);
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_clear_returns_to_current_folder_visible_items()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var listingSource = new FakeFolderEntrySource();
        var searchSource = new FakeFolderEntrySource();
        listingSource.SetEntries(@"D:\projects", Item(@"D:\projects\folder-row.txt", "folder-row.txt"));
        searchSource.SetEntries(@"D:\projects", Item(@"D:\projects\match.txt", "match.txt"));
        var viewModel = CreateViewModel(clipboard, listingSource: listingSource, searchSource: searchSource);
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1 && viewModel.VisibleItems[0].Name == "folder-row.txt");
        viewModel.StartRecursiveSearch("match");
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1 && viewModel.VisibleItems[0].Name == "match.txt");

        viewModel.ClearRecursiveSearch();

        Assert.AreEqual(RecursiveSearchStatus.NotStarted, viewModel.RecursiveSearch.Status);
        Assert.AreEqual("", viewModel.RecursiveSearchStatusText);
        CollectionAssert.AreEqual(new[] { "folder-row.txt" }, viewModel.VisibleItems.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Search")]
    public async Task Recursive_search_new_query_after_cap_replaces_old_results_and_ignores_stale_updates()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var listingSource = new FakeFolderEntrySource();
        listingSource.SetEntries(@"D:\projects", Item(@"D:\projects\folder-row.txt", "folder-row.txt"));
        var searchService = new ScriptedRecursiveSearchService();
        var viewModel = CreateViewModel(clipboard, listingSource: listingSource, searchService: searchService);
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1);

        viewModel.StartRecursiveSearch("old", resultLimit: 2);
        searchService.Emit("old", RecursiveSearchUpdate.ResultFound(Item(@"D:\projects\old-1.txt", "old-1.txt"), 1));
        searchService.Emit("old", RecursiveSearchUpdate.ResultFound(Item(@"D:\projects\old-2.txt", "old-2.txt"), 2));
        searchService.Emit("old", RecursiveSearchUpdate.Skipped(@"D:\projects\old-denied", "access-denied", 2));
        searchService.Emit("old", RecursiveSearchUpdate.LimitReached(2));
        await WaitUntilAsync(() => viewModel.RecursiveSearch.Status is RecursiveSearchStatus.ResultLimitReached);
        CollectionAssert.AreEqual(new[] { "old-1.txt", "old-2.txt" }, viewModel.VisibleItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(1, viewModel.SearchSkippedLocations.Count);
        StringAssert.Contains(viewModel.RecursiveSearchStatusText, "refine or start a new search");

        viewModel.StartRecursiveSearch("new");
        searchService.Emit("new", RecursiveSearchUpdate.ResultFound(Item(@"D:\projects\new-1.txt", "new-1.txt"), 1));
        searchService.Emit("old", RecursiveSearchUpdate.ResultFound(Item(@"D:\projects\old-late.txt", "old-late.txt"), 3));
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1 && viewModel.VisibleItems[0].Name == "new-1.txt");

        Assert.AreEqual("new", viewModel.RecursiveSearch.Query);
        Assert.IsFalse(viewModel.RecursiveSearch.ResultLimitReached);
        Assert.AreEqual(0, viewModel.SearchSkippedLocations.Count);
        CollectionAssert.AreEqual(new[] { "new-1.txt" }, viewModel.VisibleItems.Select(item => item.Name).ToArray());
    }

    private static AppShellViewModel CreateViewModel(
        IClipboardTextWriter clipboardWriter,
        string initialPath = @"D:\projects",
        string? defaultPath = null,
        FakeFolderEntrySource? listingSource = null,
        FakeFolderEntrySource? searchSource = null,
        IRecursiveSearchService? searchService = null,
        IReadOnlyList<string>? existingPaths = null)
    {
        var workspace = NavigationWorkspace.Create(initialPath);
        var sidebar = SidebarStateService.Create(
            FavoritesStatePayload.Empty,
            RecentLocationsStatePayload.Empty,
            drives: []);
        var visibility = VisibilitySettingsService.FromPayload(SettingsStatePayload.Default);
        var commandSurface = new AppShellCommandSurface(
            "VeloFile",
            workspace,
            sidebar,
            visibility,
            CrashRecoveryState.None,
            new TestDefaultLaunchPathProvider(defaultPath ?? initialPath),
            new TestPathExistenceProbe(existingPaths ?? [initialPath, defaultPath ?? initialPath]),
            NoOpSettingsStateWriter.Instance,
            utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z"));
        var startupState = new AppShellStartupState(
            "VeloFile",
            commandSurface,
            WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement));
        var coordinator = listingSource is null
            ? null
            : new FolderListingCoordinator(new FolderListingService(listingSource));
        searchService ??= searchSource is null
            ? null
            : new RecursiveSearchService(searchSource);

        return new AppShellViewModel(startupState, clipboardWriter, coordinator, searchService, viewportItemCount: 100);
    }

    private static ListedFileItem Item(
        string fullPath,
        string name,
        FileSystemEntryKind kind = FileSystemEntryKind.File,
        FileAttributes attributes = FileAttributes.Normal)
    {
        return new ListedFileItem(
            fullPath,
            name,
            name,
            kind,
            Length: null,
            LastWriteTimeUtc: null,
            attributes,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private sealed class CollectingClipboardTextWriter : IClipboardTextWriter
    {
        public string? Text { get; private set; }

        public void SetText(string text)
        {
            Text = text;
        }
    }

    private sealed class TestDefaultLaunchPathProvider : IDefaultLaunchPathProvider
    {
        private readonly string _path;

        public TestDefaultLaunchPathProvider(string path)
        {
            _path = path;
        }

        public string GetDefaultLaunchPath()
        {
            return _path;
        }
    }

    private sealed class TestPathExistenceProbe : IPathExistenceProbe
    {
        private readonly HashSet<string> _existingPaths;

        public TestPathExistenceProbe(IEnumerable<string> existingPaths)
        {
            _existingPaths = new HashSet<string>(existingPaths, StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string path)
        {
            return _existingPaths.Contains(path);
        }
    }

    private sealed class TestKeyboardFocusContextProvider : IKeyboardFocusContextProvider
    {
        private readonly AppKeyboardFocusScope _focusScope;

        public TestKeyboardFocusContextProvider(AppKeyboardFocusScope focusScope)
        {
            _focusScope = focusScope;
        }

        public AppKeyboardFocusScope GetFocusScope()
        {
            return _focusScope;
        }
    }

    private sealed class TestFileListRow : IFileListRowItem
    {
        public TestFileListRow(ListedFileItem fileItem)
        {
            FileItem = fileItem;
        }

        public ListedFileItem FileItem { get; }
    }

    private sealed class TestSelectionContainer
    {
        public TestSelectionContainer(object dataContext)
        {
            DataContext = dataContext;
        }

        public object DataContext { get; }
    }

    private class FakeFolderEntrySource : IFolderEntrySource
    {
        private readonly Dictionary<string, IReadOnlyList<FileSystemEntrySnapshot>> _entries = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Exception> _exceptions = new(StringComparer.OrdinalIgnoreCase);

        public int SearchEnumerationCount { get; private set; }

        public void SetEntries(string path, params ListedFileItem[] items)
        {
            _entries[path] = items
                .Select(item => new FileSystemEntrySnapshot(
                    item.FullPath,
                    item.Name,
                    item.Kind,
                    item.Length,
                    item.LastWriteTimeUtc,
                    item.Attributes))
                .ToArray();
        }

        public void SetException(string path, Exception exception)
        {
            _exceptions[path] = exception;
        }

        public virtual async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(string path, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            if (path == @"D:\projects")
            {
                SearchEnumerationCount++;
            }

            if (_exceptions.TryGetValue(path, out var exception))
            {
                throw exception;
            }

            if (!_entries.TryGetValue(path, out var entries))
            {
                yield break;
            }

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }

    private sealed class ScriptedRecursiveSearchService : IRecursiveSearchService
    {
        private readonly Dictionary<string, System.Threading.Channels.Channel<RecursiveSearchUpdate>> _channels =
            new(StringComparer.OrdinalIgnoreCase);

        public void Emit(string query, RecursiveSearchUpdate update)
        {
            ChannelFor(query).Writer.TryWrite(update);
        }

        public async IAsyncEnumerable<RecursiveSearchUpdate> SearchAsync(
            string rootPath,
            string query,
            RecursiveSearchOptions options,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var channel = ChannelFor(query);
            await foreach (var update in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                yield return update;
            }
        }

        private System.Threading.Channels.Channel<RecursiveSearchUpdate> ChannelFor(string query)
        {
            if (!_channels.TryGetValue(query, out var channel))
            {
                channel = System.Threading.Channels.Channel.CreateUnbounded<RecursiveSearchUpdate>();
                _channels[query] = channel;
            }

            return channel;
        }
    }

    private sealed class GateFolderEntrySource : FakeFolderEntrySource
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _yieldedFirst;

        public void Release()
        {
            _gate.TrySetResult();
        }

        public override async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var entry in base.EnumerateAsync(path, cancellationToken))
            {
                if (_yieldedFirst)
                {
                    await _gate.Task.ConfigureAwait(false);
                }

                _yieldedFirst = true;
                yield return entry;
            }
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(2);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(20);
        }

        Assert.Fail("Condition was not met before the timeout.");
    }
}
