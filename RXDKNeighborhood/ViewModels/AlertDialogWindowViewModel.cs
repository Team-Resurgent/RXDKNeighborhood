using ReactiveUI;
using RXDKNeighborhood.Views;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class AlertDialogWindowViewModel : ViewModelBase<AlertDialogWindow>
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

        public AlertDialogWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {
                Owner?.Close();
            });
        }
    }
}
