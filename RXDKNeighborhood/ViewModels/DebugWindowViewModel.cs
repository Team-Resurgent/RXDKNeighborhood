using Avalonia.Platform.Storage;
using ReactiveUI;
using RXDKNeighborhood.Views;
using RXDKXBDM;
using RXDKXBDM.Commands;
using System.Windows.Input;
using System.Text;
using Avalonia.Threading;
using System;

namespace RXDKNeighborhood.ViewModels
{
    public class DebugWindowViewModel : ViewModelBase<DebugWindow>
    {
        private EchoServer? _echoServer;
        private uint _baseAddress;
        private int _port;
        private string? _pdbPath;

        public bool ShowMenu => OperatingSystem.IsWindows();

        private string _ipAddress = "";
        public string IpAddress
        {
            get => _ipAddress;
            set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
        }

        private string _debugLog = "";
        public string DebugLog
        {
            get => _debugLog;
            set => this.RaiseAndSetIfChanged(ref _debugLog, value);
        }

        public ICommand LoadPdbCommand { get; }

        private void OnLineReceived(object? sender, LineReceivedEventArgs e)
        {
            var logMessage = new StringBuilder($"{e.MessageType} {e.Message}");

            var paramDictionary = ParamParser.ParseParams(e.Message);
            if (e.MessageType.Equals("modload") && paramDictionary.ContainsKey("base") && paramDictionary.ContainsKey("xbe"))
            {
                if (uint.TryParse(paramDictionary["base"], out var addr))
                {
                    _baseAddress = addr;
                }
            }
            else if (e.MessageType.Equals("break") || e.MessageType.Equals("singlestep") || e.MessageType.Equals("exception"))
            {
                if (!paramDictionary.ContainsKey("addr") && !paramDictionary.ContainsKey("address"))
                {
                    return;
                }
                if (!string.IsNullOrEmpty(_pdbPath) && OperatingSystem.IsWindows())
                {
                    uint addr = 0;
                    if (uint.TryParse(paramDictionary["addr"], out addr) || uint.TryParse(paramDictionary["address"], out addr))
                    {
                        var rva = addr - _baseAddress;
                        using var pdb = new PdbHelper();
                        pdb.LoadPdb(_pdbPath);
                        if (pdb.TryGetFileLineByRva(rva, out string file, out var line, out var col))
                        {
                            logMessage.Append($" line={line} file={file}");
                        }
                    }
                }
            }

            logMessage.AppendLine();
            DebugLog += logMessage.ToString();

            Dispatcher.UIThread.Invoke(() => { 
                Owner?.LogScrollViewer.ScrollToEnd();
            });
        }

        public async void Start()
        {
            _port = 5005;

            _echoServer = new EchoServer(_port);
            _echoServer.LineReceived += OnLineReceived;
            _echoServer.Start();

            using var _connection = new Connection();
            if (await _connection.OpenAsync(IpAddress) == false)
            {
                DebugLog += "Failed to connect to Xbox!";
                return;
            }
            var notifyResponseCode = NotifyAt.SendAsync(_connection, _port, null, NotifyAtType.Debug).Result;
            if (notifyResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
            {
                DebugLog += "NotifyAt command failed, most likely hit max connections, warm reboot and try again.";
                return;
            }
        }


        public DebugWindowViewModel()
        {
            LoadPdbCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }

                try
                {
                    var options = new FilePickerOpenOptions
                    {
                        Title = "Open Pdb",
                        AllowMultiple = false,
                        FileTypeFilter = [new FilePickerFileType("PDB files") { Patterns = ["*.pdb"] }]
                    };

                    var files = await Owner.StorageProvider.OpenFilePickerAsync(options);
                    if (files == null)
                    {
                        return;
                    }

                    _pdbPath = files[0].Path.LocalPath;
                }
                catch
                {
                    // do nothing
                }
            });
        }
    }
}
