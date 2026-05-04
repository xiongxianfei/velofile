using VeloFile.Core.Commands;
using VeloFile.Core.Listing;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Commands;

[TestClass]
[TestCategory("Commands")]
public sealed class BuiltInCommandRegistryTests
{
    [TestMethod]
    public void Built_in_context_menu_contains_v1_core_verbs_without_shell_extension_providers()
    {
        var registry = BuiltInCommandRegistry.CreateDefault();
        var context = CommandContext.ForSelection(
            currentFolderPath: @"D:\folder",
            selectedItems: [Item("report.txt")],
            canPaste: true);

        var menu = registry.BuildContextMenu(context);

        CollectionAssert.AreEqual(
            new[]
            {
                VeloFileCommandId.Open,
                VeloFileCommandId.OpenWith,
                VeloFileCommandId.Cut,
                VeloFileCommandId.Copy,
                VeloFileCommandId.Paste,
                VeloFileCommandId.Rename,
                VeloFileCommandId.Delete,
                VeloFileCommandId.Properties,
                VeloFileCommandId.CopyPath,
                VeloFileCommandId.CopyName,
                VeloFileCommandId.OpenTerminalHere
            },
            menu.Select(item => item.CommandId).ToArray());
        Assert.IsTrue(menu.All(item => item.Provider == CommandProviderKind.BuiltIn));
        Assert.IsFalse(registry.EnumeratesShellExtensions);
    }

    [TestMethod]
    public void Selection_and_clipboard_command_availability_is_explicit()
    {
        var registry = BuiltInCommandRegistry.CreateDefault();
        var emptyContext = CommandContext.ForSelection(@"D:\folder", [], canPaste: false);
        var selectedContext = CommandContext.ForSelection(@"D:\folder", [Item("report.txt")], canPaste: false);

        Assert.IsFalse(registry.GetCommand(VeloFileCommandId.Open).IsAvailable(emptyContext));
        Assert.IsTrue(registry.GetCommand(VeloFileCommandId.Open).IsAvailable(selectedContext));
        Assert.IsFalse(registry.GetCommand(VeloFileCommandId.CopyPath).IsAvailable(emptyContext));
        Assert.IsTrue(registry.GetCommand(VeloFileCommandId.CopyPath).IsAvailable(selectedContext));
        Assert.IsFalse(registry.GetCommand(VeloFileCommandId.Paste).IsAvailable(selectedContext));
        Assert.IsTrue(registry.GetCommand(VeloFileCommandId.OpenTerminalHere).IsAvailable(emptyContext));
    }

    private static ListedFileItem Item(string name)
    {
        return new ListedFileItem(
            FullPath: Path.Combine(@"D:\folder", name),
            Name: name,
            DisplayName: name,
            Kind: FileSystemEntryKind.File,
            Length: 1,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            Attributes: FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }
}
