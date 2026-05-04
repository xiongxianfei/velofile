using VeloFile.App.Input;
using VeloFile.App.ViewModels;
using VeloFile.Core.Commands;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

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

        var mapped = FileListSelectionMapper.ToListedFileItems([
            direct,
            new TestFileListRow(wrapped),
            new TestSelectionContainer(container),
            new object()
        ]);

        CollectionAssert.AreEqual(new[] { direct, wrapped, container }, mapped.ToArray());
    }

    [TestMethod]
    public void App_file_accelerator_route_suppresses_file_commands_when_text_input_has_focus()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var viewModel = CreateViewModel(clipboard);
        viewModel.SetSelectedFileItems([Item(@"D:\projects\alpha.txt", "alpha.txt")]);
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
        viewModel.SetSelectedFileItems([Item(@"D:\projects\alpha.txt", "alpha.txt")]);
        var router = new AppFileCommandAcceleratorRouter(
            viewModel,
            new TestKeyboardFocusContextProvider(AppKeyboardFocusScope.Other));

        var result = router.Route(KeyGesture.CtrlShift("C"));

        Assert.AreEqual(KeyboardRouteStatus.NotHandled, result.Status);
        Assert.IsNull(clipboard.Text);
    }

    private static AppShellViewModel CreateViewModel(IClipboardTextWriter clipboardWriter)
    {
        var workspace = NavigationWorkspace.Create(@"D:\projects");
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
            new TestDefaultLaunchPathProvider(@"D:\projects"),
            new TestPathExistenceProbe(),
            NoOpSettingsStateWriter.Instance,
            utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z"));
        var startupState = new AppShellStartupState(
            "VeloFile",
            commandSurface,
            WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement));

        return new AppShellViewModel(startupState, clipboardWriter);
    }

    private static ListedFileItem Item(
        string fullPath,
        string name,
        FileSystemEntryKind kind = FileSystemEntryKind.File)
    {
        return new ListedFileItem(
            fullPath,
            name,
            name,
            kind,
            Length: null,
            LastWriteTimeUtc: null,
            FileAttributes.Normal,
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
        public bool Exists(string path)
        {
            return true;
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
}
