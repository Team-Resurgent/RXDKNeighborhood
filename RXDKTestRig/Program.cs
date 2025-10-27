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
        

        static void Main(string[] args)
        {
           
            _ = Task.Run(async () =>
            {
                var launcher = new Launcher();
                await launcher.Test();
            });

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
