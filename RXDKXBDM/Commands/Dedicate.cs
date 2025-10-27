using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class Dedicate : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string? handler)
        {
            var command = "dedicate";
            if (handler == null)
            {
                command += " global";
            }
            else
            {
                command += $" handler={handler}";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
