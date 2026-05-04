using Microsoft.UI.Xaml;
using VeloFile.Core.Foundation;

namespace VeloFile.App;

public sealed partial class MainWindow : Window
{
    public MainWindow(InitialAppState initialState)
    {
        InitializeComponent();
        Title = initialState.WindowTitle;
    }
}
