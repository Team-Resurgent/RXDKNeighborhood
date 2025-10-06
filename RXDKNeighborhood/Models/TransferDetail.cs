using ReactiveUI;
using System.Threading;

namespace RXDKNeighborhood.Models
{
    public enum TransferType
    {
        Download,
        Upload,
    }

    public class TransferDetail : ReactiveObject
    {
        private string _ipAddress = string.Empty;
        public string IpAddress
        {
            get => _ipAddress;
            set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
        }

        private TransferType _transferType;
        public TransferType TransferType
        {
            get => _transferType;
            set => this.RaiseAndSetIfChanged(ref _transferType, value);
        }

        private bool _failed;
        public bool Failed
        {
            get => _failed;
            set => this.RaiseAndSetIfChanged(ref _failed, value);
        }

        private string _progress = string.Empty;
        public string Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _sourcePath = string.Empty;
        public string SourcePath
        {
            get => _sourcePath;
            set => this.RaiseAndSetIfChanged(ref _sourcePath, value);
        }

        private string _destPath = string.Empty;
        public string DestPath
        {
            get => _destPath;
            set => this.RaiseAndSetIfChanged(ref _destPath, value);
        }

        private bool _isDirectory;
        public bool IsDirectory
        {
            get => _isDirectory;
            set => this.RaiseAndSetIfChanged(ref _isDirectory, value);
        }

        private ulong _fileSize;
        public ulong FileSize
        {
            get => _fileSize;
            set => this.RaiseAndSetIfChanged(ref _fileSize, value);
        }

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    }
}
