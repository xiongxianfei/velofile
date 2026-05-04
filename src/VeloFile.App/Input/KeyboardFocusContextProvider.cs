using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace VeloFile.App.Input;

public sealed class WinUiKeyboardFocusContextProvider : IKeyboardFocusContextProvider
{
    private const string TextInputScopeTag = "TextInputScope";
    private readonly FrameworkElement _root;
    private readonly FrameworkElement _fileListScope;

    public WinUiKeyboardFocusContextProvider(FrameworkElement root, FrameworkElement fileListScope)
    {
        _root = root;
        _fileListScope = fileListScope;
    }

    public AppKeyboardFocusScope GetFocusScope()
    {
        var focusedElement = _root.XamlRoot is null
            ? FocusManager.GetFocusedElement()
            : FocusManager.GetFocusedElement(_root.XamlRoot);
        var current = focusedElement as DependencyObject;
        var isInFileListScope = false;

        while (current is not null)
        {
            if (IsTextInputElement(current))
            {
                return AppKeyboardFocusScope.TextInput;
            }

            if (ReferenceEquals(current, _fileListScope))
            {
                isInFileListScope = true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return isInFileListScope ? AppKeyboardFocusScope.FileList : AppKeyboardFocusScope.Other;
    }

    private static bool IsTextInputElement(DependencyObject element)
    {
        return element switch
        {
            TextBox => true,
            PasswordBox => true,
            RichEditBox => true,
            AutoSuggestBox => true,
            FrameworkElement { Tag: TextInputScopeTag } => true,
            ComboBox comboBox when IsEditableComboBox(comboBox) => true,
            _ => false
        };
    }

    private static bool IsEditableComboBox(ComboBox comboBox)
    {
        var property = comboBox.GetType().GetProperty("IsEditable");
        return property?.GetValue(comboBox) is true;
    }
}
