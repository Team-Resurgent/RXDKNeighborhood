using RXDKXBDM.Commands.Helpers;
using System.Xml.Linq;

namespace RXDKXBDM.Commands
{
    public class SetUserPriv : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name)
        {
            var command = $"setuserpriv name=\"{name}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
