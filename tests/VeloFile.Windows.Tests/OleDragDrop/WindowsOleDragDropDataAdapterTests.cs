using VeloFile.Core.Listing;
using VeloFile.Windows.DragDrop;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.OleDragDrop;

[TestClass]
[TestCategory("DragDrop")]
public sealed class WindowsOleDragDropDataAdapterTests
{
    [TestMethod]
    public void File_drop_paths_are_projected_to_core_drop_items()
    {
        using var scratch = ScratchWorkspace.Create();
        var file = scratch.WriteFile("report.txt", "report");
        var directory = scratch.CreateDirectory("folder");
        var adapter = new WindowsOleDragDropDataAdapter();

        var result = adapter.ExtractFileDrop([file, directory]);

        Assert.IsTrue(result.CanDrop);
        Assert.AreEqual(2, result.Items.Count);
        Assert.AreEqual(file, result.Items[0].FullPath);
        Assert.AreEqual("report.txt", result.Items[0].Name);
        Assert.AreEqual(FileSystemEntryKind.File, result.Items[0].Kind);
        Assert.AreEqual(directory, result.Items[1].FullPath);
        Assert.AreEqual("folder", result.Items[1].Name);
        Assert.AreEqual(FileSystemEntryKind.Directory, result.Items[1].Kind);
    }

    [TestMethod]
    public void Empty_or_unknown_file_drop_paths_are_rejected_without_throwing()
    {
        var adapter = new WindowsOleDragDropDataAdapter();

        var empty = adapter.ExtractFileDrop([]);
        var missing = adapter.ExtractFileDrop([Path.Combine(Path.GetTempPath(), "velofile-missing-" + Guid.NewGuid().ToString("N"))]);

        Assert.IsFalse(empty.CanDrop);
        Assert.AreEqual("ole-drop-no-files", empty.ReasonCode);
        Assert.IsFalse(missing.CanDrop);
        Assert.AreEqual("drop-path-unsupported", missing.ReasonCode);
    }

    [TestMethod]
    public void Malformed_or_mixed_file_drop_paths_are_rejected_without_throwing()
    {
        using var scratch = ScratchWorkspace.Create();
        var file = scratch.WriteFile("report.txt", "report");
        var adapter = new WindowsOleDragDropDataAdapter();

        var malformed = adapter.ExtractFileDrop(["C:\\invalid\0path.txt"]);
        var mixed = adapter.ExtractFileDrop([file, "C:\\invalid\0path.txt"]);

        Assert.IsFalse(malformed.CanDrop);
        Assert.AreEqual("drop-path-invalid", malformed.ReasonCode);
        Assert.IsFalse(mixed.CanDrop);
        Assert.AreEqual("drop-path-invalid", mixed.ReasonCode);
    }

    [TestMethod]
    public void Blank_or_mixed_blank_file_drop_paths_are_rejected_without_throwing()
    {
        using var scratch = ScratchWorkspace.Create();
        var file = scratch.WriteFile("report.txt", "report");
        var adapter = new WindowsOleDragDropDataAdapter();

        var blank = adapter.ExtractFileDrop([""]);
        var whitespace = adapter.ExtractFileDrop(["   "]);
        var mixed = adapter.ExtractFileDrop([file, "   "]);

        Assert.IsFalse(blank.CanDrop);
        Assert.AreEqual("drop-path-invalid", blank.ReasonCode);
        Assert.IsFalse(whitespace.CanDrop);
        Assert.AreEqual("drop-path-invalid", whitespace.ReasonCode);
        Assert.IsFalse(mixed.CanDrop);
        Assert.AreEqual("drop-path-invalid", mixed.ReasonCode);
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
            return new ScratchWorkspace(Path.Combine(Path.GetTempPath(), "velofile-ole-dragdrop-tests-" + Guid.NewGuid().ToString("N")));
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
