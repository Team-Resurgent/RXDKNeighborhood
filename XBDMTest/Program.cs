using System.Net;
using XBDMTest.Commands;

namespace XBDMTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Globals.GlobalSharedConnection.UlXboxIPAddr = IPAddress.Parse("192.168.1.80");

            //RebootCommand.Execute(RebootFlags.DMBOOT_WARM, string.Empty);

            var xbeInfo = new XbeInfo();
            XbeInfoCommand.Execute("E:\\XBMC\\default.xbe", ref xbeInfo);

            Console.WriteLine("Hello, World!");
        }
    }
}
