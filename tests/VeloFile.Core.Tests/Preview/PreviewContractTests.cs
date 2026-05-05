using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.Core.Tests.Preview;

[TestClass]
[TestCategory("PreviewContract")]
public sealed class PreviewContractTests
{
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

        Assert.AreEqual(1, diagnostics.Events.Count);
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
        return new PreviewController(
            new[] { provider },
            new PreviewMetadataProvider(),
            new PreviewControllerOptions(
                TimeSpan.FromMilliseconds(loadingDelayMs),
                TimeSpan.FromMilliseconds(timeoutMs)),
            diagnostics,
            new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF")));
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

    private sealed class ScriptedPreviewProvider : IPreviewProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<PreviewProviderResult>> _results = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

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

        public async ValueTask<PreviewProviderResult> PreviewAsync(PreviewRequest request, CancellationToken cancellationToken)
        {
            _started.Add(request.Item.FullPath);
            using var _ = cancellationToken.Register(() => _cancelled.Add(request.Item.FullPath));
            return await _results[request.Item.FullPath].Task.ConfigureAwait(false);
        }
    }
}
