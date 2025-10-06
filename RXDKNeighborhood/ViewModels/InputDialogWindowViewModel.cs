using ReactiveUI;
using RXDKNeighborhood.Views;
using System;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class InputDialogWindowViewModel : ViewModelBase<InputDialogWindow>
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

        private string _input = "";
        public string Input
        {
            get => _input;
            set
            {
                var changed = _input != value;
                this.RaiseAndSetIfChanged(ref _input, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(CanSubmit));
                }
            }
        }

        public ICommand OkCommand { get; }

        public ICommand CloseCommand { get; }

        public bool CanSubmit => !string.IsNullOrWhiteSpace(Input);

        public event Action<string?>? OnClosing;

        public InputDialogWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {
                OnClosing?.Invoke(Input);
                Owner?.Close();
            });

            CloseCommand = ReactiveCommand.Create(() =>
            {
                OnClosing?.Invoke(null);
                Owner?.Close();
            });
        }
    }
}
