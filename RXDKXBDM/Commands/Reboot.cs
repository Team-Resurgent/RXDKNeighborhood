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
        public static async Task<ResponseCode> SendAsync(Connection connection, bool warm)
        {
            var command = "reboot";
            if (warm)
            {
                command += " warm";
            }
            var socketResponse = await SendCommandAsync(connection, command);
            return socketResponse.ResponseCode;
        }
    }
}
