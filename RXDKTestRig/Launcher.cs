using RXDKXBDM.Commands;
using RXDKXBDM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RXDKNeighborhood.Helpers;
using System.Threading;

namespace RXDKTestRig
{
    public class Launcher
    {
        private const int port = 5000;

        private void OnLineReceived(object? sender, LineReceivedEventArgs e)
        {
            System.Diagnostics.Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [EVENT] Client '{e.ClientEndpoint}' Type: '{e.MessageType}' Message: '{e.Message}'");
        }

        public async Task Test()
        {
            System.Diagnostics.Debug.Print($"Starting EchoServer on port {port}...");
            var echoServer = new EchoServer(port);
            echoServer.LineReceived += OnLineReceived;
            echoServer.Start();

            var xboxItems = XboxDiscovery.Discover();
            if (xboxItems.Count() == 0)
            {
                System.Diagnostics.Debug.Print("No Xbox's found.");
            }

            var xboxIp = xboxItems.First().IpAddress;
            using var connection = new Connection();
            System.Diagnostics.Debug.Print($"Attempting to connect to Xbox at {xboxIp}...");
            if (await connection.OpenAsync(xboxIp) == false)
            {
                System.Diagnostics.Debug.Print("Failed to connect to Xbox! Check the Xbox IP address.");
                return;
            }
            System.Diagnostics.Debug.Print("Successfully connected to Xbox!");


            var response = WriteFile.SendAsync(connection, "E:\\PrometheOSXbe\\PrometheOSXbe2.xbe").Result;

            var bye = Bye.SendAsync(connection).Result;


            try
            {
                var rebootResponseCode = Reboot.SendAsync(connection, true, false, WaitType.Wait).Result;
                var notifyResponseCode = NotifyAt.SendAsync(connection, port, null, NotifyAtType.Debug).Result;
                if (notifyResponseCode.ResponseCode != ResponseCode.SUCCESS_OK)
                {
                    System.Diagnostics.Debug.Print($"NotifyAt response: {notifyResponseCode.ResponseCode} {notifyResponseCode.ResponseValue}");
                    return;
                }


                var isDebuggerResponseCode = IsDebugger.SendAsync(connection).Result;
                //var xbeInfoResponseCode = await XbeInfo.SendAsync(connection, "E:\\PrometheOSXbe\\PrometheOSXbe.xbe");
                var titleResponseCode = Title.SendAsync(connection, "PrometheOSXbe.xbe", "E:\\PrometheOSXbe\\", true).Result;
                var debuggerResponseCode = Debugger.SendAsync(connection, DebuggerType.Connect).Result;
                var breakResponseCode = Break.SendAsync(connection, false, true, false, false).Result;
                var stopOnResponseCode1 = StopOn.SendOptionsAsync(connection, false, false, true).Result;
                var goResponseCode1 = Go.SendAsync(connection).Result;
                //var stopOnResponseCode2 = StopOn.SendAsync(connection, true, false, false).Result;
                var modulesResponseCode = Modules.SendAsync(connection).Result;


                await Task.Delay(20000);


                // what to do here, hacky try to resume all stopped threads
                for (int i = 0; i < 2; i++)
                {

                    await Task.Delay(1000);

                    var threadsResponseCode = Threads.SendAsync(connection).Result;
                    for (int j = 0; j < threadsResponseCode.ResponseValue.Length; j++)
                    {

                        var istoppedResponse = IsStopped.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
                        if (!istoppedResponse.ResponseValue.Contains("not stopped"))
                        {
                            var resumeResponse = Resume.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
                            var threadInfoResponse = ThreadInfo.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
                            var continueResponseCode = Continue.SendAsync(connection, threadsResponseCode.ResponseValue[j], false).Result;
                            var goResponseCode3 = await Go.SendAsync(connection);

                            await Task.Delay(1000);

                            //manually edit breakpoint based on persistent connection response
                            var breakResponseCode2 = Break.SendAsync(connection, false, false, false, true, BreakType.Addr, threadsResponseCode.ResponseValue[j]).Result;
                            var goResponseCode4 = await Go.SendAsync(connection);
                        }
                    }
                }

         

                await Task.Delay(1000);


                while (true)
                {
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                //int q = 1;
            }
        }
    }
}
