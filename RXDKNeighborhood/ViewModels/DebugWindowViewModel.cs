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
using System.Collections.ObjectModel;
using DynamicData;
using System.Collections.Generic;

namespace RXDKNeighborhood.ViewModels
{
    public class Breakpoint
    {
        public required string File { get; set; }
        public required uint Line { get; set; }
        public required uint VirtualAddress { get; set; }
        public string VirtualAddressString => $"0x{VirtualAddress:X8}";
    }

    public class DebugWindowViewModel : ViewModelBase<DebugWindow>
    {
        private EchoServer? _echoServer;
        private uint _baseAddress;
        private uint _addr;
        private uint _thread;
        private int _port;
        private string? _pdbPath;

        public bool ShowMenu => OperatingSystem.IsWindows();

        public ObservableCollection<Breakpoint> Breakpoints { get; } = [];

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

        private bool _pdbLoaded;
        public bool PdbLoaded
        {
            get => _pdbLoaded;
            set => this.RaiseAndSetIfChanged(ref _pdbLoaded, value);
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

        public ICommand AddBreakpointCommand { get; }

        public ICommand RemoveBreakpointCommand { get; }

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

        private void AddressToLogMessage(StringBuilder logMessage, uint address)
        {
            if (string.IsNullOrEmpty(_pdbPath))
            {
                return;
            }
            var rva = address - _baseAddress;
            using var pdb = new PdbParser();
            pdb.LoadPdb(_pdbPath);
            if (pdb.TryGetFileLineByRva(rva, out string file, out var line, out var col))
            {
                logMessage.AppendLine($"    Line={line}, File='{file}'");
            }
        }

        private string ConvertMemoryDataToValue(byte[]? memData, RXDKXBDM.DetailedTypeInfo typeInfo)
        {
            if (memData == null || memData.Length == 0)
                return "null";

            if (memData.Length < typeInfo.Size)
                return "insufficient data";

            try
            {
                return typeInfo.TypeName.ToLower() switch
                {
                    "bool" => memData[0] != 0 ? "true" : "false",
                    "char" => $"'{(char)memData[0]}' (0x{memData[0]:X2})",
                    "wchar" => BitConverter.ToUInt16(memData, 0).ToString(),
                    "int8" => ((sbyte)memData[0]).ToString(),
                    "uint8" => memData[0].ToString(),
                    "int16" => BitConverter.ToInt16(memData, 0).ToString(),
                    "uint16" => BitConverter.ToUInt16(memData, 0).ToString(),
                    "int32" => BitConverter.ToInt32(memData, 0).ToString(),
                    "uint32" => BitConverter.ToUInt32(memData, 0).ToString(),
                    "int64" => BitConverter.ToInt64(memData, 0).ToString(),
                    "uint64" => BitConverter.ToUInt64(memData, 0).ToString(),
                    "float" => BitConverter.ToSingle(memData, 0).ToString("F6"),
                    "double" => BitConverter.ToDouble(memData, 0).ToString("F15"),
                    "void" => "void",
                    _ => ConvertUnknownType(memData, typeInfo)
                };
            }
            catch (Exception ex)
            {
                return $"conversion error: {ex.Message}";
            }
        }

        private string ConvertUnknownType(byte[] memData, RXDKXBDM.DetailedTypeInfo typeInfo)
        {
            // For unknown types, show raw hex data
            var hex = string.Join(" ", memData.Take((int)typeInfo.Size).Select(b => $"{b:X2}"));
            return $"raw: {hex}";
        }

        private string ConvertCharPointerToValue(Connection connection, byte[]? ptrData, RXDKXBDM.DetailedTypeInfo typeInfo)
        {
            if (ptrData == null || ptrData.Length < typeInfo.Size)
                return "null";

            try
            {
                // Get the pointer address (assuming 32-bit for Xbox)
                uint stringAddress = BitConverter.ToUInt32(ptrData, 0);
                
                if (stringAddress == 0)
                    return "null";

                // Read up to 256 characters from the string address
                var stringData = GetMem2.SendAsync(connection, stringAddress, 256).Result;
                if (stringData.ResponseCode != ResponseCode.SUCCESS_OK || stringData.ResponseValue == null)
                    return $"0x{stringAddress:X8} (read failed)";

                // Find the null terminator
                int nullIndex = Array.IndexOf(stringData.ResponseValue, (byte)0);
                if (nullIndex == -1)
                    nullIndex = Math.Min(stringData.ResponseValue.Length, 50); // Limit to 50 chars if no null found

                // Convert to string
                var chars = stringData.ResponseValue.Take(nullIndex).Select(b => (char)b);
                var stringValue = new string(chars.ToArray());
                
                // Escape special characters for display
                stringValue = stringValue.Replace("\\", "\\\\")
                                       .Replace("\"", "\\\"")
                                       .Replace("\n", "\\n")
                                       .Replace("\r", "\\r")
                                       .Replace("\t", "\\t");

                return $"\"{stringValue}\" (0x{stringAddress:X8})";
            }
            catch (Exception ex)
            {
                return $"conversion error: {ex.Message}";
            }
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
                    AddressToLogMessage(logMessage, _addr);
                }
                if (keys.Contains("stop") && keys.Contains("thread"))
                {
                    if (uint.TryParse(paramDictionary["thread"], out _thread))
                    {
                        IsStopped = true;
                        if (!string.IsNullOrEmpty(_pdbPath))
                        {
                            var rva = _addr - _baseAddress;
                            using var pdb = new PdbParser();
                            pdb.LoadPdb(_pdbPath);
                            if (pdb.TryGetSymbolsByRva(rva, out var variables))
                            {
                                using var connection = new Connection();
                                if (!connection.OpenAsync(IpAddress).Result)
                                {
                                    return;
                                }
                                var contextResponseCode = GetContext.SendAsync(connection, _thread, true, true, false, true).Result;
                                if (contextResponseCode.ResponseCode != ResponseCode.SUCCESS_OK || contextResponseCode.ResponseValue == null)
                                {
                                    return;
                                }

                                logMessage.AppendLine("    Variables:");
                                for (int i = 0; i < variables.Length; i++)
                                {
                                    var variable = variables[i];
                                    var typeInfo = variable.Type;
                                    if (typeInfo != null && typeInfo.IsValid)
                                    {
                                        if (!variable.Register.Equals("VFRAME"))
                                        {
                                            continue;
                                        }

                                        var arrayInfo = typeInfo.IsArray ? $" [{typeInfo.ElementCount}]" : "";
                                        var flags = new List<string>();
                                        if (typeInfo.IsPointer) flags.Add("pointer");
                                        if (typeInfo.IsArray) flags.Add("array");
                                        if (typeInfo.IsStruct) flags.Add("struct");
                                        if (typeInfo.IsEnum) flags.Add("enum");
                                        //var flagInfo = flags.Count > 0 ? $" [{string.Join(", ", flags)}]" : "";

                                        if (!typeInfo.IsPointer && !typeInfo.IsArray && !typeInfo.IsStruct && !typeInfo.IsEnum)
                                        {
                                            var memdata = GetMem2.SendAsync(connection, (uint)(contextResponseCode.ResponseValue.Ebp + variable.Offset), typeInfo.Size).Result;
                                            var value = ConvertMemoryDataToValue(memdata.ResponseValue, typeInfo);
                                            logMessage.AppendLine($"        {typeInfo.TypeName} {variable.Name}{arrayInfo} = {value}");
                                        }
                                        else if (typeInfo.IsPointer && typeInfo.TypeName.Equals("char*"))
                                        {
                                            var ptrData = GetMem2.SendAsync(connection, (uint)(contextResponseCode.ResponseValue.Ebp + variable.Offset), typeInfo.Size).Result;
                                            var value = ConvertCharPointerToValue(connection, ptrData.ResponseValue, typeInfo);
                                            logMessage.AppendLine($"        {typeInfo.TypeName} {variable.Name}{arrayInfo} = {value}");
                                        }
                                    }
                                    //else
                                    //{
                                    //    logMessage.Append($"        Unknown {variable.Name}: Currently unsupported");
                                    //    logMessage.AppendLine();
                                    //    logMessage.Append($"        Reg: {variable.Register} Offset: {variable.Offset}");
                                    //}
                                }
                            }
                        }
                    }
                }
            }
            else if (messageType.Equals("create") && keys.Contains("stop") && keys.Contains("thread"))
            {
                if (uint.TryParse(paramDictionary["thread"], out _thread))
                {
                    using var connection = new Connection();
                    if (await connection.OpenAsync(IpAddress))
                    {
                        _ = await NoStopOn.SendOptionsAsync(connection, false, false, true);
                    }
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
                        AddressToLogMessage(logMessage, addr);
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
                logMessage.AppendLine(index == -1 ? $"{e.MessageType} {e.Message}" : e.Message[(index + marker.Length)..].Trim());
            }
            else if (e.MessageType.Equals("hello"))
            {
                // dont output
            }
            else
            {
                logMessage.AppendLine($"{e.MessageType} {e.Message}");
            }

            PdbProcess(logMessage, e.MessageType, e.Message);

            Dispatcher.UIThread.Invoke(() => {
                DebugLog += logMessage.ToString();
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
                    PdbLoaded = true;

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

            AddBreakpointCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null || _pdbPath == null)
                {
                    return;
                }
                
                using var pdb = new PdbParser();
                pdb.LoadPdb(_pdbPath);
                if (pdb.TryGetFilenames(out var filenames))
                {
                    return;
                }

                var breakpointDialogWindow = new BreakpointDialogWindow();
                var breakpointDialogWindowViewModel = new BreakpointDialogWindowViewModel { Owner = breakpointDialogWindow, PdbPath = _pdbPath };
                breakpointDialogWindowViewModel.Files.AddRange(filenames);
                breakpointDialogWindow.DataContext = breakpointDialogWindowViewModel;
                breakpointDialogWindowViewModel.OnClosing += BreakpointDialogWindowViewModel_OnClosing;
                await breakpointDialogWindow.ShowDialog(Owner);
            });

            RemoveBreakpointCommand = ReactiveCommand.Create<Breakpoint>(async (breakpoint) =>
            {
                using var connection = new Connection();
                if (await connection.OpenAsync(IpAddress) == false)
                {
                    DebugLog += "Remove breakpoint failed.\n";
                    return;
                }

                var removeResponseCode = await Break.SendRemoveAsync(connection, breakpoint.VirtualAddress);
                if (removeResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Remove breakpoint failed.\n";
                    return;
                }

                Breakpoints.Remove(breakpoint);
            });
        }

        private async void BreakpointDialogWindowViewModel_OnClosing(string file, uint line)
        {
            if (_pdbPath == null)
            {
                return;
            }

            using var pdb = new PdbParser();
            pdb.LoadPdb(_pdbPath);
            if (pdb.TryGetRvaByFileLine(file, line, 0, out var rva))
            {
                using var connection = new Connection();
                if (await connection.OpenAsync(IpAddress) == false)
                {
                    DebugLog += "Add breakpoint failed.";
                    return;
                }

                uint virtualAddress = _baseAddress + rva;
                var breakResponseCode = await Break.SendAddAsync(connection, virtualAddress);
                if (breakResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    DebugLog += "Add breakpoint failed.";
                    return;
                }

                Breakpoints.Add(new Breakpoint { File = file, Line = line, VirtualAddress = virtualAddress } );
            }
        }
    }
}
