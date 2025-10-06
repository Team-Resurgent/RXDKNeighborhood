using ReactiveUI;
using System.Windows.Input;

namespace RXDKNeighborhood.ViewModels
{
    public class DrivePropertiesWindowViewModel : ViewModelBase<DrivePropertiesWindow>
    {
        private string _title = "Drive Properties";
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private ulong _usedSpaceBytes = 3;
        public ulong UsedSpaceBytes
        {
            get => _usedSpaceBytes;
            set => this.RaiseAndSetIfChanged(ref _usedSpaceBytes, value);
        }

        private ulong _freeSpaceBytes = 1;
        public ulong FreeSpaceBytes
        {
            get => _freeSpaceBytes;
            set => this.RaiseAndSetIfChanged(ref _freeSpaceBytes, value);
        }

        private string _drive = "C on 192.168.1.102";
        public string Drive
        {
            get => _drive;
            set => this.RaiseAndSetIfChanged(ref _drive, value);
        }

        private string _type = "Main Volume";
        public string Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value);
        }

        private string _usedSpaceBytesFormatted = "0 bytes";
        public string UsedSpaceBytesFormatted
        {
            get => _usedSpaceBytesFormatted;
            set => this.RaiseAndSetIfChanged(ref _usedSpaceBytesFormatted, value);
        }

        private string _usedSpaceFormatted = "0 KB";
        public string UsedSpaceFormatted
        {
            get => _usedSpaceFormatted;
            set => this.RaiseAndSetIfChanged(ref _usedSpaceFormatted, value);
        }

        private string _freeSpaceBytesFormatted = "0 bytes";
        public string FreeSpaceBytesFormatted
        {
            get => _freeSpaceBytesFormatted;
            set => this.RaiseAndSetIfChanged(ref _freeSpaceBytesFormatted, value);
        }

        private string _freeSpaceFormatted = "0 KB";
        public string FreeSpaceFormatted
        {
            get => _freeSpaceFormatted;
            set => this.RaiseAndSetIfChanged(ref _freeSpaceFormatted, value);
        }

        private string _capacitySpaceBytesFormatted = "0 bytes";
        public string CapacitySpaceBytesFormatted
        {
            get => _capacitySpaceBytesFormatted;
            set => this.RaiseAndSetIfChanged(ref _capacitySpaceBytesFormatted, value);
        }

        private string _capacitySpaceFormatted = "0 KB";
        public string CapacitySpaceFormatted
        {
            get => _capacitySpaceFormatted;
            set => this.RaiseAndSetIfChanged(ref _capacitySpaceFormatted, value);
        }

        public ICommand OkCommand { get; }

        public DrivePropertiesWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {
                Owner?.Close();
            });
        }
    }
}
