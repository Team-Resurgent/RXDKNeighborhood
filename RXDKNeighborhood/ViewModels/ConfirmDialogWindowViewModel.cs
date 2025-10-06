using ReactiveUI;
using RXDKNeighborhood.Views;
using System;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class ConfirmDialogWindowViewModel : ViewModelBase<ConfirmDialogWindow>
    {
        private string _title = "";
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _prompt = "";
        public string Prompt
        {
            get => _prompt;
            set => this.RaiseAndSetIfChanged(ref _prompt, value);
        }

        public ICommand OkCommand { get; }

        public ICommand CloseCommand { get; }


        public event Action<bool>? OnClosing;

        public ConfirmDialogWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {
                OnClosing?.Invoke(true);
                Owner?.Close();
            });

            CloseCommand = ReactiveCommand.Create(() =>
            {
                OnClosing?.Invoke(false);
                Owner?.Close();
            });
        }
    }
}
