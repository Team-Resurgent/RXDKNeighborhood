using RXDKXBDM.Commands;
using RXDKXBDM;
using RXDKNeighborhood.Helpers;
using RXDKXBDM.Models;

namespace RXDKTestRig
{
    public class Launcher // idisposable
    {
        private const int port = 5000;
        private EchoServer _echoServer;
        private Connection _connection;
        private uint _baseAddress;

        public Action? OnXbeLoaded;

        public Action<uint, uint>? OnBreakpoint;

        public Launcher()
        {
            _echoServer = new EchoServer(port);
            _echoServer.LineReceived += OnLineReceived;
            _echoServer.Start();
            _connection = new Connection();
        }

        private void OnLineReceived(object? sender, LineReceivedEventArgs e)
        {
            System.Diagnostics.Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [EVENT] Client '{e.ClientEndpoint}' Type: '{e.MessageType}' Message: '{e.Message}'");

            var paramDictionary = ParamParser.ParseParams(e.Message);
            if (e.MessageType.Equals("create") && paramDictionary.ContainsKey("stop") && paramDictionary.ContainsKey("thread"))
            {
                if (uint.TryParse(paramDictionary["thread"], out var thread))
                {
                    var modules = Modules.SendAsync(_connection).Result;
                    _baseAddress = modules.ResponseValue.Where(x => x.Name.Equals("PrometheOSXbe.exe")).First().Base;
                    var nostopOnResponseCode1 = NoStopOn.SendOptionsAsync(_connection, false, false, true).Result;
                    OnXbeLoaded?.Invoke();
                    SendContinue(thread);
                }
            }
            else if (e.MessageType.Equals("break") && paramDictionary.ContainsKey("stop") && paramDictionary.ContainsKey("addr") && paramDictionary.ContainsKey("thread"))
            {
                if (uint.TryParse(paramDictionary["addr"], out var addr))
                {
                    if (uint.TryParse(paramDictionary["thread"], out var thread))
                    {
                        OnBreakpoint?.Invoke(addr - _baseAddress, thread);
                    }
                }
            }
        }

        public async Task Launch()
        {
            var xboxItems = XboxDiscovery.Discover();
            if (xboxItems.Count() == 0)
            {
                System.Diagnostics.Debug.Print("No Xbox's found.");
            }

            var xboxIp = xboxItems.First().IpAddress;
            _connection = new Connection();
            System.Diagnostics.Debug.Print($"Attempting to connect to Xbox at {xboxIp}...");
            if (await _connection.OpenAsync(xboxIp) == false)
            {
                System.Diagnostics.Debug.Print("Failed to connect to Xbox! Check the Xbox IP address.");
                return;
            }
            System.Diagnostics.Debug.Print("Successfully connected to Xbox!");

            var rebootResponseCode = await Reboot.SendAsync(_connection, true, false, WaitType.Wait);
            var notifyResponseCode = await NotifyAt.SendAsync(_connection, port, null, NotifyAtType.Debug);
            if (notifyResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
            {
                System.Diagnostics.Debug.Print($"NotifyAt response: {notifyResponseCode.ResponseCode} {notifyResponseCode.ResponseValue}");
                return;
            }
            var isDebuggerResponseCode = await IsDebugger.SendAsync(_connection);
            var titleResponseCode = await Title.SendAsync(_connection, "PrometheOSXbe.xbe", "E:\\PrometheOSXbe\\", true);
            var debuggerResponseCode = await Debugger.SendAsync(_connection, DebuggerType.Connect);
            var stopOnResponseCode1 = await StopOn.SendOptionsAsync(_connection, false, false, true);
            var goResponseCode1 = await Go.SendAsync(_connection);

            System.Diagnostics.Debug.Print("Lauched Xbe!");
        }

        public void SendContinue(uint thread)
        {
            var continueResponseCode = Continue.SendAsync(_connection, thread, false).Result;
            var goResponseCode3 = Go.SendAsync(_connection).Result;
        }

        public void AddBreakpoint(uint address)
        {
            var virtAddress = _baseAddress + address;
            var breakResponseCode = Break.SendAddAsync(_connection, virtAddress).Result;
        }

        public void RemoveBreakpoint(uint address)
        {
            var virtAddress = _baseAddress + address;
            var breakResponseCode = Break.SendRemoveAsync(_connection, virtAddress).Result;
        }

        public ContextItem? GetContextInfo(uint address, uint thread)
        {
            var contextResponseCode = GetContext.SendAsync(_connection, thread, true, true, false, true).Result;
            //var contextExtResponseCode = GetExtContext.SendAsync(_connection, thread).Result;
            // use address to get synbols
            // use context info to cross ref symbol registers to memory
            // read memory and display variable contents

            return contextResponseCode.ResponseValue;
        }

        public byte[]? GetMem(uint address, uint length)
        {
            return GetMem2.SendAsync(_connection, address, length).Result.ResponseValue;
        }

        public void SendStop()
        {
            var haltResponse = Halt.SendAsync(_connection, 0);
        }

        public uint BaseAddress()
        {
            return _baseAddress;
        }

    }
}



//[2025-10-27 19:10:36] [EVENT] Client '192.168.1.93:11183' Type: 'modload' Message: 'name="PrometheOSXbe.exe" base=0x00011b40 size=0x0066f5c0 check=0x00000000 timestamp=0x00000000 tls xbe'

//var modules = Modules.SendAsync(connection).Result;
//var moduleBase = modules.ResponseValue.Where(x => x.Name.Equals("PrometheOSXbe.exe")).First().Base;
//var diaaddress = (uint)0x0023360E;
//var virt = moduleBase + diaaddress; // expected 0x0024514;
//var breakResponseCode = Break.SendAddAsync(connection, virt).Result;


//// what to do here, hacky try to resume all stopped threads
//for (int i = 0; i < 2; i++)
//{

//    await Task.Delay(1000);

//    var threadsResponseCode = Threads.SendAsync(connection).Result;
//    for (int j = 0; j < threadsResponseCode.ResponseValue.Length; j++)
//    {

//        var istoppedResponse = IsStopped.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
//        if (!istoppedResponse.ResponseValue.Contains("not stopped"))
//        {
//            var resumeResponse = Resume.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
//            var threadInfoResponse = ThreadInfo.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
//            var continueResponseCode = Continue.SendAsync(connection, threadsResponseCode.ResponseValue[j], false).Result;
//            var goResponseCode3 = await Go.SendAsync(connection);

//            await Task.Delay(1000);

//            //manually edit breakpoint based on persistent connection response
//            var breakResponseCode2 = Break.SendAsync(connection, false, false, false, true, BreakType.Addr, threadsResponseCode.ResponseValue[j]).Result;
//            var goResponseCode4 = await Go.SendAsync(connection);
//        }
//    }
//}