using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class Reboot : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool warm)
        {
            var command = "reboot";
            if (warm)
            {
                command += " warm";
            }
            return await SendCommandAsync(connection, command);
        }
    }
}
