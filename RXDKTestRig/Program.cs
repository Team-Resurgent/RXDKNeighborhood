using RXDKXBDM.Commands;
using RXDKXBDM;
using System.Net;
using System.Net.NetworkInformation;

namespace RXDKTestRig
{
    //    XBEINFO NAME = "E:\PrometheOSXbe.xbe" ONDISKONLY
    //REBOOT WAIT WARM
    //TITLE NOPERSIST
    //TITLE NAME = "PrometheOs.xbe" DIR="e:\Prometheosxbe\" CMDLINE
    //DEBUGGER CONNECT
    //BREAK START
    //STOPON CREATETHREAD
    //GO
    //GETPID
    //STOPON CREATETHREAD FCE
    //THREADS
    //MODULES
    //threadinfo THREAD=28
    //CONTINUE THREAD = 28
    //GETCONTEXT THREAD = 28 CONTROL INT F P
    //GETCONTEXT THREAD = 28
    //MODSECTIONS NAME = "PrometheOSxbe.exe"
    //BREAK ADDR = 0X0024c b50
    //BREAK ADDR=0x001e0 eb1 CLEAR

    internal class Program
    {
        private const int port = 5002;

        private static void ShowCurrentIPAddresses()
        {
            System.Diagnostics.Debug.Print("=== Current Machine IP Addresses ===");
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        System.Diagnostics.Debug.Print($"IPv4: {ip}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print($"Error getting IP addresses: {ex.Message}");
            }
            System.Diagnostics.Debug.Print("=====================================");
        }

        private static void OnLineReceived(object? sender, LineReceivedEventArgs e)
        {
            System.Diagnostics.Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [EVENT] Client '{e.ClientEndpoint}' Type: '{e.MessageType}' Message: '{e.Message}'");
        }

        private static async Task Test()
        {
            ShowCurrentIPAddresses();
            System.Diagnostics.Debug.Print($"Starting EchoServer on port {port}...");
            var aa = new EchoServer(port);
            
            // Subscribe to the LineReceived event
            aa.LineReceived += OnLineReceived;
            
            aa.Start();
            System.Diagnostics.Debug.Print("EchoServer started! Waiting for connections...");

            using var connection = new Connection();
            System.Diagnostics.Debug.Print("Attempting to connect to Xbox at 192.168.1.93...");
            if (await connection.OpenAsync("192.168.1.93") == false)
            {
                System.Diagnostics.Debug.Print("Failed to connect to Xbox! Check the Xbox IP address.");
                return;
            }
            System.Diagnostics.Debug.Print("Successfully connected to Xbox!");
            try
            {

                var line = string.Empty;
                var success = false;

                var rebootResponseCode = Reboot.SendAsync(connection, true, false, WaitType.Wait).Result;
                await Task.Delay(1000);

                // TODO: Update this IP to your current machine's IP address
                System.Diagnostics.Debug.Print($"Sending NotifyAt command to Xbox - telling it to send debug notifications to 192.168.1.90:{port}");
                System.Diagnostics.Debug.Print("WARNING: Make sure 192.168.1.90 is your current machine's IP address!");
                var notifyResponseCode = NotifyAt.SendAsync(connection, port, "192.168.1.90", NotifyAtType.Debug).Result;
                System.Diagnostics.Debug.Print($"NotifyAt response: {notifyResponseCode.ResponseCode} {notifyResponseCode.ResponseValue}");

                var isDebuggerResponseCode = IsDebugger.SendAsync(connection).Result;

                var xbeInfoResponseCode = await XbeInfo.SendAsync(connection, "E:\\PrometheOSXbe\\PrometheOSXbe.xbe");
                var titleResponseCode = Title.SendAsync(connection, "PrometheOSXbe.xbe", "E:\\PrometheOSXbe\\", true).Result;
                var debuggerResponseCode = Debugger.SendAsync(connection, DebuggerType.Connect).Result;
                var breakResponseCode = Break.SendAsync(connection, false, true, false, false).Result;
                var stopOnResponseCode1 = StopOn.SendAsync(connection, false, false, true).Result;
                var goResponseCode1 = Go.SendAsync(connection).Result;
                var stopOnResponseCode2 = StopOn.SendAsync(connection, true, false, false).Result;
                var modulesResponseCode = Modules.SendAsync(connection).Result;



                // what to do here, hacky try to resume all stopped threads
                for (int i = 0; i < 2; i++)
                {

                    await Task.Delay(1000);

                    var threadsResponseCode = Threads.SendAsync(connection).Result;
                    for (int j = 0; j < threadsResponseCode.ResponseValue.Length; j++)
                    {
      
                        var istoppedResponse = IsStopped.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
                        if (istoppedResponse.ResponseValue.Equals("stopped"))
                        {
                            var resumeResponse = Resume.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
                            var threadInfoResponse = ThreadInfo.SendAsync(connection, threadsResponseCode.ResponseValue[j]).Result;
                            var continueResponseCode = Continue.SendAsync(connection, threadsResponseCode.ResponseValue[j], false).Result;
                            var goResponseCode3 = await Go.SendAsync(connection);

                            await Task.Delay(1000);

                            //manually edit breakpoint based on persistent connection response
                            var breakResponseCode2 = Break.SendAsync(connection, false, false, false, true, BreakType.Addr, 0x0000001C).Result;
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
                int q = 1;
            }
        }

        static void Main(string[] args)
        {
           
            _ = Task.Run(async () =>
            {
                await Test();
            });

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
