using Avalonia.Threading;
using ReactiveUI;
using RXDKNeighborhood.Extensions;
using RXDKNeighborhood.Helpers;
using RXDKXBDM.Commands;
using RXDKXBDM.Models;
using System;
using System.Threading;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class DirectoryPropertiesWindowViewModel : ViewModelBase<DirectoryPropertiesWindow>
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
                    this.RaisePropertyChanged(nameof(Title));
                    this.RaisePropertyChanged(nameof(FolderName));
                    this.RaisePropertyChanged(nameof(Location));
                    this.RaisePropertyChanged(nameof(Created));
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

        private string _size = "0 bytes";
        public string Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }

        private string _contains = "0 Files, 0 Folders";
        public string Contains
        {
            get => _contains;
            set => this.RaiseAndSetIfChanged(ref _contains, value);
        }

        public string Title => $"{_fileSystemItem?.Name ?? string.Empty} Properties";

        public string FolderName => _fileSystemItem?.Name ?? string.Empty;

        public string Location => $"{_fileSystemItem?.Path.TrimEnd('\\') ?? string.Empty} (On {_ipAddress ?? string.Empty})";

        public string Created => _fileSystemItem?.Created.ToString("dddd, MMMM dd, yyyy h:mm:ss tt") ?? string.Empty;

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

        public DirectoryPropertiesWindowViewModel()
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
                        await SetFileAttributes.SendAsync(connection, System.IO.Path.Combine(FileSystemItem.Path, FileSystemItem.Name), FileSystemItem.Created, FileSystemItem.Changed, Hidden, ReadOnly);
                    }
                }
                OnClosing?.Invoke(HasChanged);
                Owner?.Close();
            });

            CancelCommand = ReactiveCommand.Create(() =>
            {
                Owner?.Close();
            });

            if (Owner != null)
            {
                Owner.Opened += async (s, e) =>
                {
                    if (IpAddress == null || FileSystemItem == null)
                    {
                        return;
                    }
                    using var connection = new RXDKXBDM.Connection();
                    if (await connection.OpenAsync(IpAddress) == true)
                    {
                        await FolderHelper.GetFolderComtents(connection, FileSystemItem, _cancellationTokenSource.Token, (p) =>
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                Size = StringExtension.FormatBytes(p.TotalSize);
                                Contains = $"{p.FilesCount} Files, {p.FolderCount} Folders";
                            });
                        });
                    }
                };
            }
        }
    }
}
