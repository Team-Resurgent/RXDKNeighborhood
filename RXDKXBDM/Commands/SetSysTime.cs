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
            var nowValues = Utils.DateTimeToDictionary(DateTime.UtcNow);
            string command = $"setsystime clockhi={nowValues["hi"]} clocklo={nowValues["lo"]} tz=1";

            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, string.Empty);
            return commandResponse;
        }
    }
}
