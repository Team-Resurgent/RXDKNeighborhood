using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class ModLong : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name)
        {
            var command = $"modlong name=\"{name}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
