using VeloFile.Core.Commands;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Commands;

[TestClass]
[TestCategory("Commands")]
public sealed class KeyboardCommandRouterTests
{
    [TestMethod]
    public void Keyboard_shortcuts_route_to_v1_commands_and_selection_actions()
    {
        var router = KeyboardCommandRouter.CreateDefault();

        Assert.AreEqual(VeloFileCommandId.Open, router.Route(KeyGesture.Enter(), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.Rename, router.Route(KeyGesture.F2(), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.Delete, router.Route(KeyGesture.Delete(), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.PermanentDelete, router.Route(KeyGesture.Delete(shift: true), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.Refresh, router.Route(KeyGesture.F5(), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.ParentFolder, router.Route(KeyGesture.Backspace(), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.CopyPath, router.Route(KeyGesture.CtrlShift("C"), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(VeloFileCommandId.CopyName, router.Route(KeyGesture.CtrlShift("N"), KeyboardCommandContext.FileList).CommandId);
        Assert.AreEqual(SelectionKeyboardAction.SelectAll, router.Route(KeyGesture.Ctrl("A"), KeyboardCommandContext.FileList).SelectionAction);
        Assert.AreEqual(SelectionKeyboardAction.ClearSelection, router.Route(KeyGesture.Escape(), KeyboardCommandContext.FileList).SelectionAction);
    }

    [TestMethod]
    public void File_commands_are_suppressed_when_text_input_has_focus()
    {
        var router = KeyboardCommandRouter.CreateDefault();

        var delete = router.Route(KeyGesture.Delete(), KeyboardCommandContext.TextInput);
        var copyPath = router.Route(KeyGesture.CtrlShift("C"), KeyboardCommandContext.TextInput);

        Assert.AreEqual(KeyboardRouteStatus.SuppressedByTextInputFocus, delete.Status);
        Assert.IsNull(delete.CommandId);
        Assert.AreEqual(KeyboardRouteStatus.SuppressedByTextInputFocus, copyPath.Status);
        Assert.IsNull(copyPath.CommandId);
    }
}
