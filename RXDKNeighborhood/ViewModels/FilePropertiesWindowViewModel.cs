using ReactiveUI;
using RXDKNeighborhood.Extensions;
using RXDKXBDM.Commands;
using RXDKXBDM.Models;
using System;
using System.IO;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class FilePropertiesWindowViewModel : ViewModelBase<FilePropertiesWindow>
    {
        private FileSystemItem? _fileSystemItem;
        public FileSystemItem? FileSystemItem
        {
            get => _fileSystemItem;
            set
            {
                var changed = _fileSystemItem != value;
                this.RaiseAndSetIfChanged(ref _fileSystemItem, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(Size));
                    this.RaisePropertyChanged(nameof(Title));
                    this.RaisePropertyChanged(nameof(FileName));
                    this.RaisePropertyChanged(nameof(FileType));
                    this.RaisePropertyChanged(nameof(Location));
                    this.RaisePropertyChanged(nameof(Created));
                    this.RaisePropertyChanged(nameof(Modified));
                    ReadOnly = _fileSystemItem?.IsReadOnly ?? false;
                    Hidden = _fileSystemItem?.IsHidden ?? false;
                }
            }
        }

        private string? _ipAddress;
        public string? IpAddress
        {
            get => _ipAddress;
            set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
        }

        public string Size => StringExtension.FormatBytes(_fileSystemItem?.Size ?? 0);

        public string Title => $"{_fileSystemItem?.Name ?? string.Empty} Properties";

        public string FileName => _fileSystemItem?.Name ?? string.Empty;

        public string FileType =>  (Path.GetExtension(_fileSystemItem?.Name ?? string.Empty) + " File").Trim();

        public string Location => $"{_fileSystemItem?.Path.TrimEnd('\\') ?? string.Empty} (On {_ipAddress ?? string.Empty})";

        public string Created => _fileSystemItem?.Created.ToString("dddd, MMMM dd, yyyy h:mm:ss tt") ?? string.Empty;

        public string Modified => _fileSystemItem?.Changed.ToString("dddd, MMMM dd, yyyy h:mm:ss tt") ?? string.Empty;

        private bool _readOnly;
        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                var changed = _readOnly != value;
                this.RaiseAndSetIfChanged(ref _readOnly, value);
                if (changed)
                {
                    HasChanged = true;
                }
            }
        }

        private bool _hidden;
        public bool Hidden
        {
            get => _hidden;
            set
            {
                var changed = _hidden != value;
                this.RaiseAndSetIfChanged(ref _hidden, value);
                if (changed)
                {
                    HasChanged = true;
                }
            }
        }

        private bool _hasChanged;
        public bool HasChanged
        {
            get => _hasChanged;
            set => this.RaiseAndSetIfChanged(ref _hasChanged, value);
        }

        public event Action<bool>? OnClosing;

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public FilePropertiesWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(async () =>
            {
                if (HasChanged)
                {
                    if (IpAddress == null || FileSystemItem == null)
                    {
                        return;
                    }
                    using var connection = new RXDKXBDM.Connection();
                    if (await connection.OpenAsync(IpAddress) == true)
                    {
                        await SetFileAttributes.SendAsync(connection, Path.Combine(FileSystemItem.Path, FileSystemItem.Name), FileSystemItem.Created, FileSystemItem.Changed, Hidden, ReadOnly);
                    }
                }
                OnClosing?.Invoke(HasChanged);
                Owner?.Close();
            });

            CancelCommand = ReactiveCommand.Create(() =>
            {
                Owner?.Close();
            });
        }
    }
}
