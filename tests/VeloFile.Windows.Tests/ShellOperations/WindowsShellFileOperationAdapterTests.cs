using VeloFile.Core.Listing;
using VeloFile.Core.Operations;
using VeloFile.Windows.Shell;

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
}
