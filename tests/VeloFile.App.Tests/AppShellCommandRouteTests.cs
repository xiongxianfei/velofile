using VeloFile.App.Input;
using VeloFile.App.ViewModels;
using VeloFile.Core.Commands;
using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Operations;
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
    [TestCategory("Operations")]
    public async Task Operations_delete_command_routes_selected_items_to_recycle_bin_operation_and_visible_status()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new RecordingFileOperationAdapter();
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        Assert.AreEqual(FileOperationKind.RecycleBinDelete, operationAdapter.LastRequest?.Kind);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        StringAssert.Contains(viewModel.FileOperationStatusText, "completed");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_copy_then_paste_routes_copy_to_active_folder_without_undo()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter();
        var copyMe = Item(@"D:\source\copy-me.txt", "copy-me.txt");
        var copied = Item(@"D:\target\copy-me.txt", "copy-me.txt");
        source.SetEntries(@"D:\source", copyMe);
        source.SetEntries(@"D:\target");
        var viewModel = CreateViewModel(
            clipboard,
            initialPath: @"D:\source",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\source", @"D:\target"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1);

        viewModel.SetSelectedFileItems([copyMe]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Copy);
        Assert.IsTrue(viewModel.CanPasteFileOperation);

        var navigation = viewModel.SubmitPath(@"D:\target");
        Assert.IsTrue(navigation.Accepted);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 0);
        source.SetEntries(@"D:\target", copied);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);

        Assert.AreEqual(FileOperationKind.Copy, operationAdapter.LastRequest?.Kind);
        Assert.AreEqual(@"D:\target", operationAdapter.LastRequest?.TargetDirectory);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        Assert.IsFalse(viewModel.FileOperation.UndoEligibility.CanUndo);
        CollectionAssert.AreEqual(new[] { "copy-me.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_cut_then_paste_routes_move_to_active_folder_and_records_undo()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter();
        var moveMe = Item(@"D:\source\move-me.txt", "move-me.txt");
        var moved = Item(@"D:\target\move-me.txt", "move-me.txt");
        source.SetEntries(@"D:\source", moveMe);
        source.SetEntries(@"D:\target");
        var viewModel = CreateViewModel(
            clipboard,
            initialPath: @"D:\source",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\source", @"D:\target"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1);

        viewModel.SetSelectedFileItems([moveMe]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Cut);
        var navigation = viewModel.SubmitPath(@"D:\target");
        Assert.IsTrue(navigation.Accepted);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 0);
        source.SetEntries(@"D:\target", moved);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);

        Assert.AreEqual(FileOperationKind.Move, operationAdapter.LastRequest?.Kind);
        Assert.AreEqual(@"D:\target", operationAdapter.LastRequest?.TargetDirectory);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        Assert.IsTrue(viewModel.FileOperation.UndoEligibility.CanUndo);
        Assert.AreEqual(FileOperationKind.Move, viewModel.FileOperation.UndoEligibility.OperationKind);
        StringAssert.Contains(viewModel.FileOperationStatusText, "Undo available");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_copy_conflict_surfaces_resolution_and_replace_resumes_operation()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var conflictItem = Item(@"D:\source\copy-me.txt", "copy-me.txt");
        var copied = Item(@"D:\target\copy-me.txt", "copy-me.txt");
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(conflictItem)],
                    @"D:\target",
                    "copy-me.txt"))
        };
        source.SetEntries(@"D:\source", conflictItem);
        source.SetEntries(@"D:\target", copied);
        var viewModel = CreateViewModel(
            clipboard,
            initialPath: @"D:\source",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\source", @"D:\target"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1);

        viewModel.SetSelectedFileItems([conflictItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Copy);
        Assert.IsTrue(viewModel.SubmitPath(@"D:\target").Accepted);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);

        Assert.AreEqual(FileOperationStatus.WaitingForConflict, viewModel.FileOperation.Status);
        Assert.AreEqual("copy-me.txt", viewModel.PendingFileOperationConflict?.ExistingName);
        Assert.IsFalse(viewModel.FileOperation.UndoEligibility.CanUndo);
        Assert.AreEqual(1, operationAdapter.Requests.Count);

        operationAdapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await viewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Replace);

        Assert.AreEqual(2, operationAdapter.Requests.Count);
        Assert.AreEqual(FileOperationConflictChoice.Replace, operationAdapter.LastRequest?.ConflictChoice);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        Assert.IsNull(viewModel.PendingFileOperationConflict);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_copy_conflict_skip_preserves_existing_target_and_copies_unaffected_item()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var conflictItem = Item(@"D:\source\report.txt", "report.txt");
        var unaffectedItem = Item(@"D:\source\fresh.txt", "fresh.txt");
        var existingTarget = Item(@"D:\target\report.txt", "report.txt");
        var freshTarget = Item(@"D:\target\fresh.txt", "fresh.txt");
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(conflictItem), FileOperationTarget.FromListedItem(unaffectedItem)],
                    @"D:\target",
                    "report.txt"))
        };
        source.SetEntries(@"D:\source", conflictItem, unaffectedItem);
        source.SetEntries(@"D:\target", existingTarget);
        var viewModel = CreateViewModel(
            clipboard,
            initialPath: @"D:\source",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\source", @"D:\target"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);

        viewModel.SetSelectedFileItems([conflictItem, unaffectedItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Copy);
        Assert.IsTrue(viewModel.SubmitPath(@"D:\target").Accepted);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);

        Assert.AreEqual(FileOperationStatus.WaitingForConflict, viewModel.FileOperation.Status);

        source.SetEntries(@"D:\target", existingTarget, freshTarget);
        operationAdapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await viewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Skip);

        Assert.AreEqual(FileOperationConflictChoice.Skip, operationAdapter.LastRequest?.ConflictChoice);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        CollectionAssert.AreEqual(
            new[] { "report.txt", "fresh.txt" },
            viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(1, viewModel.FileItems.Count(item => item.Name == "report.txt"));
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_copy_conflict_keep_both_creates_distinct_destination_and_refreshes_visible_rows()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var conflictItem = Item(@"D:\source\report.txt", "report.txt");
        var existingTarget = Item(@"D:\target\report.txt", "report.txt");
        var keptTarget = Item(@"D:\target\report (2).txt", "report (2).txt");
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(conflictItem)],
                    @"D:\target",
                    "report.txt"))
        };
        source.SetEntries(@"D:\source", conflictItem);
        source.SetEntries(@"D:\target", existingTarget);
        var viewModel = CreateViewModel(
            clipboard,
            initialPath: @"D:\source",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\source", @"D:\target"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1);

        viewModel.SetSelectedFileItems([conflictItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Copy);
        Assert.IsTrue(viewModel.SubmitPath(@"D:\target").Accepted);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);

        Assert.AreEqual(FileOperationStatus.WaitingForConflict, viewModel.FileOperation.Status);

        source.SetEntries(@"D:\target", existingTarget, keptTarget);
        operationAdapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await viewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.KeepBoth);

        Assert.AreEqual(FileOperationConflictChoice.KeepBoth, operationAdapter.LastRequest?.ConflictChoice);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        CollectionAssert.AreEqual(
            new[] { "report.txt", "report (2).txt" },
            viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreNotEqual(existingTarget.FullPath, keptTarget.FullPath);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_conflict_resolution_refreshes_original_target_when_user_navigates_elsewhere()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var conflictItem = Item(@"D:\source\copy-me.txt", "copy-me.txt");
        var copied = Item(@"D:\target\copy-me.txt", "copy-me.txt");
        var other = Item(@"D:\other\other.txt", "other.txt");
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(conflictItem)],
                    @"D:\target",
                    "copy-me.txt"))
        };
        source.SetEntries(@"D:\source", conflictItem);
        source.SetEntries(@"D:\target");
        source.SetEntries(@"D:\other", other);
        var viewModel = CreateViewModel(
            clipboard,
            initialPath: @"D:\source",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\source", @"D:\target", @"D:\other"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1);

        viewModel.SetSelectedFileItems([conflictItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Copy);
        Assert.IsTrue(viewModel.SubmitPath(@"D:\target").Accepted);
        await WaitUntilAsync(() => source.EnumerationCount(@"D:\target") == 1);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);
        Assert.AreEqual(FileOperationStatus.WaitingForConflict, viewModel.FileOperation.Status);

        Assert.IsTrue(viewModel.SubmitPath(@"D:\other").Accepted);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].FullPath == other.FullPath);
        source.SetEntries(@"D:\target", copied);
        operationAdapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await viewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Replace);

        Assert.AreEqual(2, source.EnumerationCount(@"D:\target"));
        Assert.AreEqual(1, source.EnumerationCount(@"D:\other"));
        Assert.AreEqual(@"D:\other", viewModel.ActivePath);
        CollectionAssert.AreEqual(new[] { "other.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_permanent_delete_command_requires_confirmation_before_adapter_call()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new RecordingFileOperationAdapter();
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.PermanentDelete);

        Assert.AreEqual(0, operationAdapter.Requests.Count);
        Assert.IsNotNull(viewModel.PendingPermanentDeleteConfirmation);
        Assert.AreEqual(FileOperationStatus.WaitingForConfirmation, viewModel.FileOperation.Status);

        await viewModel.ConfirmPermanentDeleteAsync(confirm: true);

        Assert.AreEqual(FileOperationKind.PermanentDelete, operationAdapter.LastRequest?.Kind);
        Assert.IsTrue(operationAdapter.LastRequest!.ConfirmedPermanentDelete);
        Assert.IsFalse(viewModel.FileOperation.UndoEligibility.CanUndo);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_recycle_bin_unavailable_delete_surfaces_confirmation_reason()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.RecycleBinUnavailable("recycle-bin-unavailable")
        };
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"\\server\share\delete-me.txt", "delete-me.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        Assert.IsNotNull(viewModel.PendingPermanentDeleteConfirmation);
        Assert.AreEqual(PermanentDeleteReason.RecycleBinUnavailable, viewModel.PendingPermanentDeleteConfirmation.Reason);
        StringAssert.Contains(viewModel.FileOperationStatusText, "recycle-bin-unavailable");
        Assert.AreEqual(1, operationAdapter.Requests.Count);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_rename_commit_routes_pending_rename_to_operation_adapter()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new RecordingFileOperationAdapter();
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\old.txt", "old.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        Assert.IsTrue(viewModel.IsRenameActive);
        Assert.AreEqual(item, viewModel.PendingRenameItem);
        Assert.AreEqual("old.txt", viewModel.PendingRenameText);

        viewModel.SetPendingRenameText("new.txt");
        await viewModel.CommitPendingRenameAsync();

        Assert.AreEqual(FileOperationKind.Rename, operationAdapter.LastRequest?.Kind);
        Assert.AreEqual("new.txt", operationAdapter.LastRequest?.TargetName);
        Assert.IsTrue(viewModel.FileOperation.UndoEligibility.CanUndo);
        Assert.IsFalse(viewModel.IsRenameActive);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_rename_success_refreshes_visible_file_rows()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter();
        var oldItem = Item(@"D:\projects\old.txt", "old.txt");
        var renamedItem = Item(@"D:\projects\new.txt", "new.txt");
        source.SetEntries(@"D:\projects", oldItem);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "old.txt");

        source.SetEntries(@"D:\projects", renamedItem);
        viewModel.SetSelectedFileItems([oldItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        viewModel.SetPendingRenameText("new.txt");
        await viewModel.CommitPendingRenameAsync();

        CollectionAssert.AreEqual(new[] { "new.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
        CollectionAssert.DoesNotContain(viewModel.FileItems.Select(item => item.Name).ToArray(), "old.txt");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_recycle_bin_delete_success_refreshes_visible_file_rows()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter();
        var deleteMe = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        var keep = Item(@"D:\projects\keep.txt", "keep.txt");
        source.SetEntries(@"D:\projects", deleteMe, keep);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);

        source.SetEntries(@"D:\projects", keep);
        viewModel.SetSelectedFileItems([deleteMe]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        CollectionAssert.AreEqual(new[] { "keep.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_confirmed_permanent_delete_success_refreshes_visible_file_rows_after_confirmation_only()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.RecycleBinUnavailable("recycle-bin-unavailable")
        };
        var deleteMe = Item(@"\\server\share\delete-me.txt", "delete-me.txt");
        var keep = Item(@"\\server\share\keep.txt", "keep.txt");
        source.SetEntries(@"D:\projects", deleteMe, keep);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);

        viewModel.SetSelectedFileItems([deleteMe]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        Assert.AreEqual(FileOperationStatus.WaitingForConfirmation, viewModel.FileOperation.Status);
        CollectionAssert.AreEqual(
            new[] { "delete-me.txt", "keep.txt" },
            viewModel.FileItems.Select(item => item.Name).ToArray());

        source.SetEntries(@"D:\projects", keep);
        operationAdapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: true);
        await viewModel.ConfirmPermanentDeleteAsync(confirm: true);

        CollectionAssert.AreEqual(new[] { "keep.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_failed_rename_preserves_visible_file_rows()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.Failed("access-denied")
        };
        var oldItem = Item(@"D:\projects\old.txt", "old.txt");
        var renamedItem = Item(@"D:\projects\new.txt", "new.txt");
        source.SetEntries(@"D:\projects", oldItem);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "old.txt");

        source.SetEntries(@"D:\projects", renamedItem);
        viewModel.SetSelectedFileItems([oldItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        viewModel.SetPendingRenameText("new.txt");
        await viewModel.CommitPendingRenameAsync();

        CollectionAssert.AreEqual(new[] { "old.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(FileOperationStatus.Failed, viewModel.FileOperation.Status);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_cancelled_delete_preserves_visible_file_rows()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new CancellablePendingFileOperationAdapter(supportsCancellation: true);
        var deleteMe = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        var keep = Item(@"D:\projects\keep.txt", "keep.txt");
        source.SetEntries(@"D:\projects", deleteMe, keep);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);

        source.SetEntries(@"D:\projects", keep);
        viewModel.SetSelectedFileItems([deleteMe]);
        var delete = viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);
        viewModel.CancelFileOperation();
        await delete;

        CollectionAssert.AreEqual(
            new[] { "delete-me.txt", "keep.txt" },
            viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(FileOperationStatus.Cancelled, viewModel.FileOperation.Status);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_delete_failure_preserves_visible_rows_and_shows_recoverable_status()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.Failed("access-denied")
        };
        var deleteMe = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        var keep = Item(@"D:\projects\keep.txt", "keep.txt");
        source.SetEntries(@"D:\projects", keep, deleteMe);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);
        var enumerationCountAfterInitialLoad = source.EnumerationCount(@"D:\projects");

        source.SetEntries(@"D:\projects", keep);
        viewModel.SetSelectedFileItems([deleteMe]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        CollectionAssert.AreEqual(
            new[] { "keep.txt", "delete-me.txt" },
            viewModel.FileItems.Select(item => item.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "delete-me.txt" }, viewModel.SelectedFileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(enumerationCountAfterInitialLoad, source.EnumerationCount(@"D:\projects"));
        Assert.AreEqual(FileOperationStatus.Failed, viewModel.FileOperation.Status);
        StringAssert.Contains(viewModel.FileOperationStatusText, "Recycle Bin delete failed");
        Assert.IsNull(viewModel.PendingPermanentDeleteConfirmation);
        Assert.AreEqual(@"D:\projects", viewModel.ActivePath);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_completed_mutation_with_failed_refresh_preserves_rows_and_shows_refresh_warning()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var operationAdapter = new RecordingFileOperationAdapter();
        var deleteMe = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        var keep = Item(@"D:\projects\keep.txt", "keep.txt");
        source.SetEntries(@"D:\projects", keep, deleteMe);
        var viewModel = CreateViewModel(clipboard, listingSource: source, operationAdapter: operationAdapter);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 2);
        var enumerationCountAfterInitialLoad = source.EnumerationCount(@"D:\projects");

        source.SetException(@"D:\projects", new UnauthorizedAccessException("denied"));
        viewModel.SetSelectedFileItems([deleteMe]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
        CollectionAssert.AreEqual(
            new[] { "keep.txt", "delete-me.txt" },
            viewModel.FileItems.Select(item => item.Name).ToArray());
        Assert.AreEqual(enumerationCountAfterInitialLoad + 1, source.EnumerationCount(@"D:\projects"));
        StringAssert.Contains(viewModel.FileOperationStatusText, "Recycle Bin delete completed");
        StringAssert.Contains(viewModel.FileOperationStatusText, "Could not refresh the folder");
        Assert.AreEqual(@"D:\projects", viewModel.ActivePath);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_late_post_mutation_refresh_cannot_overwrite_newer_navigation()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new DelayedSecondListingFolderEntrySource(@"D:\projects");
        var operationAdapter = new RecordingFileOperationAdapter();
        var oldItem = Item(@"D:\projects\old.txt", "old.txt");
        var renamedItem = Item(@"D:\projects\new.txt", "new.txt");
        var otherItem = Item(@"D:\other\other.txt", "other.txt");
        source.SetEntries(@"D:\projects", oldItem);
        source.SetEntries(@"D:\other", otherItem);
        var viewModel = CreateViewModel(
            clipboard,
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\projects", @"D:\other"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "old.txt");

        source.SetEntries(@"D:\projects", renamedItem);
        viewModel.SetSelectedFileItems([oldItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        viewModel.SetPendingRenameText("new.txt");
        var rename = viewModel.CommitPendingRenameAsync();
        await WaitUntilAsync(() => viewModel.FileOperation.Status is FileOperationStatus.Completed);

        var navigation = viewModel.SubmitPath(@"D:\other");
        Assert.IsTrue(navigation.Accepted);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "other.txt");

        source.Release();
        await rename;

        Assert.AreEqual(@"D:\other", viewModel.ActivePath);
        CollectionAssert.AreEqual(new[] { "other.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_late_post_mutation_refresh_cannot_overwrite_active_tab_after_tab_switch()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new DelayedSecondListingFolderEntrySource(@"D:\projects");
        var operationAdapter = new RecordingFileOperationAdapter();
        var oldItem = Item(@"D:\projects\a-old.txt", "a-old.txt");
        var renamedItem = Item(@"D:\projects\a-new.txt", "a-new.txt");
        var otherItem = Item(@"D:\other\b.txt", "b.txt");
        source.SetEntries(@"D:\projects", oldItem);
        source.SetEntries(@"D:\other", otherItem);
        var viewModel = CreateViewModel(
            clipboard,
            defaultPath: @"D:\other",
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\projects", @"D:\other"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "a-old.txt");

        viewModel.NewTab();
        await WaitUntilAsync(() => viewModel.ActiveTabIndex == 1 && viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "b.txt");
        viewModel.SwitchToTab(0);
        await WaitUntilAsync(() => viewModel.ActiveTabIndex == 0 && viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "a-old.txt");

        source.SetEntries(@"D:\projects", renamedItem);
        viewModel.SetSelectedFileItems([oldItem]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        viewModel.SetPendingRenameText("a-new.txt");
        var rename = viewModel.CommitPendingRenameAsync();
        await WaitUntilAsync(() => viewModel.FileOperation.Status is FileOperationStatus.Completed);

        viewModel.SwitchToTab(1);
        await WaitUntilAsync(() => viewModel.ActiveTabIndex == 1 && viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "b.txt");

        source.Release();
        await rename;

        Assert.AreEqual(1, viewModel.ActiveTabIndex);
        Assert.AreEqual(@"D:\other", viewModel.ActivePath);
        CollectionAssert.AreEqual(new[] { "b.txt" }, viewModel.FileItems.Select(item => item.Name).ToArray());

        viewModel.SwitchToTab(0);
        await WaitUntilAsync(() => viewModel.ActiveTabIndex == 0 && viewModel.FileItems.Count == 1 && viewModel.FileItems[0].Name == "a-new.txt");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_rename_cancel_clears_pending_rename_without_adapter_call()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new RecordingFileOperationAdapter();
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\old.txt", "old.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        viewModel.SetPendingRenameText("new.txt");
        viewModel.CancelPendingRename();

        Assert.IsFalse(viewModel.IsRenameActive);
        Assert.AreEqual(0, operationAdapter.Requests.Count);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_invalid_rename_stays_recoverable_without_adapter_call()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new RecordingFileOperationAdapter();
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\old.txt", "old.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Rename);
        viewModel.SetPendingRenameText(@"bad\name.txt");
        await viewModel.CommitPendingRenameAsync();

        Assert.IsTrue(viewModel.IsRenameActive);
        StringAssert.Contains(viewModel.RenameError!, "invalid");
        Assert.AreEqual(0, operationAdapter.Requests.Count);
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_delete_exposes_cancel_command_and_routes_cancellation_to_adapter()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new CancellablePendingFileOperationAdapter(supportsCancellation: true);
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        var operation = viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        Assert.AreEqual(FileOperationStatus.Running, viewModel.FileOperation.Status);
        Assert.IsTrue(viewModel.CanCancelFileOperation);

        viewModel.CancelFileOperation();
        await operation;

        Assert.IsTrue(operationAdapter.CancellationObserved);
        Assert.AreEqual(FileOperationStatus.Cancelled, viewModel.FileOperation.Status);
        Assert.IsFalse(viewModel.CanCancelFileOperation);
        StringAssert.Contains(viewModel.FileOperationStatusText, "cancelled");
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_move_cancel_after_partial_progress_keeps_visible_cancel_status_and_hides_undo()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var source = new FakeFolderEntrySource();
        var first = Item(@"D:\projects\one.txt", "one.txt");
        var second = Item(@"D:\projects\two.txt", "two.txt");
        var third = Item(@"D:\projects\three.txt", "three.txt");
        source.SetEntries(@"D:\projects", first, second, third);
        source.SetEntries(@"D:\target");
        var operationAdapter = new CancellablePendingFileOperationAdapter(supportsCancellation: true)
        {
            ProgressToReport = new FileOperationProgress(
                FileOperationKind.Move,
                CompletedItemCount: 1,
                TotalItemCount: 3,
                StatusText: "Moved 1 of 3")
        };
        var viewModel = CreateViewModel(
            clipboard,
            listingSource: source,
            operationAdapter: operationAdapter,
            existingPaths: [@"D:\projects", @"D:\target"]);
        await WaitUntilAsync(() => viewModel.FileItems.Count == 3);

        viewModel.SetSelectedFileItems([first, second, third]);
        await viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Cut);
        Assert.IsTrue(viewModel.SubmitPath(@"D:\target").Accepted);
        await WaitUntilAsync(() => viewModel.ActivePath == @"D:\target" && source.EnumerationCount(@"D:\target") == 1);
        var operation = viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Paste);

        Assert.AreEqual(FileOperationStatus.Running, viewModel.FileOperation.Status);
        Assert.IsTrue(viewModel.CanCancelFileOperation);
        Assert.AreEqual(1, viewModel.FileOperation.Progress.CompletedItemCount);
        Assert.AreEqual(3, viewModel.FileOperation.Progress.TotalItemCount);
        StringAssert.Contains(viewModel.FileOperationStatusText, "1 of 3");

        viewModel.CancelFileOperation();
        await operation;

        Assert.IsTrue(operationAdapter.CancellationObserved);
        Assert.AreEqual(FileOperationStatus.Cancelled, viewModel.FileOperation.Status);
        Assert.AreEqual(1, viewModel.FileOperation.Progress.CompletedItemCount);
        Assert.AreEqual(3, viewModel.FileOperation.Progress.TotalItemCount);
        Assert.IsFalse(viewModel.FileOperation.UndoEligibility.CanUndo);
        Assert.IsFalse(viewModel.FileOperationStatusText.Contains("Undo available", StringComparison.Ordinal));
        StringAssert.Contains(viewModel.FileOperationStatusText, "cancelled");
        StringAssert.Contains(viewModel.FileOperationStatusText, "1 of 3");
        Assert.AreEqual(1, source.EnumerationCount(@"D:\target"));
    }

    [TestMethod]
    [TestCategory("Operations")]
    public async Task Operations_non_cancellable_operation_hides_cancel_command()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var operationAdapter = new CancellablePendingFileOperationAdapter(supportsCancellation: false);
        var viewModel = CreateViewModel(clipboard, operationAdapter: operationAdapter);
        var item = Item(@"D:\projects\delete-me.txt", "delete-me.txt");
        viewModel.SetFileItems([item]);
        viewModel.SetSelectedFileItems([item]);

        var operation = viewModel.ExecuteBuiltInCommandAsync(VeloFileCommandId.Delete);

        Assert.AreEqual(FileOperationStatus.Running, viewModel.FileOperation.Status);
        Assert.IsFalse(viewModel.CanCancelFileOperation);

        viewModel.CancelFileOperation();
        operationAdapter.Complete(FileOperationAdapterResult.Completed(undoSupported: true));
        await operation;

        Assert.IsFalse(operationAdapter.CancellationObserved);
        Assert.AreEqual(FileOperationStatus.Completed, viewModel.FileOperation.Status);
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
    public async Task Recursive_search_normal_start_uses_v1_default_ten_thousand_result_cap()
    {
        var clipboard = new CollectingClipboardTextWriter();
        var listingSource = new FakeFolderEntrySource();
        listingSource.SetEntries(@"D:\projects", Item(@"D:\projects\folder-row.txt", "folder-row.txt"));
        var searchService = new ScriptedRecursiveSearchService();
        var viewModel = CreateViewModel(clipboard, listingSource: listingSource, searchService: searchService);
        await WaitUntilAsync(() => viewModel.VisibleItems.Count == 1);

        viewModel.StartRecursiveSearch("default-cap");

        await WaitUntilAsync(() => searchService.LastOptions is not null);
        Assert.AreEqual("default-cap", searchService.LastQuery);
        Assert.AreEqual(10_000, searchService.LastOptions!.ResultLimit);
        Assert.AreEqual(RecursiveSearchStatus.Running, viewModel.RecursiveSearch.Status);
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
        IFileOperationAdapter? operationAdapter = null,
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

        var operationService = operationAdapter is null ? null : new FileOperationService(operationAdapter);
        return new AppShellViewModel(startupState, clipboardWriter, coordinator, searchService, operationService, viewportItemCount: 100);
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
        private readonly Dictionary<string, int> _enumerationCounts = new(StringComparer.OrdinalIgnoreCase);

        public int SearchEnumerationCount { get; private set; }

        public int EnumerationCount(string path)
        {
            return _enumerationCounts.TryGetValue(path, out var count) ? count : 0;
        }

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
            _enumerationCounts[path] = EnumerationCount(path) + 1;
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

        public string? LastQuery { get; private set; }

        public RecursiveSearchOptions? LastOptions { get; private set; }

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
            LastQuery = query;
            LastOptions = options;
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

    private sealed class RecordingFileOperationAdapter : IFileOperationAdapter
    {
        public List<FileOperationRequest> Requests { get; } = [];

        public FileOperationRequest? LastRequest => Requests.LastOrDefault();

        public FileOperationAdapterResult NextResult { get; set; } = FileOperationAdapterResult.Completed(undoSupported: true);

        public Task<FileOperationAdapterResult> ExecuteAsync(
            FileOperationRequest request,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            progress?.Report(new FileOperationProgress(request.Kind, request.Items.Count, request.Items.Count, "Completed"));
            return Task.FromResult(NextResult.Status is FileOperationAdapterResultStatus.Completed
                ? FileOperationAdapterResult.Completed(undoSupported: request.Kind is FileOperationKind.Rename or FileOperationKind.Move or FileOperationKind.RecycleBinDelete)
                : NextResult);
        }
    }

    private sealed class CancellablePendingFileOperationAdapter : IFileOperationAdapter, ICancellableFileOperationAdapter
    {
        private readonly TaskCompletionSource<FileOperationAdapterResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly bool _supportsCancellation;

        public CancellablePendingFileOperationAdapter(bool supportsCancellation)
        {
            _supportsCancellation = supportsCancellation;
        }

        public bool CancellationObserved { get; private set; }

        public FileOperationRequest? LastRequest { get; private set; }

        public FileOperationProgress? ProgressToReport { get; set; }

        public bool CanCancel(FileOperationRequest request)
        {
            return _supportsCancellation;
        }

        public Task<FileOperationAdapterResult> ExecuteAsync(
            FileOperationRequest request,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (ProgressToReport is not null)
            {
                progress?.Report(ProgressToReport);
            }

            cancellationToken.Register(() =>
            {
                CancellationObserved = true;
                _completion.TrySetResult(FileOperationAdapterResult.Cancelled());
            });
            return _completion.Task;
        }

        public void Complete(FileOperationAdapterResult result)
        {
            _completion.TrySetResult(result);
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

    private sealed class DelayedSecondListingFolderEntrySource : FakeFolderEntrySource
    {
        private readonly string _delayedPath;
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _enumerationCount;

        public DelayedSecondListingFolderEntrySource(string delayedPath)
        {
            _delayedPath = delayedPath;
        }

        public void Release()
        {
            _gate.TrySetResult();
        }

        public override async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var enumeration = string.Equals(path, _delayedPath, StringComparison.OrdinalIgnoreCase)
                ? Interlocked.Increment(ref _enumerationCount)
                : 0;

            if (enumeration > 1)
            {
                await _gate.Task.ConfigureAwait(false);
            }

            await foreach (var entry in base.EnumerateAsync(path, cancellationToken))
            {
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
