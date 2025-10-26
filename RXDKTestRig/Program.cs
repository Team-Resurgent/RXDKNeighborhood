using RXDKXBDM.Commands;
using RXDKXBDM;

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
        private static async Task Test()
        {
            var aa = new EchoServer(port);
            aa.Start();

            using var connection = new Connection();
            if (await connection.OpenAsync("192.168.1.93") == false)
            {
                return;
            }
            try
            {

                var line = string.Empty;
                var success = false;

                var rebootResponseCode = Reboot.SendAsync(connection, true, false, WaitType.Wait).Result;

                var notifyResponseCode = NotifyAt.SendAsync(connection, port, "192.168.1.90", NotifyAtType.Debug).Result;

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
                Task.Delay(1000);
            }
        }
    }
}
