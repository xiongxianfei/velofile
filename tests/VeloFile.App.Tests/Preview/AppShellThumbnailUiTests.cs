using VeloFile.App.ViewModels;
using VeloFile.Core.Foundation;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Preview;
using VeloFile.Core.Search;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

namespace VeloFile.App.Tests.Preview;

[TestClass]
public sealed class AppShellThumbnailUiTests
{
    [TestMethod]
    [TestCategory("Thumbnails")]
    [TestCategory("PreviewUi")]
    public async Task PreviewUi_file_list_rows_expose_thumbnail_state_and_visual_dimmed_state()
    {
        var provider = new ScriptedThumbnailProvider();
        provider.SetResult("alpha.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("TXT")));
        provider.SetResult("hidden.sys", ThumbnailProviderResult.GenericIcon(ThumbnailArtifact.GenericIcon("SYS"), "generic-icon"));
        var thumbnailController = new ThumbnailController(
            provider,
            PreviewTimeoutPolicy.ForTesting(TimeSpan.FromMilliseconds(500)));
        var viewModel = CreateViewModel(thumbnailController);
        var visible = Item("alpha.txt", isVisuallyDimmed: false);
        var dimmed = Item("hidden.sys", isVisuallyDimmed: true);

        viewModel.SetFileItems([visible, dimmed]);
        var rowList = viewModel.FileListRows;
        var alphaRow = viewModel.FileListRows[0];
        var dimmedRow = viewModel.FileListRows[1];

        await WaitUntilAsync(() => viewModel.FileListRows.All(row => row.ThumbnailStatus is ThumbnailStatus.Ready or ThumbnailStatus.GenericIcon));

        Assert.HasCount(2, viewModel.FileListRows);
        Assert.AreSame(rowList, viewModel.FileListRows);
        Assert.AreSame(alphaRow, viewModel.FileListRows[0]);
        Assert.AreSame(dimmedRow, viewModel.FileListRows[1]);
        Assert.AreSame(visible, viewModel.FileListRows[0].FileItem);
        Assert.AreEqual(ThumbnailStatus.Ready, viewModel.FileListRows[0].ThumbnailStatus);
        Assert.AreEqual("TXT", viewModel.FileListRows[0].ThumbnailDisplayText);
        Assert.IsFalse(viewModel.FileListRows[0].IsVisuallyDimmed);
        Assert.AreEqual(ThumbnailStatus.GenericIcon, viewModel.FileListRows[1].ThumbnailStatus);
        Assert.AreEqual("SYS", viewModel.FileListRows[1].ThumbnailDisplayText);
        Assert.IsTrue(viewModel.FileListRows[1].IsVisuallyDimmed);
    }

    [TestMethod]
    [TestCategory("PreviewUi")]
    public async Task PreviewUi_preview_accessibility_name_distinguishes_empty_loading_unsupported_and_failed()
    {
        var provider = new ScriptedPreviewProvider();
        provider.SetPending("loading.txt");
        provider.SetResult("unsupported.bin", PreviewProviderResult.Unsupported("unsupported"));
        provider.SetResult("failed.bin", PreviewProviderResult.Failed("decode-error"));
        var viewModel = CreateViewModel(previewController: new PreviewController(
            [provider],
            new PreviewMetadataProvider(),
            new PreviewControllerOptions(
                TimeSpan.FromMilliseconds(1),
                PreviewTimeoutPolicy.ForTesting(TimeSpan.FromMilliseconds(500)))));
        var loading = Item("loading.txt");
        var unsupported = Item("unsupported.bin");
        var failed = Item("failed.bin");
        viewModel.SetFileItems([loading, unsupported, failed]);

        Assert.AreEqual("Preview empty", viewModel.PreviewAccessibilityName);

        viewModel.TogglePreviewPane();
        viewModel.SetSelectedFileItems([loading]);
        await WaitUntilAsync(() => viewModel.PreviewStatus is PreviewStatus.Loading);
        Assert.AreEqual("Preview loading", viewModel.PreviewAccessibilityName);

        viewModel.SetSelectedFileItems([unsupported]);
        await WaitUntilAsync(() => viewModel.PreviewStatus is PreviewStatus.Unsupported);
        Assert.AreEqual("Preview unsupported: unsupported", viewModel.PreviewAccessibilityName);

        viewModel.SetSelectedFileItems([failed]);
        await WaitUntilAsync(() => viewModel.PreviewStatus is PreviewStatus.Failed);
        Assert.AreEqual("Preview failed: decode-error", viewModel.PreviewAccessibilityName);
        CollectionAssert.IsSubsetOf(
            new[] { "Size", "Created", "Modified", "Accessed", "Attributes", "Type" },
            viewModel.DetailsMetadataFields.Select(field => field.Label).ToArray());
    }

    private static AppShellViewModel CreateViewModel(
        ThumbnailController? thumbnailController = null,
        PreviewController? previewController = null)
    {
        return new AppShellViewModel(
            new AppShellStartupState(
                "VeloFile",
                new AppShellCommandSurface(
                    "VeloFile",
                    NavigationWorkspace.Create(@"D:\projects"),
                    SidebarStateService.Create(FavoritesStatePayload.Empty, RecentLocationsStatePayload.Empty, drives: []),
                    VisibilitySettingsService.FromPayload(SettingsStatePayload.Default),
                    CrashRecoveryState.None,
                    new TestDefaultLaunchPathProvider(@"D:\projects"),
                    new TestPathExistenceProbe([@"D:\projects"]),
                    NoOpSettingsStateWriter.Instance,
                    () => DateTimeOffset.Parse("2026-05-05T00:00:00Z")),
                WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement)),
            previewController: previewController,
            thumbnailController: thumbnailController);
    }

    private static ListedFileItem Item(string name, bool isVisuallyDimmed = false)
    {
        return new ListedFileItem(
            @"D:\projects\" + name,
            name,
            name,
            FileSystemEntryKind.File,
            128,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            isVisuallyDimmed ? FileAttributes.Hidden | FileAttributes.System : FileAttributes.Archive,
            IsHidden: isVisuallyDimmed,
            IsProtectedOperatingSystemFile: isVisuallyDimmed,
            IsVisuallyDimmed: isVisuallyDimmed,
            CreationTimeUtc: new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            LastAccessTimeUtc: new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class ScriptedThumbnailProvider : IThumbnailProvider
    {
        private readonly Dictionary<string, ThumbnailProviderResult> _results = new(StringComparer.OrdinalIgnoreCase);

        public void SetResult(string name, ThumbnailProviderResult result)
        {
            _results[name] = result;
        }

        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(_results[item.Name]);
        }
    }

    private sealed class ScriptedPreviewProvider : IPreviewProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<PreviewProviderResult>> _results = new(StringComparer.OrdinalIgnoreCase);

        public PreviewOperation Operation => PreviewOperation.MetadataFallback;

        public void SetResult(string name, PreviewProviderResult result)
        {
            var completion = new TaskCompletionSource<PreviewProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            completion.SetResult(result);
            _results[name] = completion;
        }

        public void SetPending(string name)
        {
            _results[name] = new TaskCompletionSource<PreviewProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public bool CanPreview(PreviewRequest request)
        {
            return _results.ContainsKey(request.Item.Name);
        }

        public async ValueTask<PreviewProviderResult> PreviewAsync(
            PreviewRequest request,
            PreviewProviderContext context,
            CancellationToken cancellationToken)
        {
            return await _results[request.Item.Name].Task.ConfigureAwait(false);
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
        private readonly HashSet<string> _paths;

        public TestPathExistenceProbe(IEnumerable<string> paths)
        {
            _paths = paths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string path)
        {
            return _paths.Contains(path);
        }
    }
}
