using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class SetSysTime : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool warm)
        {
            DateTime time = DateTime.UtcNow;
            DateTime fileTimeStart = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fileTimeTicks = (ulong)(time.ToUniversalTime() - fileTimeStart).Ticks;
            uint clockHigh = (uint)(fileTimeTicks >> 32);
            uint clockLow = (uint)(fileTimeTicks & 0xFFFFFFFF);
            string command = $"setsystime clockhi=0x{clockHigh:X} clocklo=0x{clockLow:X} tz=1";
            return await SendCommandAndGetResponseAsync(connection, command);
        }
    }
}
