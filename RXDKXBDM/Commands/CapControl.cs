using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class CapControl : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool start)
        {
            var command = $"capcontrol";
            if (start)
            {
                command += " start";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
