using ReactiveUI;
using RXDKNeighborhood.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class BreakpointDialogWindowViewModel : ViewModelBase<BreakpointDialogWindow>
    {
        public ObservableCollection<string> Files { get; } = [];

        private string _filterText = "";
        public string FilterText
        {
            get => _filterText;
            set
            {
                this.RaiseAndSetIfChanged(ref _filterText, value);
                UpdateFilteredFiles();
            }
        }

        public ObservableCollection<string> FilteredFiles { get; } = [];

        private string? _selectedFile;
        public string? SelectedFile
        {
            get => _selectedFile;
            set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
        }

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

            // Set up collection change handling
            Files.CollectionChanged += (s, e) => UpdateFilteredFiles();
            UpdateFilteredFiles(); // Initialize filtered collection
        }

        private void UpdateFilteredFiles()
        {
            FilteredFiles.Clear();
            
            var filtered = string.IsNullOrWhiteSpace(FilterText) 
                ? Files.ToList()
                : Files.Where(file => file.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var file in filtered)
            {
                FilteredFiles.Add(file);
            }
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