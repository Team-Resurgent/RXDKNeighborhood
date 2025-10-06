using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RXDKNeighborhood.ViewModels;
using RXDKNeighborhood.Views;

namespace RXDKNeighborhood
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                var mainWindowViewModel = new MainWindowViewModel { Owner = mainWindow };
                mainWindow.DataContext = mainWindowViewModel;
                desktop.MainWindow = mainWindow;
            }
            base.OnFrameworkInitializationCompleted();
        }

    }
}