using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class Notify : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection)
        {
            var command = $"notify";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
