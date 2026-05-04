namespace VeloFile.Core.Commands;

public enum KeyboardRouteStatus
{
    NotHandled,
    Routed,
    SuppressedByTextInputFocus
}

public enum SelectionKeyboardAction
{
    None,
    SelectAll,
    ClearSelection
}

public enum KeyboardCommandContext
{
    FileList,
    TextInput
}

public sealed record KeyGesture(
    string Key,
    bool Control = false,
    bool Shift = false,
    bool Alt = false)
{
    public static KeyGesture Enter() => new("Enter");

    public static KeyGesture F2() => new("F2");

    public static KeyGesture F5() => new("F5");

    public static KeyGesture Delete(bool shift = false) => new("Delete", Shift: shift);

    public static KeyGesture Backspace() => new("Backspace");

    public static KeyGesture Escape() => new("Escape");

    public static KeyGesture Ctrl(string key) => new(key, Control: true);

    public static KeyGesture CtrlShift(string key) => new(key, Control: true, Shift: true);
}

public sealed record KeyboardRouteResult(
    KeyboardRouteStatus Status,
    VeloFileCommandId? CommandId,
    SelectionKeyboardAction SelectionAction)
{
    public static KeyboardRouteResult NotHandled { get; } = new(
        KeyboardRouteStatus.NotHandled,
        CommandId: null,
        SelectionKeyboardAction.None);

    public static KeyboardRouteResult Suppressed { get; } = new(
        KeyboardRouteStatus.SuppressedByTextInputFocus,
        CommandId: null,
        SelectionKeyboardAction.None);

    public static KeyboardRouteResult Command(VeloFileCommandId commandId)
    {
        return new KeyboardRouteResult(KeyboardRouteStatus.Routed, commandId, SelectionKeyboardAction.None);
    }

    public static KeyboardRouteResult Selection(SelectionKeyboardAction action)
    {
        return new KeyboardRouteResult(KeyboardRouteStatus.Routed, CommandId: null, action);
    }
}

public sealed class KeyboardCommandRouter
{
    private static readonly HashSet<string> TextInputSuppressedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Backspace",
        "Delete",
        "Enter",
        "F2",
        "C",
        "N",
        "A",
        "Escape"
    };

    public static KeyboardCommandRouter CreateDefault()
    {
        return new KeyboardCommandRouter();
    }

    public KeyboardRouteResult Route(KeyGesture gesture, KeyboardCommandContext context)
    {
        if (context is KeyboardCommandContext.TextInput && ShouldSuppressForTextInput(gesture))
        {
            return KeyboardRouteResult.Suppressed;
        }

        if (!gesture.Control && !gesture.Shift && !gesture.Alt)
        {
            return gesture.Key.ToUpperInvariant() switch
            {
                "ENTER" => KeyboardRouteResult.Command(VeloFileCommandId.Open),
                "F2" => KeyboardRouteResult.Command(VeloFileCommandId.Rename),
                "DELETE" => KeyboardRouteResult.Command(VeloFileCommandId.Delete),
                "F5" => KeyboardRouteResult.Command(VeloFileCommandId.Refresh),
                "BACKSPACE" => KeyboardRouteResult.Command(VeloFileCommandId.ParentFolder),
                "ESCAPE" => KeyboardRouteResult.Selection(SelectionKeyboardAction.ClearSelection),
                _ => KeyboardRouteResult.NotHandled
            };
        }

        if (!gesture.Control && gesture.Shift && !gesture.Alt
            && string.Equals(gesture.Key, "Delete", StringComparison.OrdinalIgnoreCase))
        {
            return KeyboardRouteResult.Command(VeloFileCommandId.PermanentDelete);
        }

        if (gesture.Control && !gesture.Shift && !gesture.Alt
            && string.Equals(gesture.Key, "A", StringComparison.OrdinalIgnoreCase))
        {
            return KeyboardRouteResult.Selection(SelectionKeyboardAction.SelectAll);
        }

        if (gesture.Control && gesture.Shift && !gesture.Alt)
        {
            return gesture.Key.ToUpperInvariant() switch
            {
                "C" => KeyboardRouteResult.Command(VeloFileCommandId.CopyPath),
                "N" => KeyboardRouteResult.Command(VeloFileCommandId.CopyName),
                _ => KeyboardRouteResult.NotHandled
            };
        }

        return KeyboardRouteResult.NotHandled;
    }

    private static bool ShouldSuppressForTextInput(KeyGesture gesture)
    {
        return TextInputSuppressedKeys.Contains(gesture.Key);
    }
}
