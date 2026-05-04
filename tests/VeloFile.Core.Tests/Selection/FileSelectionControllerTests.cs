using VeloFile.Core.Listing;
using VeloFile.Core.Selection;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Selection;

[TestClass]
[TestCategory("Selection")]
public sealed class FileSelectionControllerTests
{
    [TestMethod]
    public void Single_ctrl_shift_select_all_and_escape_follow_explorer_selection_contract()
    {
        var selection = new FileSelectionController(Items("alpha.txt", "bravo.txt", "charlie.txt", "delta.txt"));

        selection.SelectSingle(1);

        Assert.AreEqual(1, selection.FocusedIndex);
        CollectionAssert.AreEqual(new[] { "bravo.txt" }, selection.SelectedNames.ToArray());

        selection.ToggleSelection(3);

        Assert.AreEqual(3, selection.FocusedIndex);
        CollectionAssert.AreEquivalent(new[] { "bravo.txt", "delta.txt" }, selection.SelectedNames.ToArray());

        selection.SelectRangeTo(0);

        Assert.AreEqual(0, selection.FocusedIndex);
        CollectionAssert.AreEqual(new[] { "alpha.txt", "bravo.txt", "charlie.txt", "delta.txt" }, selection.SelectedNames.ToArray());

        selection.ClearSelection();

        Assert.AreEqual(0, selection.SelectedItems.Count);
        Assert.AreEqual(0, selection.FocusedIndex);

        selection.SelectAll();

        CollectionAssert.AreEqual(new[] { "alpha.txt", "bravo.txt", "charlie.txt", "delta.txt" }, selection.SelectedNames.ToArray());
    }

    [TestMethod]
    public void Arrow_focus_movement_can_replace_select_extend_range_or_move_without_selection()
    {
        var selection = new FileSelectionController(Items("alpha.txt", "bravo.txt", "charlie.txt", "delta.txt"));
        selection.SelectSingle(1);

        selection.MoveFocus(1, extendSelection: false, preserveSelection: true);

        Assert.AreEqual(2, selection.FocusedIndex);
        CollectionAssert.AreEqual(new[] { "bravo.txt" }, selection.SelectedNames.ToArray());

        selection.MoveFocus(1, extendSelection: true, preserveSelection: false);

        Assert.AreEqual(3, selection.FocusedIndex);
        CollectionAssert.AreEqual(new[] { "bravo.txt", "charlie.txt", "delta.txt" }, selection.SelectedNames.ToArray());

        selection.MoveFocus(-3, extendSelection: false, preserveSelection: false);

        Assert.AreEqual(0, selection.FocusedIndex);
        CollectionAssert.AreEqual(new[] { "alpha.txt" }, selection.SelectedNames.ToArray());
    }

    private static IReadOnlyList<ListedFileItem> Items(params string[] names)
    {
        return names
            .Select(name => new ListedFileItem(
                FullPath: Path.Combine(@"D:\folder", name),
                Name: name,
                DisplayName: name,
                Kind: FileSystemEntryKind.File,
                Length: 1,
                LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
                Attributes: FileAttributes.Archive,
                IsHidden: false,
                IsProtectedOperatingSystemFile: false,
                IsVisuallyDimmed: false))
            .ToArray();
    }
}
