using VeloFile.App.ViewModels;
using VeloFile.Core.Commands;

namespace VeloFile.App.Input;

public sealed class AppFileCommandAcceleratorRouter
{
    private readonly AppShellViewModel _viewModel;
    private readonly IKeyboardFocusContextProvider _focusContextProvider;

    public AppFileCommandAcceleratorRouter(
        AppShellViewModel viewModel,
        IKeyboardFocusContextProvider focusContextProvider)
    {
        _viewModel = viewModel;
        _focusContextProvider = focusContextProvider;
    }

    public KeyboardRouteResult Route(KeyGesture gesture)
    {
        return _focusContextProvider.GetFocusScope() switch
        {
            AppKeyboardFocusScope.TextInput => _viewModel.HandleFileListShortcut(gesture, textInputHasFocus: true),
            AppKeyboardFocusScope.FileList => _viewModel.HandleFileListShortcut(gesture, textInputHasFocus: false),
            _ => KeyboardRouteResult.NotHandled
        };
    }
}
