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

namespace RXDKNeighborhood.ViewModels
{
    public class DebugWindowViewModel : ViewModelBase<DebugWindow>
    {
        private EchoServer? _echoServer;
        private uint _baseAddress;
        private uint _addr;
        private uint _thread;
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

        private string _xbePath = "";
        public string XbePath
        {
            get => _xbePath;
            set => this.RaiseAndSetIfChanged(ref _xbePath, value);
        }

        private bool _isStopped;
        public bool IsStopped
        {
            get => _isStopped;
            set => this.RaiseAndSetIfChanged(ref _isStopped, value);
        }

        public ICommand LoadPdbCommand { get; }

        public ICommand ClearLogCommand { get; }

        public ICommand ContinueCommand { get; }

        public async void Opened()
        {
            _port = 5005;

            _echoServer = new EchoServer(_port);
            _echoServer.LineReceived += OnLineReceived;
            _echoServer.Start();

            using var connection = new Connection();
            if (await connection.OpenAsync(IpAddress) == false)
            {
                DebugLog += "Init failed.";
                return;
            }

            bool launchWithDebug = !string.IsNullOrEmpty(_xbePath);

            if (launchWithDebug)
            {
                var rebootResponseCode = await Reboot.SendAsync(connection, true, false, WaitType.Wait);
                if (rebootResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Init failed.";
                    return;
                }
            }

            var notifyResponseCode = await NotifyAt.SendAsync(connection, _port, null, NotifyAtType.Debug);
            if (notifyResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
            {
                DebugLog += "Init failed.";
                return;
            }

            if (launchWithDebug)
            {
                var isDebuggerResponseCode = await IsDebugger.SendAsync(connection);

                var xbeDirectory = (System.IO.Path.GetDirectoryName(_xbePath)?.TrimEnd('\\') ?? string.Empty) + "\\";
                var titleResponseCode = await RXDKXBDM.Commands.Title.SendAsync(connection, System.IO.Path.GetFileName(_xbePath), xbeDirectory, true);
                if (titleResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Init failed.";
                    return;
                }
                var debuggerResponseCode = await Debugger.SendAsync(connection, DebuggerType.Connect);
                if (debuggerResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Init failed.";
                    return;
                }
                var stopOnResponseCode = await StopOn.SendOptionsAsync(connection, false, false, true);
                if (stopOnResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Init failed.";
                    return;
                }
                var goResponseCode = await Go.SendAsync(connection);
                if (goResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Init failed.";
                    return;
                }
            }
        }

        public async void Closing()
        {
            using var connection = new Connection();
            if (await connection.OpenAsync(IpAddress))
            {
                _ = NotifyAt.SendAsync(connection, _port, null, NotifyAtType.Drop).Result;
            }
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

        private async void PdbProcess(StringBuilder logMessage, string messageType, string message)
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
                if (uint.TryParse(paramDictionary["addr"], out _addr) || uint.TryParse(paramDictionary["address"], out _addr))
                {
                    logMessage.Append(AddressToLogMessage(_addr));
                }
                if (keys.Contains("stop") && keys.Contains("thread"))
                {
                    using var connection = new Connection();
                    if (await connection.OpenAsync(IpAddress))
                    {
                        _ = await NoStopOn.SendOptionsAsync(connection, false, false, true);
                    }
                    if (uint.TryParse(paramDictionary["thread"], out _thread))
                    {
                        IsStopped = true;
                    }
                }
            }
            else if (messageType.Equals("create") && keys.Contains("stop") && keys.Contains("thread"))
            {
                if (uint.TryParse(paramDictionary["thread"], out _thread))
                {
                    _addr = 0;
                    IsStopped = true;
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

            if (logMessage.Length > 0)
            {
                logMessage.AppendLine();
                DebugLog += logMessage.ToString();
            }

            Dispatcher.UIThread.Invoke(() => { 
                Owner?.LogScrollViewer.ScrollToEnd();
            });
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

            ContinueCommand = ReactiveCommand.Create(async () =>
            {
                using var connection = new Connection();
                if (await connection.OpenAsync(IpAddress) == false)
                {
                    DebugLog += "Continue failed.";
                    return;
                }

                var continueResponseCode = await Continue.SendAsync(connection, _thread, false);
                if (continueResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Continue failed.";
                    return;
                }

                var goResponseCode = await Go.SendAsync(connection);
                if (goResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Continue failed.";
                    return;
                }

                IsStopped = false;
            });
        }
    }
}
