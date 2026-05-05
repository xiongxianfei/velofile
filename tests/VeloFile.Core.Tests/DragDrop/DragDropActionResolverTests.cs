using VeloFile.Core.DragDrop;
using VeloFile.Core.Listing;

namespace VeloFile.Core.Tests.DragDrop;

[TestClass]
[TestCategory("DragDrop")]
public sealed class DragDropActionResolverTests
{
    [TestMethod]
    public void No_modifier_resolves_move_for_same_volume_and_copy_for_cross_volume()
    {
        var resolver = new DragDropActionResolver();
        var item = Item(@"D:\source\report.txt");

        var sameVolume = resolver.Resolve(new DragDropRequest(
            [item],
            @"D:\target",
            DropVolumeRelationship.SameVolume,
            DragDropKeyModifiers.None));
        var crossVolume = resolver.Resolve(new DragDropRequest(
            [item],
            @"E:\target",
            DropVolumeRelationship.CrossVolume,
            DragDropKeyModifiers.None));

        Assert.AreEqual(DropAction.Move, sameVolume.Action);
        Assert.AreEqual("Move to D:\\target", sameVolume.IndicatorText);
        Assert.IsTrue(sameVolume.CanDrop);
        Assert.AreEqual(DropAction.Copy, crossVolume.Action);
        Assert.AreEqual("Copy to E:\\target", crossVolume.IndicatorText);
        Assert.IsTrue(crossVolume.CanDrop);
    }

    [TestMethod]
    public void Modifiers_resolve_copy_move_and_shortcut_actions()
    {
        var resolver = new DragDropActionResolver();
        var item = Item(@"D:\source\report.txt");

        var copy = resolver.Resolve(Request(item, DragDropKeyModifiers.Control));
        var move = resolver.Resolve(Request(item, DragDropKeyModifiers.Shift));
        var shortcut = resolver.Resolve(Request(item, DragDropKeyModifiers.Control | DragDropKeyModifiers.Shift));

        Assert.AreEqual(DropAction.Copy, copy.Action);
        Assert.AreEqual("Copy to D:\\target", copy.IndicatorText);
        Assert.AreEqual(DropAction.Move, move.Action);
        Assert.AreEqual("Move to D:\\target", move.IndicatorText);
        Assert.AreEqual(DropAction.Shortcut, shortcut.Action);
        Assert.AreEqual("Create shortcut in D:\\target", shortcut.IndicatorText);
    }

    [TestMethod]
    public void Empty_items_or_missing_target_reject_drop_with_visible_reason()
    {
        var resolver = new DragDropActionResolver();

        var noItems = resolver.Resolve(new DragDropRequest(
            [],
            @"D:\target",
            DropVolumeRelationship.SameVolume,
            DragDropKeyModifiers.None));
        var noTarget = resolver.Resolve(new DragDropRequest(
            [Item(@"D:\source\report.txt")],
            "",
            DropVolumeRelationship.SameVolume,
            DragDropKeyModifiers.None));

        Assert.IsFalse(noItems.CanDrop);
        Assert.AreEqual(DropAction.None, noItems.Action);
        Assert.AreEqual("drop-no-items", noItems.ReasonCode);
        Assert.IsFalse(noTarget.CanDrop);
        Assert.AreEqual(DropAction.None, noTarget.Action);
        Assert.AreEqual("drop-no-target", noTarget.ReasonCode);
    }

    [TestMethod]
    public void Shortcut_modifier_is_rejected_when_payload_cannot_create_shortcuts()
    {
        var resolver = new DragDropActionResolver();
        var item = Item(@"D:\source\report.txt");

        var resolution = resolver.Resolve(new DragDropRequest(
            [item],
            @"D:\target",
            DropVolumeRelationship.SameVolume,
            DragDropKeyModifiers.Control | DragDropKeyModifiers.Shift,
            SupportsShortcut: false));

        Assert.IsFalse(resolution.CanDrop);
        Assert.AreEqual(DropAction.None, resolution.Action);
        Assert.AreEqual("drop-shortcut-unsupported", resolution.ReasonCode);
    }

    [TestMethod]
    public void Volume_relationship_classifier_uses_source_and_target_roots()
    {
        var sameVolume = DropVolumeRelationshipClassifier.Classify(
            [Item(@"D:\source\report.txt")],
            @"D:\target");
        var crossVolume = DropVolumeRelationshipClassifier.Classify(
            [Item(@"E:\source\report.txt")],
            @"D:\target");

        Assert.AreEqual(DropVolumeRelationship.SameVolume, sameVolume);
        Assert.AreEqual(DropVolumeRelationship.CrossVolume, crossVolume);
    }

    private static DragDropRequest Request(DropItem item, DragDropKeyModifiers modifiers)
    {
        return new DragDropRequest(
            [item],
            @"D:\target",
            DropVolumeRelationship.SameVolume,
            modifiers);
    }

    private static DropItem Item(string path)
    {
        return new DropItem(path, Path.GetFileName(path), FileSystemEntryKind.File);
    }
}
