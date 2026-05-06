using VeloFile.App.ViewModels;
using VeloFile.Core.Commands;
using VeloFile.Core.FileAssociations;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Terminal;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.App.Tests;

[TestClass]
[TestCategory("Terminal")]
[TestCategory("FileAssociations")]
public sealed class AppShellTerminalAndFileAssociationsCommandRouteTests
{
    [TestMethod]
    public void Shell_construction_does_not_start_terminal_discovery()
    {
        var targetSource = new RecordingTerminalTargetSource([]);
        _ = CreateViewModel(
            terminalLaunchService: new TerminalLaunchService(
                new TerminalDiscoveryService(targetSource),
                new StaticWorkingDirectoryProbe(exists: true),
                new RecordingTerminalProcessLauncher()));

        Assert.AreEqual(0, targetSource.CallCount);
    }

    [TestMethod]
    public async Task Open_terminal_here_uses_active_path_as_working_directory_data()
    {
        var terminal = new TerminalTarget(
            "wt",
            TerminalTargetKind.WindowsTerminal,
            "Windows Terminal",
            @"C:\WindowsApps\wt.exe");
        var launcher = new RecordingTerminalProcessLauncher();
        var viewModel = CreateViewModel(
            terminalLaunchService: new TerminalLaunchService(
                new TerminalDiscoveryService(new RecordingTerminalTargetSource([terminal])),
                new StaticWorkingDirectoryProbe(exists: true),
                launcher));

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.OpenTerminalHere);

        Assert.AreEqual(@"D:\projects", launcher.Requests.Single().WorkingDirectory);
        Assert.IsNull(launcher.Requests.Single().CommandText);
        StringAssert.Contains(viewModel.LaunchStatusText, "Terminal launched");
        Assert.AreEqual(@"D:\projects", viewModel.ActivePath);
    }

    [TestMethod]
    public async Task Optional_discovered_terminal_target_can_be_selected_for_open_terminal_here()
    {
        var gitBash = new TerminalTarget(
            "git-bash",
            TerminalTargetKind.GitBash,
            "Git Bash",
            @"C:\Program Files\Git\git-bash.exe");
        var windowsTerminal = new TerminalTarget(
            "windows-terminal",
            TerminalTargetKind.WindowsTerminal,
            "Windows Terminal",
            @"C:\WindowsApps\wt.exe");
        var launcher = new RecordingTerminalProcessLauncher();
        var viewModel = CreateViewModel(
            terminalLaunchService: new TerminalLaunchService(
                new TerminalDiscoveryService(new RecordingTerminalTargetSource([gitBash, windowsTerminal])),
                new StaticWorkingDirectoryProbe(exists: true),
                launcher));

        await viewModel.LoadTerminalTargetsAsync();
        viewModel.SelectTerminalTarget(viewModel.TerminalTargets.Single(target => target.Kind is TerminalTargetKind.GitBash));
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.OpenTerminalHere);

        Assert.AreEqual(TerminalTargetKind.GitBash, launcher.Requests.Single().Target.Kind);
        Assert.AreEqual(@"D:\projects", launcher.Requests.Single().WorkingDirectory);
    }

    [TestMethod]
    public async Task Selected_terminal_target_is_persisted_in_settings_payload()
    {
        var settingsWriter = new RecordingSettingsStateWriter();
        var gitBash = new TerminalTarget(
            "git-bash",
            TerminalTargetKind.GitBash,
            "Git Bash",
            @"C:\Program Files\Git\git-bash.exe");
        var viewModel = CreateViewModel(
            terminalLaunchService: new TerminalLaunchService(
                new TerminalDiscoveryService(new RecordingTerminalTargetSource([gitBash])),
                new StaticWorkingDirectoryProbe(exists: true),
                new RecordingTerminalProcessLauncher()),
            settingsWriter: settingsWriter);

        await viewModel.LoadTerminalTargetsAsync();
        viewModel.SelectTerminalTarget(viewModel.TerminalTargets.Single());

        Assert.AreEqual("git-bash", settingsWriter.LastPayload?.PreferredTerminalTargetId);
    }

    [TestMethod]
    public async Task Open_terminal_here_reports_missing_terminal_without_changing_browsing_state()
    {
        var viewModel = CreateViewModel(
            terminalLaunchService: new TerminalLaunchService(
                new TerminalDiscoveryService(new RecordingTerminalTargetSource([])),
                new StaticWorkingDirectoryProbe(exists: true),
                new RecordingTerminalProcessLauncher()));
        var beforePath = viewModel.ActivePath;

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.OpenTerminalHere);

        StringAssert.Contains(viewModel.LaunchStatusText, TerminalLaunchReasonCodes.TerminalUnavailable);
        Assert.AreEqual(beforePath, viewModel.ActivePath);
        Assert.AreEqual(0, viewModel.SelectedFileItems.Count);
    }

    [TestMethod]
    public async Task Open_command_uses_file_association_route_for_selected_file()
    {
        var adapter = new RecordingFileAssociationLaunchAdapter();
        var item = Item(@"D:\projects\report.txt", "report.txt");
        var viewModel = CreateViewModel(fileAssociationLaunchService: new FileAssociationLaunchService(adapter));
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Open);

        Assert.AreEqual(FileAssociationLaunchKind.Open, adapter.Requests.Single().Kind);
        Assert.AreEqual(item.FullPath, adapter.Requests.Single().Path);
        Assert.IsFalse(adapter.Requests.Single().ModifySystemAssociations);
        StringAssert.Contains(viewModel.LaunchStatusText, "Open launched");
        Assert.AreEqual(@"D:\projects", viewModel.ActivePath);
    }

    [TestMethod]
    public async Task Open_with_failure_is_visible_and_does_not_modify_associations_or_browsing_state()
    {
        var adapter = new RecordingFileAssociationLaunchAdapter
        {
            NextResult = FileAssociationLaunchResult.Failed(FileAssociationLaunchKind.OpenWith, "association-launch-failed")
        };
        var item = Item(@"D:\projects\unknown.velo", "unknown.velo");
        var viewModel = CreateViewModel(fileAssociationLaunchService: new FileAssociationLaunchService(adapter));
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.OpenWith);

        Assert.AreEqual(FileAssociationLaunchKind.OpenWith, adapter.Requests.Single().Kind);
        Assert.IsFalse(adapter.Requests.Single().ModifySystemAssociations);
        StringAssert.Contains(viewModel.LaunchStatusText, "association-launch-failed");
        Assert.AreEqual(@"D:\projects", viewModel.ActivePath);
    }

    private static AppShellViewModel CreateViewModel(
        TerminalLaunchService? terminalLaunchService = null,
        FileAssociationLaunchService? fileAssociationLaunchService = null,
        ISettingsStateWriter? settingsWriter = null)
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
            new TestPathExistenceProbe([@"D:\projects"]),
            settingsWriter ?? NoOpSettingsStateWriter.Instance,
            utcNow: () => DateTimeOffset.Parse("2026-05-05T00:00:00Z"));
        var startupState = new AppShellStartupState(
            "VeloFile",
            commandSurface,
            WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement));

        return new AppShellViewModel(
            startupState,
            terminalLaunchService: terminalLaunchService,
            fileAssociationLaunchService: fileAssociationLaunchService);
    }

    private static ListedFileItem Item(string fullPath, string name)
    {
        return new ListedFileItem(
            fullPath,
            name,
            name,
            FileSystemEntryKind.File,
            Length: 1,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private sealed class RecordingTerminalTargetSource : ITerminalTargetSource
    {
        private readonly IReadOnlyList<TerminalTarget> _targets;

        public RecordingTerminalTargetSource(IReadOnlyList<TerminalTarget> targets)
        {
            _targets = targets;
        }

        public int CallCount { get; private set; }

        public ValueTask<IReadOnlyList<TerminalTarget>> GetAvailableTargetsAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return ValueTask.FromResult(_targets);
        }
    }

    private sealed class StaticWorkingDirectoryProbe : IWorkingDirectoryProbe
    {
        private readonly bool _exists;

        public StaticWorkingDirectoryProbe(bool exists)
        {
            _exists = exists;
        }

        public bool Exists(string path)
        {
            return _exists;
        }
    }

    private sealed class RecordingTerminalProcessLauncher : ITerminalProcessLauncher
    {
        public List<TerminalLaunchRequest> Requests { get; } = [];

        public Task<TerminalLaunchResult> LaunchAsync(TerminalLaunchRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(TerminalLaunchResult.Succeeded(request.Target));
        }
    }

    private sealed class RecordingFileAssociationLaunchAdapter : IFileAssociationLaunchAdapter
    {
        public List<FileAssociationLaunchRequest> Requests { get; } = [];

        public FileAssociationLaunchResult NextResult { get; set; } =
            FileAssociationLaunchResult.Succeeded(FileAssociationLaunchKind.Open);

        public Task<FileAssociationLaunchResult> LaunchAsync(
            FileAssociationLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(NextResult with { Kind = request.Kind });
        }
    }

    private sealed class RecordingSettingsStateWriter : ISettingsStateWriter
    {
        public SettingsStatePayload? LastPayload { get; private set; }

        public void Write(SettingsStatePayload payload)
        {
            LastPayload = payload;
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
}
