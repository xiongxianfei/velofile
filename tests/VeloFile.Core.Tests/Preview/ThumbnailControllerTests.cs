using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.Core.Tests.Preview;

[TestClass]
[TestCategory("Thumbnails")]
public sealed class ThumbnailControllerTests
{
    [TestMethod]
    public async Task Thumbnails_controller_limits_concurrency_and_times_out_each_item_to_generic_icon()
    {
        var provider = new BlockingThumbnailProvider();
        var policy = new PreviewTimeoutPolicy(
            ImageDecodeBudget: TimeSpan.FromSeconds(2),
            TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
            PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
            MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
            ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(40),
            ThumbnailConcurrencyLimit: 4);
        var controller = new ThumbnailController(provider, policy);

        controller.Start(Enumerable.Range(0, 8).Select(index => Item($"file-{index}.txt")).ToArray());

        await WaitUntilAsync(() => provider.MaxConcurrentCount == 4);
        await WaitUntilAsync(() => controller.Snapshot.Count == 8
            && controller.Snapshot.Values.All(state => state.Status is ThumbnailStatus.GenericIcon));

        Assert.AreEqual(4, provider.MaxConcurrentCount);
        Assert.IsTrue(controller.Snapshot.Values.All(state => state.ReasonCode == "thumbnail-timeout"));
    }

    [TestMethod]
    public async Task Thumbnails_controller_times_out_noncooperative_provider_without_releasing_live_slot()
    {
        var provider = new NonCooperativeThumbnailProvider();
        var policy = new PreviewTimeoutPolicy(
            ImageDecodeBudget: TimeSpan.FromSeconds(2),
            TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
            PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
            MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
            ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(30),
            ThumbnailConcurrencyLimit: 2);
        var controller = new ThumbnailController(provider, policy);
        var first = Item("first.txt");
        var second = Item("second.txt");
        var third = Item("third.txt");

        controller.Start([first, second, third]);

        await WaitUntilAsync(() => provider.StartedCount == 2);
        await WaitUntilAsync(() => controller.GetState(first).ReasonCode == "thumbnail-timeout"
            && controller.GetState(second).ReasonCode == "thumbnail-timeout"
            && controller.GetState(third).ReasonCode == "thumbnail-timeout");

        Assert.AreEqual(2, provider.StartedCount);
        Assert.AreEqual(2, provider.LiveCount);
        Assert.AreEqual(2, provider.MaxConcurrentCount);
        Assert.AreEqual(ThumbnailStatus.GenericIcon, controller.GetState(third).Status);

        provider.Release("first.txt");
        await WaitUntilAsync(() => provider.LiveCount == 1);
        await Task.Delay(80);

        Assert.AreEqual(2, provider.StartedCount, "Queued requests that already reached their visible deadline must not start later.");
        Assert.AreEqual(2, provider.MaxConcurrentCount);
    }

    [TestMethod]
    public async Task Thumbnails_controller_counts_timed_out_provider_work_across_generations()
    {
        var provider = new NonCooperativeThumbnailProvider();
        var policy = new PreviewTimeoutPolicy(
            ImageDecodeBudget: TimeSpan.FromSeconds(2),
            TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
            PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
            MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
            ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(30),
            ThumbnailConcurrencyLimit: 4);
        var controller = new ThumbnailController(provider, policy);
        var firstGeneration = Enumerable.Range(0, 4)
            .Select(index => Item($"gen-a-{index}.txt"))
            .ToArray();
        var secondGeneration = Enumerable.Range(0, 4)
            .Select(index => Item($"gen-b-{index}.txt"))
            .ToArray();

        controller.Start(firstGeneration);

        await WaitUntilAsync(() => provider.StartedCount == 4);
        await WaitUntilAsync(() => firstGeneration.All(item => controller.GetState(item).ReasonCode == "thumbnail-timeout"));

        Assert.AreEqual(4, provider.LiveCount);

        controller.Start(secondGeneration);

        await WaitUntilAsync(() => secondGeneration.All(item => controller.GetState(item).ReasonCode == "thumbnail-timeout"));
        await Task.Delay(80);

        Assert.AreEqual(4, provider.StartedCount, "A superseding generation must not bypass live provider operations from an older generation.");
        Assert.AreEqual(4, provider.LiveCount);
        Assert.AreEqual(4, provider.MaxConcurrentCount);
        Assert.IsTrue(secondGeneration.All(item => controller.GetState(item).Status is ThumbnailStatus.GenericIcon));

        provider.Release("gen-a-0.txt");
        await WaitUntilAsync(() => provider.LiveCount == 3);
        await Task.Delay(80);

        Assert.AreEqual(4, provider.StartedCount, "Second-generation requests that already timed out visibly must not start after an old provider slot finally frees.");
        Assert.AreEqual(4, provider.MaxConcurrentCount);
    }

    [TestMethod]
    public async Task Thumbnails_controller_reuses_global_slot_for_new_generation_after_provider_completion()
    {
        var provider = new NonCooperativeThumbnailProvider();
        var controller = new ThumbnailController(
            provider,
            new PreviewTimeoutPolicy(
                ImageDecodeBudget: TimeSpan.FromSeconds(2),
                TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
                PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
                MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
                ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(30),
                ThumbnailConcurrencyLimit: 1));
        var first = Item("old-hung.txt");
        var stale = Item("stale-timeout.txt");
        var retry = Item("retry.txt");

        controller.Start([first]);
        await WaitUntilAsync(() => provider.Started("old-hung.txt"));
        await WaitUntilAsync(() => controller.GetState(first).ReasonCode == "thumbnail-timeout");

        controller.Start([stale]);
        await WaitUntilAsync(() => controller.GetState(stale).ReasonCode == "thumbnail-timeout");
        await Task.Delay(80);

        Assert.IsFalse(provider.Started("stale-timeout.txt"));

        provider.Release("old-hung.txt");
        await WaitUntilAsync(() => provider.LiveCount == 0);

        controller.Start([retry]);

        await WaitUntilAsync(() => provider.Started("retry.txt"));

        Assert.AreEqual(2, provider.StartedCount);
        Assert.AreEqual(1, provider.MaxConcurrentCount);
    }

    [TestMethod]
    public async Task Thumbnails_controller_ignores_late_success_after_visible_timeout()
    {
        var provider = new NonCooperativeThumbnailProvider();
        var controller = new ThumbnailController(
            provider,
            new PreviewTimeoutPolicy(
                ImageDecodeBudget: TimeSpan.FromSeconds(2),
                TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
                PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
                MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
                ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(30),
                ThumbnailConcurrencyLimit: 1));
        var item = Item("late.txt");

        controller.Start([item]);
        await WaitUntilAsync(() => controller.GetState(item).ReasonCode == "thumbnail-timeout");

        provider.Release("late.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("LATE")));
        await WaitUntilAsync(() => provider.LiveCount == 0);
        await Task.Delay(50);

        Assert.AreEqual(ThumbnailStatus.GenericIcon, controller.GetState(item).Status);
        Assert.AreEqual("thumbnail-timeout", controller.GetState(item).ReasonCode);
        Assert.AreNotEqual("LATE", controller.GetState(item).Artifact?.DisplayText);
    }

    [TestMethod]
    public async Task Thumbnails_controller_ignores_stale_results_after_new_generation()
    {
        var provider = new ManualThumbnailProvider();
        var controller = new ThumbnailController(
            provider,
            PreviewTimeoutPolicy.ForTesting(TimeSpan.FromSeconds(5)));
        var oldItem = Item("old.txt");
        var newItem = Item("new.txt");

        controller.Start([oldItem]);
        await WaitUntilAsync(() => provider.Started("old.txt"));

        controller.Start([newItem]);
        await WaitUntilAsync(() => provider.Cancelled("old.txt") && provider.Started("new.txt"));

        provider.Complete("old.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("OLD")));
        provider.Complete("new.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("NEW")));
        await WaitUntilAsync(() => controller.GetState(newItem).Artifact?.DisplayText == "NEW");

        Assert.AreEqual(ThumbnailStatus.Ready, controller.GetState(newItem).Status);
        Assert.AreEqual(ThumbnailStatus.NotLoaded, controller.GetState(oldItem).Status);
        Assert.IsFalse(controller.Snapshot.ContainsKey(oldItem.FullPath));
    }

    private static ListedFileItem Item(string name)
    {
        return new ListedFileItem(
            @"C:\thumbs\" + name,
            name,
            name,
            FileSystemEntryKind.File,
            128,
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

    private sealed class BlockingThumbnailProvider : IThumbnailProvider
    {
        private int _current;

        public int MaxConcurrentCount { get; private set; }

        public async ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _current);
            MaxConcurrentCount = Math.Max(MaxConcurrentCount, current);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon(item.Name));
            }
            finally
            {
                Interlocked.Decrement(ref _current);
            }
        }
    }

    private sealed class ManualThumbnailProvider : IThumbnailProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<ThumbnailProviderResult>> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<ThumbnailProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[item.Name] = completion;
            _started.Add(item.Name);
            cancellationToken.Register(() =>
            {
                _cancelled.Add(item.Name);
                completion.TrySetCanceled(cancellationToken);
            });
            return new ValueTask<ThumbnailProviderResult>(completion.Task);
        }

        public bool Started(string name)
        {
            return _started.Contains(name);
        }

        public bool Cancelled(string name)
        {
            return _cancelled.Contains(name);
        }

        public void Complete(string name, ThumbnailProviderResult result)
        {
            if (_pending.TryGetValue(name, out var completion))
            {
                completion.TrySetResult(result);
            }
        }
    }

    private sealed class NonCooperativeThumbnailProvider : IThumbnailProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<ThumbnailProviderResult>> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new();
        private int _live;
        private int _startedCount;

        public int StartedCount => _startedCount;

        public int LiveCount => _live;

        public int MaxConcurrentCount { get; private set; }

        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _startedCount);
            var live = Interlocked.Increment(ref _live);
            var completion = new TaskCompletionSource<ThumbnailProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_gate)
            {
                _started.Add(item.Name);
                _pending[item.Name] = completion;
                MaxConcurrentCount = Math.Max(MaxConcurrentCount, live);
            }

            return new ValueTask<ThumbnailProviderResult>(WaitForCompletionAsync(item.Name, completion.Task));
        }

        public bool Started(string name)
        {
            lock (_gate)
            {
                return _started.Contains(name);
            }
        }

        public void Release(string name)
        {
            Release(name, ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon(name)));
        }

        public void Release(string name, ThumbnailProviderResult result)
        {
            TaskCompletionSource<ThumbnailProviderResult>? completion;
            lock (_gate)
            {
                _pending.TryGetValue(name, out completion);
            }

            if (completion is not null)
            {
                completion.TrySetResult(result);
            }
        }

        private async Task<ThumbnailProviderResult> WaitForCompletionAsync(string name, Task<ThumbnailProviderResult> completion)
        {
            try
            {
                return await completion.ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _live);
            }
        }
    }
}
