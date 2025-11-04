using Avalonia.Controls;
using Avalonia.Input;
using RXDKNeighborhood.ViewModels;

namespace RXDKNeighborhood.Views;

public partial class BreakpointDialogWindow : Window
{
    public BreakpointDialogWindow()
    {
        InitializeComponent();

        Opened += (_, _) => this.FindControl<TextBox>("InputBox")?.Focus();
    }

    private void InputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is InputDialogWindowViewModel vm && vm.CanSubmit)
        {
            vm.OkCommand.Execute(null);
        }
    }
}