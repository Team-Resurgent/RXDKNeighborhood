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

        public ObservableCollection<string> FilteredFiles { get; } = [];

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

        private string? _selectedFile;
        public string? SelectedFile
        {
            get => _selectedFile;
            set 
            {
                var changed = _selectedFile != value;
                this.RaiseAndSetIfChanged(ref _selectedFile, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(CanSubmit));
                }
            }
        }

        private string _pdbPath = "";
        public string PdbPath
        {
            get => _pdbPath;
            set => this.RaiseAndSetIfChanged(ref _pdbPath, value);
        }

        private string _line = "";
        public string Line
        {
            get => _line;
            set
            {
                var changed = _line != value;
                this.RaiseAndSetIfChanged(ref _line, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(CanSubmit));
                }
            }
        }

        public void LineChanging()
        {
            this.RaisePropertyChanged(nameof(CanSubmit));
        }

        public ICommand OkCommand { get; }

        public ICommand CloseCommand { get; }

        public bool CanSubmit
        {
            get
            {
                bool canSubmit = uint.TryParse(Line, out _) && !string.IsNullOrEmpty(SelectedFile);
                return canSubmit;
            }
        }

        public event Action<string, uint>? OnClosing;

        public BreakpointDialogWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrEmpty(SelectedFile) || !uint.TryParse(Line, out var line))
                {
                    return;
                }
                OnClosing?.Invoke(SelectedFile, line);
                Owner?.Close();
            });

            CloseCommand = ReactiveCommand.Create(() =>
            {
                OnClosing?.Invoke("", 0);
                Owner?.Close();
            });

            Files.CollectionChanged += (s, e) => UpdateFilteredFiles();
            UpdateFilteredFiles();
        }

        private void UpdateFilteredFiles()
        {
            FilteredFiles.Clear();
            var filtered = string.IsNullOrWhiteSpace(FilterText) ? Files.ToList() : Files.Where(file => file.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var file in filtered)
            {
                FilteredFiles.Add(file);
            }
        }
    }
}