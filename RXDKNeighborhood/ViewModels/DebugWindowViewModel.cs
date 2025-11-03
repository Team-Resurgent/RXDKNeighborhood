using Avalonia.Platform.Storage;
using ReactiveUI;
using RXDKNeighborhood.Views;
using RXDKXBDM;
using RXDKXBDM.Commands;
using System.Windows.Input;
using System.Text;
using Avalonia.Threading;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Linq;
using Avalonia.Controls.Shapes;
using System.IO;

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

        private string _title = "Debug Monitor";
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _debugLog = "";
        public string DebugLog
        {
            get => _debugLog;
            set => this.RaiseAndSetIfChanged(ref _debugLog, value);
        }

        public ICommand LoadPdbCommand { get; }

        public ICommand ClearLogCommand { get; }

        public void Closing()
        {
            _echoServer?.StopAsync();
        }

        private string AddressToLogMessage(uint address)
        {
            if (!string.IsNullOrEmpty(_pdbPath))
            {
                var rva = address - _baseAddress;
                using var pdb = new PdbParser();
                pdb.LoadPdb(_pdbPath);
                if (pdb.TryGetFileLineByRva(rva, out string file, out var line, out var col))
                {
                    return $" line={line} file={file}";
                }
            }
            return string.Empty;
        }

        private void PdbProcess(StringBuilder logMessage, string messageType, string message)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var paramDictionary = ParamParser.ParseParams(message);
            var keys = paramDictionary.Keys.ToArray();

            if (messageType.Equals("modload") && keys.Contains("base") && keys.Contains("xbe"))
            {
                if (uint.TryParse(paramDictionary["base"], out var addr))
                {
                    _baseAddress = addr;
                }
            }
            else if (messageType.Equals("break") || messageType.Equals("singlestep") || messageType.Equals("exception"))
            {
                if (!keys.Contains("addr") && !keys.Contains("address"))
                {
                    return;
                }
                if (uint.TryParse(paramDictionary["addr"], out var addr) || uint.TryParse(paramDictionary["address"], out addr))
                {
                    logMessage.Append(AddressToLogMessage(addr));
                }
            }
            else if (messageType.Equals("debugstr"))
            {
                if (keys.Length == 7 && keys.Contains("string") && paramDictionary["string"].Equals("Code") && keys[5] == "Addr")
                {
                    if (uint.TryParse(keys[6], NumberStyles.HexNumber, null, out var addr))
                    {
                        logMessage.Append(AddressToLogMessage(addr));
                    }
                }
            }
        }

        private void OnLineReceived(object? sender, LineReceivedEventArgs e)
        {
            var logMessage = new StringBuilder();

            if (e.MessageType.Equals("debugstr"))
            {
                const string marker = "string=";
                var index = e.Message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                logMessage.Append(index == -1 ? $"{e.MessageType} {e.Message}" : e.Message[(index + marker.Length)..].Trim());
            }
            else if (e.MessageType.Equals("hello"))
            { 
                // dont output
            }
            else
            {
                logMessage.Append($"{e.MessageType} {e.Message}");
            }

            PdbProcess(logMessage, e.MessageType, e.Message);
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

                    Title = $"Debug Monitor - Using {System.IO.Path.GetFileName(_pdbPath)}";
                }
                catch
                {
                    // do nothing
                }
            });

            ClearLogCommand = ReactiveCommand.Create(() =>
            {
                DebugLog = string.Empty;
            });
        }
    }
}
