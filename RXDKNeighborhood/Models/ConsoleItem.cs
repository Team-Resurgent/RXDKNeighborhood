using Avalonia.Media;
using ReactiveUI;
using RXDKXBDM.Models;

namespace RXDKNeighborhood.ViewModels
{
    public enum ConsoleItemType
    {
        AddXbox,
        Xbox,
        Drive,
        FileSystem,
    }

    public class ConsoleItem : ReactiveObject
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private object? _value;
        public object? Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        private IImage? _image;
        public IImage? Image
        {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }

        private ConsoleItemType _type;
        public ConsoleItemType Type
        {
            get => _type;
            set
            {
                var changed = _type != value;
                this.RaiseAndSetIfChanged(ref _type, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(CanRemove));
                    this.RaisePropertyChanged(nameof(CanDiscover));
                    this.RaisePropertyChanged(nameof(CanReboot));
                    this.RaisePropertyChanged(nameof(CanScreenshot));
                    this.RaisePropertyChanged(nameof(CanSynchronizeTime));
                    this.RaisePropertyChanged(nameof(CanDebug));
                    this.RaisePropertyChanged(nameof(CanDownload));
                    this.RaisePropertyChanged(nameof(CanLaunch));
                    this.RaisePropertyChanged(nameof(CanLaunchWithDebug));
                    this.RaisePropertyChanged(nameof(CanRename));
                    this.RaisePropertyChanged(nameof(CanDelete));
                    this.RaisePropertyChanged(nameof(CanShowProperties));
                    this.RaisePropertyChanged(nameof(ItemOpacity));
                }
            }
        }

        public bool CanRemove
        {
            get
            {
                return Type == ConsoleItemType.Xbox;
            }
        }

        public bool CanDiscover
        {
            get
            {
                return Type == ConsoleItemType.AddXbox;
            }
        }

        public bool CanReboot
        {
            get
            {
                return Type != ConsoleItemType.AddXbox;
            }
        }

        public bool CanScreenshot
        {
            get
            {
                return Type != ConsoleItemType.AddXbox;
            }
        }

        public bool CanSynchronizeTime
        {
            get
            {
                return Type != ConsoleItemType.AddXbox;
            }
        }

        public bool CanDebug
        {
            get
            {
                return Type != ConsoleItemType.AddXbox;
            }
        }

        public bool CanLaunchWithDebug
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.CanLaunch;
                }
                return false;
            }
        }

        public bool CanDownload
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.CanDownload;
                }
                return false;
            }
        }

        public bool CanLaunch
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.CanLaunch;
                }
                return false;
            }
        }

        public bool CanRename
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.CanRename;
                }
                return false;
            }
        }

        public bool CanDelete
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.CanDelete;
                }
                return false;
            }
        }

        public bool CanShowProperties
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.CanShowProperties;
                }
                if (Type == ConsoleItemType.Drive && Value is DriveItem driveItem)
                {
                    return driveItem.CanShowProperties;
                }
                return false;
            }
        }

        public float ItemOpacity
        {
            get
            {
                if (Type == ConsoleItemType.FileSystem && Value is FileSystemItem fileSystemItem)
                {
                    return fileSystemItem.IsHidden ? 0.5f : 1.0f;
                }
                return 1.0f;
            }
        }
    }
}