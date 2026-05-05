using VeloFile.Core.Listing;
using VeloFile.Core.Operations;
using VeloFile.Windows.Shell;

#pragma warning disable CA1416, MSTEST0037

namespace VeloFile.Windows.Tests.ShellOperations;

[TestClass]
[TestCategory("Operations")]
public sealed class WindowsShellFileOperationAdapterTests
{
    [TestMethod]
    public void Recycle_bin_delete_maps_to_shell_recycle_intent_not_permanent_delete()
    {
        var request = FileOperationRequest.RecycleBinDelete([Item(@"D:\scratch\delete-me.txt", "delete-me.txt")]);

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.Delete, intent.Kind);
        Assert.AreEqual(WindowsShellDeleteDisposition.RecycleBin, intent.DeleteDisposition);
        Assert.IsFalse(intent.AllowUndoBypassingDelete);
    }

    [TestMethod]
    public void Permanent_delete_requires_confirmed_permanent_delete_request()
    {
        var unconfirmed = new FileOperationRequest(
            FileOperationKind.PermanentDelete,
            [FileOperationTarget.FromListedItem(Item(@"D:\scratch\delete-me.txt", "delete-me.txt"))],
            TargetName: null,
            ConfirmedPermanentDelete: false);

        Assert.ThrowsExactly<InvalidOperationException>(() => WindowsShellFileOperationRequestMapper.Map(unconfirmed));

        var confirmed = FileOperationRequest.PermanentDelete(
            [Item(@"D:\scratch\delete-me.txt", "delete-me.txt")],
            confirmed: true);
        var intent = WindowsShellFileOperationRequestMapper.Map(confirmed);

        Assert.AreEqual(WindowsShellDeleteDisposition.Permanent, intent.DeleteDisposition);
    }

    [TestMethod]
    public void Rename_maps_to_shell_rename_intent_with_target_name()
    {
        var request = FileOperationRequest.Rename(Item(@"D:\scratch\old.txt", "old.txt"), "new.txt");

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.Rename, intent.Kind);
        Assert.AreEqual("new.txt", intent.TargetName);
        Assert.AreEqual(@"D:\scratch\old.txt", intent.Targets.Single().Path);
    }

    [TestMethod]
    public void Copy_maps_to_shell_copy_intent_with_target_directory()
    {
        var request = FileOperationRequest.Copy([Item(@"D:\scratch\source\copy-me.txt", "copy-me.txt")], @"D:\scratch\target");

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.Copy, intent.Kind);
        Assert.AreEqual(@"D:\scratch\target", intent.TargetDirectory);
        Assert.AreEqual(WindowsShellConflictChoice.None, intent.ConflictChoice);
    }

    [TestMethod]
    public void Move_maps_to_shell_move_intent_with_conflict_choice()
    {
        var request = FileOperationRequest.Move(
            [Item(@"D:\scratch\source\move-me.txt", "move-me.txt")],
            @"D:\scratch\target",
            FileOperationConflictChoice.KeepBoth);

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.Move, intent.Kind);
        Assert.AreEqual(@"D:\scratch\target", intent.TargetDirectory);
        Assert.AreEqual(WindowsShellConflictChoice.KeepBoth, intent.ConflictChoice);
    }

    [TestMethod]
    public void Create_shortcut_maps_to_shell_shortcut_intent_with_target_directory()
    {
        var request = FileOperationRequest.CreateShortcuts(
            [Item(@"D:\scratch\source\report.txt", "report.txt")],
            @"D:\scratch\target");

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.CreateShortcut, intent.Kind);
        Assert.AreEqual(@"D:\scratch\target", intent.TargetDirectory);
        Assert.AreEqual(@"D:\scratch\source\report.txt", intent.Targets.Single().Path);
    }

    [TestMethod]
    public async Task Copy_to_existing_target_returns_conflict_without_executing_copy()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor();
        var collisionProbe = new StaticFileOperationCollisionProbe(hasCollision: true);
        var adapter = new WindowsShellFileOperationAdapter(collisionProbe: collisionProbe, executor: executor);
        var request = FileOperationRequest.Copy([Item(@"D:\scratch\source\copy-me.txt", "copy-me.txt")], @"D:\scratch\target");

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.ConflictRequired, result.Status);
        Assert.AreEqual("copy-me.txt", result.Conflict?.ExistingName);
        Assert.AreEqual(0, executor.Intents.Count);
    }

    [TestMethod]
    public async Task Move_replace_conflict_executes_move_with_replace_choice()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor();
        var collisionProbe = new StaticFileOperationCollisionProbe(hasCollision: true);
        var adapter = new WindowsShellFileOperationAdapter(collisionProbe: collisionProbe, executor: executor);
        var request = FileOperationRequest.Move(
            [Item(@"D:\scratch\source\move-me.txt", "move-me.txt")],
            @"D:\scratch\target",
            FileOperationConflictChoice.Replace);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.Completed, result.Status);
        Assert.AreEqual(1, executor.Intents.Count);
        Assert.AreEqual(WindowsShellConflictChoice.Replace, executor.Intents.Single().ConflictChoice);
    }

    [TestMethod]
    public void Copy_skip_conflict_still_copies_unaffected_batch_items()
    {
        using var scratch = ScratchWorkspace.Create();
        var source = scratch.CreateDirectory("source");
        var target = scratch.CreateDirectory("target");
        var collidingSource = scratch.WriteFile(Path.Combine("source", "existing.txt"), "incoming");
        var unaffectedSource = scratch.WriteFile(Path.Combine("source", "fresh.txt"), "fresh");
        var existingTarget = scratch.WriteFile(Path.Combine("target", "existing.txt"), "existing");
        var intent = new WindowsShellFileOperationIntent(
            WindowsShellFileOperationKind.Copy,
            [
                FileOperationTarget.FromListedItem(Item(collidingSource, "existing.txt")),
                FileOperationTarget.FromListedItem(Item(unaffectedSource, "fresh.txt"))
            ],
            TargetName: null,
            target,
            WindowsShellConflictChoice.Skip,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);

        VisualBasicShellFileOperationExecutor.Instance.Execute(intent, progress: null, CancellationToken.None);

        Assert.AreEqual("existing", File.ReadAllText(existingTarget));
        Assert.AreEqual("fresh", File.ReadAllText(Path.Combine(target, "fresh.txt")));
    }

    [TestMethod]
    public void Move_skip_conflict_still_moves_unaffected_batch_items()
    {
        using var scratch = ScratchWorkspace.Create();
        var source = scratch.CreateDirectory("source");
        var target = scratch.CreateDirectory("target");
        var collidingSource = scratch.WriteFile(Path.Combine("source", "existing.txt"), "incoming");
        var unaffectedSource = scratch.WriteFile(Path.Combine("source", "fresh.txt"), "fresh");
        var existingTarget = scratch.WriteFile(Path.Combine("target", "existing.txt"), "existing");
        var intent = new WindowsShellFileOperationIntent(
            WindowsShellFileOperationKind.Move,
            [
                FileOperationTarget.FromListedItem(Item(collidingSource, "existing.txt")),
                FileOperationTarget.FromListedItem(Item(unaffectedSource, "fresh.txt"))
            ],
            TargetName: null,
            target,
            WindowsShellConflictChoice.Skip,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);

        VisualBasicShellFileOperationExecutor.Instance.Execute(intent, progress: null, CancellationToken.None);

        Assert.AreEqual("existing", File.ReadAllText(existingTarget));
        Assert.IsTrue(File.Exists(collidingSource));
        Assert.IsFalse(File.Exists(unaffectedSource));
        Assert.AreEqual("fresh", File.ReadAllText(Path.Combine(target, "fresh.txt")));
    }

    [TestMethod]
    public void Copy_keep_both_preserves_existing_target_and_writes_incoming_to_distinct_name()
    {
        using var scratch = ScratchWorkspace.Create();
        var source = scratch.CreateDirectory("source");
        var target = scratch.CreateDirectory("target");
        var incomingSource = scratch.WriteFile(Path.Combine("source", "report.txt"), "incoming");
        var existingTarget = scratch.WriteFile(Path.Combine("target", "report.txt"), "existing");
        var intent = new WindowsShellFileOperationIntent(
            WindowsShellFileOperationKind.Copy,
            [FileOperationTarget.FromListedItem(Item(incomingSource, "report.txt"))],
            TargetName: null,
            target,
            WindowsShellConflictChoice.KeepBoth,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);

        VisualBasicShellFileOperationExecutor.Instance.Execute(intent, progress: null, CancellationToken.None);

        Assert.AreEqual("existing", File.ReadAllText(existingTarget));
        var destinationFiles = Directory.GetFiles(target, "*.txt");
        Assert.AreEqual(2, destinationFiles.Length);
        var incomingDestination = destinationFiles.Single(path => !string.Equals(path, existingTarget, StringComparison.OrdinalIgnoreCase));
        Assert.AreNotEqual(existingTarget, incomingDestination);
        Assert.AreEqual("incoming", File.ReadAllText(incomingDestination));
    }

    [TestMethod]
    public void Create_shortcut_preserves_source_and_writes_lnk_targeting_source()
    {
        using var scratch = ScratchWorkspace.Create();
        var source = scratch.CreateDirectory("source");
        var target = scratch.CreateDirectory("target");
        var sourceFile = scratch.WriteFile(Path.Combine("source", "report.txt"), "source");
        var intent = new WindowsShellFileOperationIntent(
            WindowsShellFileOperationKind.CreateShortcut,
            [FileOperationTarget.FromListedItem(Item(sourceFile, "report.txt"))],
            TargetName: null,
            target,
            WindowsShellConflictChoice.None,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);

        VisualBasicShellFileOperationExecutor.Instance.Execute(intent, progress: null, CancellationToken.None);

        Assert.IsTrue(File.Exists(sourceFile));
        var shortcutPath = Directory.GetFiles(target, "*.lnk").Single();
        Assert.AreEqual(sourceFile, WindowsShortcutFile.ReadTarget(shortcutPath));
    }

    [TestMethod]
    public void Create_shortcut_uses_non_colliding_lnk_name()
    {
        using var scratch = ScratchWorkspace.Create();
        var source = scratch.CreateDirectory("source");
        var target = scratch.CreateDirectory("target");
        var sourceFile = scratch.WriteFile(Path.Combine("source", "report.txt"), "source");
        var existingShortcut = scratch.WriteFile(Path.Combine("target", "report.lnk"), "existing");
        var intent = new WindowsShellFileOperationIntent(
            WindowsShellFileOperationKind.CreateShortcut,
            [FileOperationTarget.FromListedItem(Item(sourceFile, "report.txt"))],
            TargetName: null,
            target,
            WindowsShellConflictChoice.None,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);

        VisualBasicShellFileOperationExecutor.Instance.Execute(intent, progress: null, CancellationToken.None);

        Assert.AreEqual("existing", File.ReadAllText(existingShortcut));
        var shortcuts = Directory.GetFiles(target, "*.lnk");
        Assert.AreEqual(2, shortcuts.Length);
        var createdShortcut = shortcuts.Single(path => !string.Equals(path, existingShortcut, StringComparison.OrdinalIgnoreCase));
        Assert.AreEqual(sourceFile, WindowsShortcutFile.ReadTarget(createdShortcut));
    }

    [TestMethod]
    public async Task Recycle_bin_delete_for_unc_path_returns_unavailable_without_executing_delete()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor();
        var adapter = new WindowsShellFileOperationAdapter(executor: executor);
        var request = FileOperationRequest.RecycleBinDelete([Item(@"\\server\share\delete-me.txt", "delete-me.txt")]);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.RecycleBinUnavailable, result.Status);
        Assert.AreEqual(0, executor.Intents.Count);
    }

    [TestMethod]
    public async Task Recycle_bin_probe_unsupported_result_returns_unavailable_without_executing_delete()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor();
        var adapter = new WindowsShellFileOperationAdapter(
            new StaticRecycleBinCapabilityProbe(RecycleBinCapability.NotRecyclable),
            executor);
        var request = FileOperationRequest.RecycleBinDelete([Item(@"D:\scratch\delete-me.txt", "delete-me.txt")]);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.RecycleBinUnavailable, result.Status);
        Assert.AreEqual(0, executor.Intents.Count);
    }

    [TestMethod]
    public async Task Ambiguous_recycle_bin_delete_failure_returns_failed_without_permanent_delete_fallback()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor
        {
            ExceptionToThrow = new IOException("ambiguous shell failure")
        };
        var adapter = new WindowsShellFileOperationAdapter(
            new StaticRecycleBinCapabilityProbe(RecycleBinCapability.Unknown),
            executor);
        var request = FileOperationRequest.RecycleBinDelete([Item(@"D:\scratch\delete-me.txt", "delete-me.txt")]);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.Failed, result.Status);
        Assert.AreEqual(1, executor.Intents.Count);
        Assert.AreEqual(WindowsShellDeleteDisposition.RecycleBin, executor.Intents.Single().DeleteDisposition);
        Assert.AreNotEqual(WindowsShellDeleteDisposition.Permanent, executor.Intents.Single().DeleteDisposition);
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

    private sealed class StaticRecycleBinCapabilityProbe : IRecycleBinCapabilityProbe
    {
        private readonly RecycleBinCapability _capability;

        public StaticRecycleBinCapabilityProbe(RecycleBinCapability capability)
        {
            _capability = capability;
        }

        public RecycleBinCapability GetCapability(IReadOnlyList<FileOperationTarget> targets)
        {
            return _capability;
        }
    }

    private sealed class StaticFileOperationCollisionProbe : IFileOperationCollisionProbe
    {
        private readonly bool _hasCollision;

        public StaticFileOperationCollisionProbe(bool hasCollision)
        {
            _hasCollision = hasCollision;
        }

        public FileOperationCollision? FindFirstCollision(WindowsShellFileOperationIntent intent)
        {
            return _hasCollision
                ? new FileOperationCollision(intent.Targets.Single(), "copy-me.txt")
                : null;
        }
    }

    private sealed class RecordingWindowsShellFileOperationExecutor : IWindowsShellFileOperationExecutor
    {
        public List<WindowsShellFileOperationIntent> Intents { get; } = [];

        public Exception? ExceptionToThrow { get; set; }

        public void Execute(
            WindowsShellFileOperationIntent intent,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            Intents.Add(intent);
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
        }
    }

    private sealed class ScratchWorkspace : IDisposable
    {
        private ScratchWorkspace(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static ScratchWorkspace Create()
        {
            return new ScratchWorkspace(Path.Combine(Path.GetTempPath(), "velofile-windows-operation-tests-" + Guid.NewGuid().ToString("N")));
        }

        public string CreateDirectory(string relativePath)
        {
            var path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(path);
            return path;
        }

        public string WriteFile(string relativePath, string content)
        {
            var path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
