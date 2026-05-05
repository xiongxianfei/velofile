using System.Security.Cryptography;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.Core.Tests.Preview;

[TestClass]
[TestCategory("PreviewContract")]
public sealed class PreviewContractTests
{
    [TestMethod]
    public void PreviewContract_default_timeout_policy_uses_R67_budgets()
    {
        var policy = PreviewTimeoutPolicy.Default;

        Assert.AreEqual(TimeSpan.FromSeconds(2), policy.GetBudget(PreviewOperation.ImageDecode));
        Assert.AreEqual(TimeSpan.FromSeconds(1), policy.GetBudget(PreviewOperation.TextReadAndEncodingDetection));
        Assert.AreEqual(TimeSpan.FromSeconds(3), policy.GetBudget(PreviewOperation.PdfFirstPageRender));
        Assert.AreEqual(TimeSpan.FromMilliseconds(500), policy.GetBudget(PreviewOperation.ThumbnailGeneration));
        Assert.AreEqual(4, policy.ThumbnailConcurrencyLimit);
    }

    [TestMethod]
    [DataRow(PreviewOperation.ImageDecode, 21)]
    [DataRow(PreviewOperation.TextReadAndEncodingDetection, 22)]
    [DataRow(PreviewOperation.PdfFirstPageRender, 23)]
    public async Task PreviewContract_controller_supplies_selected_provider_operation_budget(
        PreviewOperation operation,
        int expectedBudgetMs)
    {
        var provider = new ScriptedPreviewProvider(operation);
        provider.SetResult(@"D:\docs\sample.dat", PreviewProviderResult.Success(PreviewContent.Text("ok", truncated: false)));
        var controller = CreateController(
            provider,
            loadingDelayMs: 200,
            timeoutPolicy: DistinctTimeoutPolicy());

        controller.StartPreview(Item(@"D:\docs\sample.dat", "sample.dat"));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success);

        Assert.AreEqual(operation, provider.LastContext?.Operation);
        Assert.AreEqual(TimeSpan.FromMilliseconds(expectedBudgetMs), provider.LastContext?.TimeoutBudget);
    }

    [TestMethod]
    [DataRow(PreviewOperation.ImageDecode)]
    [DataRow(PreviewOperation.TextReadAndEncodingDetection)]
    [DataRow(PreviewOperation.PdfFirstPageRender)]
    public async Task PreviewContract_provider_specific_timeout_uses_selected_operation_budget(PreviewOperation operation)
    {
        var provider = new ScriptedPreviewProvider(operation);
        provider.SetPending(@"D:\docs\slow.dat");
        var controller = CreateController(
            provider,
            loadingDelayMs: 5,
            timeoutPolicy: TimeoutPolicyWithBudget(operation, TimeSpan.FromMilliseconds(40)));

        controller.StartPreview(Item(@"D:\docs\slow.dat", "slow.dat", length: 12));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Failed);

        Assert.AreEqual("timeout", controller.State.ReasonCode);
        Assert.AreEqual(TimeSpan.FromMilliseconds(40), provider.LastContext?.TimeoutBudget);
        Assert.IsTrue(provider.WasCancellationRequested(@"D:\docs\slow.dat"));
    }

    [TestMethod]
    public async Task PreviewContract_paged_pdf_preview_renders_later_pages_only_after_request()
    {
        var provider = new ScriptedPagedPreviewProvider();
        var controller = CreateController(
            provider,
            loadingDelayMs: 200,
            timeoutPolicy: DistinctTimeoutPolicy());

        controller.StartPreview(Item(@"D:\docs\paper.pdf", "paper.pdf", length: 2048));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success);

        CollectionAssert.AreEqual(new[] { 1 }, provider.RequestedPages.ToArray());
        Assert.AreEqual(1, controller.State.Content?.PdfPageArtifact?.PageNumber);
        Assert.AreEqual(3, controller.State.Content?.PdfPageArtifact?.PageCount);
        Assert.AreEqual(TimeSpan.FromMilliseconds(23), provider.Contexts.Single().TimeoutBudget);

        Assert.IsTrue(controller.RequestPreviewPage(2));
        await WaitUntilAsync(() => controller.State.Content?.PdfPageArtifact?.PageNumber == 2);

        CollectionAssert.AreEqual(new[] { 1, 2 }, provider.RequestedPages.ToArray());
        Assert.AreEqual(TimeSpan.FromMilliseconds(23), provider.Contexts.Last().TimeoutBudget);
    }

    [TestMethod]
    public async Task PreviewContract_fast_success_skips_loading_and_keeps_metadata()
    {
        var provider = new ScriptedPreviewProvider();
        provider.SetResult(@"D:\docs\readme.txt", PreviewProviderResult.Success(PreviewContent.Text("hello", truncated: false)));
        var controller = CreateController(provider, loadingDelayMs: 200, timeoutMs: 500);
        var history = RecordHistory(controller);

        controller.StartPreview(Item(@"D:\docs\readme.txt", "readme.txt", length: 42));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success);

        CollectionAssert.AreEqual(
            new[] { PreviewStatus.Empty, PreviewStatus.Success },
            history.Select(state => state.Status).ToArray());
        Assert.AreEqual("readme.txt", controller.State.Metadata?.Name);
        Assert.AreEqual(42, controller.State.Metadata?.SizeBytes);
        Assert.AreEqual("hello", controller.State.Content?.TextContent);
    }

    [TestMethod]
    [DataRow(PreviewOperation.ImageDecode, "sample.png")]
    [DataRow(PreviewOperation.TextReadAndEncodingDetection, "sample.txt")]
    [DataRow(PreviewOperation.PdfFirstPageRender, "sample.pdf")]
    public async Task PreviewContract_provider_path_does_not_modify_source_file(
        PreviewOperation operation,
        string fileName)
    {
        using var file = ScratchPreviewFile.Create(fileName, "VeloFile preview non-mutation marker.");
        var provider = new FileReadPreviewProvider(operation);
        var controller = CreateController(provider, loadingDelayMs: 10, timeoutMs: 500);

        controller.StartPreview(file.ToListedFileItem());
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success);

        file.AssertUnchanged();
        Assert.AreEqual("Preview read " + file.Length + " bytes.", controller.State.Content?.TextContent);
    }

    [TestMethod]
    public async Task PreviewContract_unsupported_metadata_fallback_does_not_modify_source_and_shows_standard_metadata()
    {
        using var file = ScratchPreviewFile.Create("unsupported.velofallback", "metadata fallback marker");
        var controller = CreateController(new MetadataOnlyPreviewProvider(), loadingDelayMs: 10, timeoutMs: 500);

        controller.StartPreview(file.ToListedFileItem());
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Unsupported);

        file.AssertUnchanged();
        var fields = controller.State.Metadata?.Fields() ?? [];
        CollectionAssert.IsSubsetOf(
            new[] { "Size", "Created", "Modified", "Accessed", "Attributes", "Type" },
            fields.Select(field => field.Label).ToArray());
        Assert.IsTrue(fields.Any(field => field.Label == "Size" && field.Value.Contains(file.Length.ToString(), StringComparison.Ordinal)));
        AssertField(fields, "Created", file.CreationDisplay);
        AssertField(fields, "Modified", file.ModifiedDisplay);
        AssertField(fields, "Accessed", file.AccessedDisplay);
        Assert.IsTrue(fields.Any(field => field.Label == "Attributes" && field.Value.Contains(nameof(FileAttributes.ReadOnly), StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task PreviewContract_metadata_fallback_handles_unavailable_metadata_without_losing_available_fields()
    {
        var controller = CreateController(new MetadataOnlyPreviewProvider(), loadingDelayMs: 10, timeoutMs: 500);
        var item = new ListedFileItem(
            @"D:\docs\metadata-only.unknown",
            "metadata-only.unknown",
            "metadata-only.unknown",
            FileSystemEntryKind.File,
            Length: null,
            LastWriteTimeUtc: new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);

        controller.StartPreview(item);
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Unsupported);

        var fields = controller.State.Metadata?.Fields() ?? [];
        AssertField(fields, "Size", "Unknown");
        AssertField(fields, "Created", "Unknown");
        AssertField(fields, "Modified", "2026-02-03 04:05:06Z");
        AssertField(fields, "Accessed", "Unknown");
        AssertField(fields, "Attributes", nameof(FileAttributes.Archive));
        Assert.IsTrue(fields.Any(field => field.Label == "Type" && field.Value.Contains(".unknown", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task PreviewContract_loading_appears_after_delay_and_timeout_fails_with_metadata()
    {
        var provider = new ScriptedPreviewProvider();
        provider.SetPending(@"D:\docs\slow.txt");
        var controller = CreateController(provider, loadingDelayMs: 40, timeoutMs: 120);

        controller.StartPreview(Item(@"D:\docs\slow.txt", "slow.txt", length: 12));

        Assert.AreEqual(PreviewStatus.Empty, controller.State.Status);
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Loading);
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Failed);

        Assert.AreEqual("timeout", controller.State.ReasonCode);
        Assert.AreEqual("slow.txt", controller.State.Metadata?.Name);
        Assert.IsTrue(provider.WasCancellationRequested(@"D:\docs\slow.txt"));
    }

    [TestMethod]
    public async Task PreviewContract_selection_change_cancels_old_work_and_ignores_late_completion()
    {
        var provider = new ScriptedPreviewProvider();
        provider.SetPending(@"D:\docs\old.txt");
        provider.SetResult(@"D:\docs\new.txt", PreviewProviderResult.Success(PreviewContent.Text("new", truncated: false)));
        var controller = CreateController(provider, loadingDelayMs: 200, timeoutMs: 500);

        controller.StartPreview(Item(@"D:\docs\old.txt", "old.txt"));
        await WaitUntilAsync(() => provider.Started(@"D:\docs\old.txt"));
        controller.StartPreview(Item(@"D:\docs\new.txt", "new.txt"));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success);
        provider.Complete(@"D:\docs\old.txt", PreviewProviderResult.Success(PreviewContent.Text("old", truncated: false)));
        await Task.Delay(50);

        Assert.IsTrue(provider.WasCancellationRequested(@"D:\docs\old.txt"));
        Assert.AreEqual("new.txt", controller.State.Metadata?.Name);
        Assert.AreEqual("new", controller.State.Content?.TextContent);
    }

    [TestMethod]
    public async Task PreviewContract_unsupported_and_failure_are_terminal_states_with_metadata_fallback()
    {
        var provider = new ScriptedPreviewProvider();
        provider.SetResult(@"D:\docs\unknown.bin", PreviewProviderResult.Unsupported("unsupported"));
        provider.SetResult(@"D:\docs\corrupt.txt", PreviewProviderResult.Failed("decode-error"));
        var controller = CreateController(provider, loadingDelayMs: 20, timeoutMs: 500);

        controller.StartPreview(Item(@"D:\docs\unknown.bin", "unknown.bin", length: 4));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Unsupported);
        Assert.AreEqual("unsupported", controller.State.ReasonCode);
        Assert.AreEqual("unknown.bin", controller.State.Metadata?.Name);

        controller.StartPreview(Item(@"D:\docs\corrupt.txt", "corrupt.txt", length: 7));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Failed);
        Assert.AreEqual("decode-error", controller.State.ReasonCode);
        Assert.AreEqual("corrupt.txt", controller.State.Metadata?.Name);
    }

    [TestMethod]
    public async Task PreviewContract_failures_emit_redacted_preview_diagnostics()
    {
        var provider = new ScriptedPreviewProvider();
        provider.SetResult(@"C:\Users\alice\Secret\budget.txt", PreviewProviderResult.Failed("access-denied"));
        var diagnostics = new CollectingDiagnosticSink();
        var controller = CreateController(provider, loadingDelayMs: 20, timeoutMs: 500, diagnostics);

        controller.StartPreview(Item(@"C:\Users\alice\Secret\budget.txt", "budget.txt"));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Failed);

        Assert.HasCount(1, diagnostics.Events);
        Assert.AreEqual("preview", diagnostics.Events[0].Component);
        Assert.AreEqual("preview", diagnostics.Events[0].OperationKind);
        Assert.AreEqual("access-denied", diagnostics.Events[0].ReasonCode);
        var serialized = DiagnosticJsonSerializer.Serialize(diagnostics.Events[0]);
        Assert.IsFalse(serialized.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(serialized.Contains("budget.txt", StringComparison.OrdinalIgnoreCase));
    }

    private static PreviewController CreateController(
        IPreviewProvider provider,
        int loadingDelayMs,
        int timeoutMs,
        IDiagnosticSink? diagnostics = null)
    {
        return CreateController(
            provider,
            loadingDelayMs,
            PreviewTimeoutPolicy.ForTesting(TimeSpan.FromMilliseconds(timeoutMs)),
            diagnostics);
    }

    private static PreviewController CreateController(
        IPreviewProvider provider,
        int loadingDelayMs,
        PreviewTimeoutPolicy timeoutPolicy,
        IDiagnosticSink? diagnostics = null)
    {
        return new PreviewController(
            new[] { provider },
            new PreviewMetadataProvider(),
            new PreviewControllerOptions(
                TimeSpan.FromMilliseconds(loadingDelayMs),
                timeoutPolicy),
            diagnostics,
            new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF")));
    }

    private static PreviewTimeoutPolicy DistinctTimeoutPolicy()
    {
        return new PreviewTimeoutPolicy(
            ImageDecodeBudget: TimeSpan.FromMilliseconds(21),
            TextReadAndEncodingDetectionBudget: TimeSpan.FromMilliseconds(22),
            PdfFirstPageRenderBudget: TimeSpan.FromMilliseconds(23),
            MetadataFallbackBudget: TimeSpan.FromMilliseconds(24),
            ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(25),
            ThumbnailConcurrencyLimit: 4);
    }

    private static PreviewTimeoutPolicy TimeoutPolicyWithBudget(PreviewOperation operation, TimeSpan budget)
    {
        var fallback = TimeSpan.FromSeconds(5);
        return new PreviewTimeoutPolicy(
            ImageDecodeBudget: operation is PreviewOperation.ImageDecode ? budget : fallback,
            TextReadAndEncodingDetectionBudget: operation is PreviewOperation.TextReadAndEncodingDetection ? budget : fallback,
            PdfFirstPageRenderBudget: operation is PreviewOperation.PdfFirstPageRender ? budget : fallback,
            MetadataFallbackBudget: operation is PreviewOperation.MetadataFallback ? budget : fallback,
            ThumbnailGenerationBudget: operation is PreviewOperation.ThumbnailGeneration ? budget : fallback,
            ThumbnailConcurrencyLimit: 4);
    }

    private static List<PreviewState> RecordHistory(PreviewController controller)
    {
        var history = new List<PreviewState>();
        controller.StateChanged += (_, _) => history.Add(controller.State);
        return history;
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
            IsVisuallyDimmed: false);
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

    private static void AssertField(IReadOnlyList<PreviewMetadataField> fields, string label, string expectedValue)
    {
        Assert.IsTrue(
            fields.Any(field => field.Label == label && string.Equals(field.Value, expectedValue, StringComparison.Ordinal)),
            $"Expected preview metadata field '{label}' to equal '{expectedValue}'.");
    }

    private sealed class ScriptedPreviewProvider : IPreviewProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<PreviewProviderResult>> _results = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

        public ScriptedPreviewProvider(PreviewOperation operation = PreviewOperation.MetadataFallback)
        {
            Operation = operation;
        }

        public PreviewOperation Operation { get; }

        public PreviewProviderContext? LastContext { get; private set; }

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

        public void Complete(string path, PreviewProviderResult result)
        {
            _results[path].TrySetResult(result);
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
            LastContext = context;
            using var _ = cancellationToken.Register(() => _cancelled.Add(request.Item.FullPath));
            return await _results[request.Item.FullPath].Task.ConfigureAwait(false);
        }
    }

    private sealed class FileReadPreviewProvider : IPreviewProvider
    {
        public FileReadPreviewProvider(PreviewOperation operation)
        {
            Operation = operation;
        }

        public PreviewOperation Operation { get; }

        public bool CanPreview(PreviewRequest request)
        {
            return true;
        }

        public async ValueTask<PreviewProviderResult> PreviewAsync(
            PreviewRequest request,
            PreviewProviderContext context,
            CancellationToken cancellationToken)
        {
            var bytes = await File.ReadAllBytesAsync(request.Item.FullPath, cancellationToken).ConfigureAwait(false);
            return PreviewProviderResult.Success(PreviewContent.Text("Preview read " + bytes.Length + " bytes.", truncated: false));
        }
    }

    private sealed class ScriptedPagedPreviewProvider : IPagedPreviewProvider
    {
        public PreviewOperation Operation => PreviewOperation.PdfFirstPageRender;

        public List<int> RequestedPages { get; } = [];

        public List<PreviewProviderContext> Contexts { get; } = [];

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
            Contexts.Add(context);
            return ValueTask.FromResult(PreviewProviderResult.Success(PreviewContent.PdfPage(new PdfPagePreviewArtifact(
                PageNumber: pageNumber,
                PageCount: 3,
                PixelWidth: 100,
                PixelHeight: 80,
                EncodedFormat: "png",
                EncodedBytes: [1, 2, 3],
                SourceWasDownsampled: false))));
        }
    }

    private sealed class ScratchPreviewFile : IDisposable
    {
        private readonly string _root;
        private readonly string _hash;
        private readonly DateTime _creationTimeUtc = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        private readonly DateTime _lastWriteTimeUtc = new(2026, 1, 3, 4, 5, 6, DateTimeKind.Utc);
        private readonly DateTime _lastAccessTimeUtc = new(2026, 1, 4, 5, 6, 7, DateTimeKind.Utc);
        private readonly FileAttributes _attributes = FileAttributes.Archive | FileAttributes.ReadOnly;

        private ScratchPreviewFile(string root, string path, string hash, long length)
        {
            _root = root;
            Path = path;
            _hash = hash;
            Length = length;
        }

        public string Path { get; }

        public long Length { get; }

        public string CreationDisplay => _creationTimeUtc.ToString("u");

        public string ModifiedDisplay => _lastWriteTimeUtc.ToString("u");

        public string AccessedDisplay => _lastAccessTimeUtc.ToString("u");

        public static ScratchPreviewFile Create(string fileName, string content)
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "velofile-preview-contract-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var path = System.IO.Path.Combine(root, fileName);
            File.WriteAllText(path, content);

            var fixture = new ScratchPreviewFile(root, path, Hash(path), new FileInfo(path).Length);
            File.SetCreationTimeUtc(path, fixture._creationTimeUtc);
            File.SetLastWriteTimeUtc(path, fixture._lastWriteTimeUtc);
            File.SetLastAccessTimeUtc(path, fixture._lastAccessTimeUtc);
            File.SetAttributes(path, fixture._attributes);
            return fixture;
        }

        public ListedFileItem ToListedFileItem()
        {
            var info = new FileInfo(Path);
            return new ListedFileItem(
                Path,
                info.Name,
                info.Name,
                FileSystemEntryKind.File,
                info.Length,
                info.LastWriteTimeUtc,
                info.Attributes,
                IsHidden: info.Attributes.HasFlag(FileAttributes.Hidden),
                IsProtectedOperatingSystemFile: false,
                IsVisuallyDimmed: false,
                CreationTimeUtc: info.CreationTimeUtc,
                LastAccessTimeUtc: info.LastAccessTimeUtc);
        }

        public void AssertUnchanged()
        {
            var info = new FileInfo(Path);
            Assert.AreEqual(Length, info.Length);
            Assert.AreEqual(_hash, Hash(Path));
            Assert.AreEqual(_creationTimeUtc, info.CreationTimeUtc);
            Assert.AreEqual(_lastWriteTimeUtc, info.LastWriteTimeUtc);
            Assert.AreEqual(_attributes, info.Attributes);
        }

        public void Dispose()
        {
            try
            {
                File.SetAttributes(Path, FileAttributes.Normal);
            }
            catch
            {
            }

            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch
            {
            }
        }

        private static string Hash(string path)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
    }
}
