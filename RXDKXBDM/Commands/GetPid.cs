using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetPid : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection)
        {
            var command = $"getpid";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
