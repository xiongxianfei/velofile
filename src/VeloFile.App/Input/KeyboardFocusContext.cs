namespace VeloFile.App.Input;

public enum AppKeyboardFocusScope
{
    Other,
    TextInput,
    FileList
}

public interface IKeyboardFocusContextProvider
{
    AppKeyboardFocusScope GetFocusScope();
}
