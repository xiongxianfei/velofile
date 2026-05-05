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
[TestCategory("PreviewContract")]
public sealed class AppShellPreviewTests
{
    [TestMethod]
    public async Task PreviewContract_preview_toggle_and_selection_start_preview_state()
    {
        var provider = new AppPreviewProvider();
        provider.SetResult(@"D:\projects\alpha.txt", PreviewProviderResult.Unsupported("unsupported"));
        var preview = CreatePreviewController(provider);
        var viewModel = CreateViewModel(preview);
        var item = Item(@"D:\projects\alpha.txt", "alpha.txt", length: 64);
        viewModel.SetFileItems([item]);

        viewModel.TogglePreviewPane();
        viewModel.SetSelectedFileItems([item]);
        await WaitUntilAsync(() => viewModel.PreviewStatus is PreviewStatus.Unsupported);

        Assert.IsTrue(viewModel.IsPreviewPaneOpen);
        Assert.AreEqual("Preview unsupported: unsupported", viewModel.PreviewStatusText);
        CollectionAssert.IsSubsetOf(
            new[] { "Size", "Created", "Modified", "Accessed", "Attributes", "Type" },
            viewModel.PreviewMetadataFields.Select(field => field.Label).ToArray());
    }

    [TestMethod]
    public async Task PreviewContract_selection_change_clears_previous_preview_immediately()
    {
        var provider = new AppPreviewProvider();
        provider.SetResult(@"D:\projects\old.txt", PreviewProviderResult.Success(PreviewContent.Text("old", truncated: false)));
        provider.SetPending(@"D:\projects\new.txt");
        var preview = CreatePreviewController(provider);
        var viewModel = CreateViewModel(preview);
        var oldItem = Item(@"D:\projects\old.txt", "old.txt");
        var newItem = Item(@"D:\projects\new.txt", "new.txt");
        viewModel.SetFileItems([oldItem, newItem]);
        viewModel.TogglePreviewPane();
        viewModel.SetSelectedFileItems([oldItem]);
        await WaitUntilAsync(() => viewModel.PreviewStatus is PreviewStatus.Success);

        viewModel.SetSelectedFileItems([newItem]);

        Assert.AreEqual(PreviewStatus.Empty, viewModel.PreviewStatus);
        Assert.AreEqual("", viewModel.PreviewStatusText);
        Assert.IsTrue(provider.WasCancellationRequested(@"D:\projects\old.txt") || provider.Started(@"D:\projects\new.txt"));
    }

    [TestMethod]
    public void PreviewContract_closing_preview_pane_clears_selection_preview_state()
    {
        var provider = new AppPreviewProvider();
        provider.SetPending(@"D:\projects\alpha.txt");
        var preview = CreatePreviewController(provider);
        var viewModel = CreateViewModel(preview);
        var item = Item(@"D:\projects\alpha.txt", "alpha.txt");
        viewModel.SetFileItems([item]);
        viewModel.TogglePreviewPane();
        viewModel.SetSelectedFileItems([item]);

        viewModel.TogglePreviewPane();

        Assert.IsFalse(viewModel.IsPreviewPaneOpen);
        Assert.AreEqual(PreviewStatus.Empty, viewModel.PreviewStatus);
        Assert.AreEqual("", viewModel.PreviewStatusText);
    }

    [TestMethod]
    public async Task PreviewContract_pdf_page_navigation_is_exposed_through_shell_view_model()
    {
        var provider = new AppPagedPreviewProvider();
        var preview = CreatePreviewController(provider);
        var viewModel = CreateViewModel(preview);
        var item = Item(@"D:\projects\paper.pdf", "paper.pdf", length: 2048);
        viewModel.SetFileItems([item]);

        viewModel.TogglePreviewPane();
        viewModel.SetSelectedFileItems([item]);
        await WaitUntilAsync(() => viewModel.PreviewStatus is PreviewStatus.Success);

        CollectionAssert.AreEqual(new[] { 1 }, provider.RequestedPages.ToArray());
        Assert.AreEqual(1, viewModel.CurrentPdfPageNumber);
        Assert.AreEqual(3, viewModel.PdfPageCount);
        Assert.IsTrue(viewModel.CanNavigatePdfPages);

        Assert.IsTrue(viewModel.RequestPdfPage(2));
        await WaitUntilAsync(() => viewModel.CurrentPdfPageNumber == 2);

        CollectionAssert.AreEqual(new[] { 1, 2 }, provider.RequestedPages.ToArray());
        Assert.AreEqual("PDF page 2 of 3", viewModel.PreviewContentText);
    }

    private static PreviewController CreatePreviewController(IPreviewProvider provider)
    {
        return new PreviewController(
            new[] { provider },
            new PreviewMetadataProvider(),
            new PreviewControllerOptions(
                TimeSpan.FromMilliseconds(20),
                PreviewTimeoutPolicy.ForTesting(TimeSpan.FromMilliseconds(500))));
    }

    private static AppShellViewModel CreateViewModel(PreviewController previewController)
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
            previewController: previewController);
    }

    private static ListedFileItem Item(string path, string name, long? length = null)
    {
        return new ListedFileItem(
            path,
            name,
            name,
            FileSystemEntryKind.File,
            length,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false,
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

    private sealed class AppPreviewProvider : IPreviewProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<PreviewProviderResult>> _results = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

        public PreviewOperation Operation => PreviewOperation.MetadataFallback;

        public bool CanPreview(PreviewRequest request)
        {
            return _results.ContainsKey(request.Item.FullPath);
        }

        public void SetResult(string path, PreviewProviderResult result)
        {
            var completion = new TaskCompletionSource<PreviewProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            completion.SetResult(result);
            _results[path] = completion;
        }

        public void SetPending(string path)
        {
            _results[path] = new TaskCompletionSource<PreviewProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public bool Started(string path)
        {
            return _started.Contains(path);
        }

        public bool WasCancellationRequested(string path)
        {
            return _cancelled.Contains(path);
        }

        public async ValueTask<PreviewProviderResult> PreviewAsync(
            PreviewRequest request,
            PreviewProviderContext context,
            CancellationToken cancellationToken)
        {
            _started.Add(request.Item.FullPath);
            using var _ = cancellationToken.Register(() => _cancelled.Add(request.Item.FullPath));
            return await _results[request.Item.FullPath].Task.ConfigureAwait(false);
        }
    }

    private sealed class AppPagedPreviewProvider : IPagedPreviewProvider
    {
        public PreviewOperation Operation => PreviewOperation.PdfFirstPageRender;

        public List<int> RequestedPages { get; } = [];

        public bool CanPreview(PreviewRequest request)
        {
            return string.Equals(request.Item.Name, "paper.pdf", StringComparison.OrdinalIgnoreCase);
        }

        public ValueTask<PreviewProviderResult> PreviewAsync(
            PreviewRequest request,
            PreviewProviderContext context,
            CancellationToken cancellationToken)
        {
            return PreviewPageAsync(request, pageNumber: 1, context, cancellationToken);
        }

        public ValueTask<PreviewProviderResult> PreviewPageAsync(
            PreviewRequest request,
            int pageNumber,
            PreviewProviderContext context,
            CancellationToken cancellationToken)
        {
            RequestedPages.Add(pageNumber);
            return ValueTask.FromResult(PreviewProviderResult.Success(PreviewContent.PdfPage(new PdfPagePreviewArtifact(
                PageNumber: pageNumber,
                PageCount: 3,
                PixelWidth: 200,
                PixelHeight: 120,
                EncodedFormat: "png",
                EncodedBytes: [1, 2, 3],
                SourceWasDownsampled: false))));
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
