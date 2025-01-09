using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class Download : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, ExpectedSizeStream outputStream)
        {
            var command = $"getfile name=\"{path}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command, outputStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, uint offset, uint size, ExpectedSizeStream outputStream)
        {
            var command = $"getfile name=\"{path}\" offset=0x{offset:x8} size=0x{offset:x8}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command, outputStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
