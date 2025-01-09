using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RXDKXBDM.Commands
{
    public class DriveList : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection)
        {
            var socketResponse = await SendCommandAndGetResponseAsync(connection, "drivelist");
            var result = socketResponse.Response.Select(c => c.ToString()).Order().ToArray();
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, result);
            return commandResponse;
        }
    }
}
