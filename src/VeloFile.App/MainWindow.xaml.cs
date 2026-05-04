using Microsoft.UI.Xaml;
using VeloFile.App.ViewModels;
using VeloFile.Core.Foundation;

namespace VeloFile.App;

public sealed partial class MainWindow : Window
{
    public MainWindow(InitialAppState initialState)
    {
        InitializeComponent();
        Title = initialState.WindowTitle;
        RootShell.DataContext = AppShellViewModel.Create(initialState);
    }
}
