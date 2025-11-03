using Avalonia.Controls;
using RXDKNeighborhood.ViewModels;

namespace RXDKNeighborhood.Views;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();

        Closing += (sender, e) => 
        {
            if (DataContext is not DebugWindowViewModel vm)
            {
                return;
            }
            vm.Closing();
        };
    }
}