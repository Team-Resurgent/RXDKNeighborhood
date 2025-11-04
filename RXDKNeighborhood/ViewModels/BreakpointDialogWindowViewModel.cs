using ReactiveUI;
using RXDKNeighborhood.Views;
using RXDKXBDM;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tmds.DBus.Protocol;

namespace RXDKNeighborhood.ViewModels
{
    public class BreakpointDialogWindowViewModel : ViewModelBase<BreakpointDialogWindow>
    {
        public ObservableCollection<string> Files { get; } = [];

        private string _pdbPath = "";
        public string PdbPath
        {
            get => _pdbPath;
            set => this.RaiseAndSetIfChanged(ref _pdbPath, value);
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

        public BreakpointDialogWindowViewModel()
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


//var rva = address - _baseAddress;
//using var pdb = new PdbParser();
//pdb.LoadPdb(_pdbPath);
//if (pdb.TryGetFileLineByRva(rva, out string file, out var line, out var col))
//{
//    return $" line={line} file={file}";
//}