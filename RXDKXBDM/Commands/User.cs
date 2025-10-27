using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class User : Command
    {
        public static async Task<CommandResponse<string>> SendAddAsync(Connection connection, string name, string? password)
        {
            var command = $"user name=\"{name}\"";
            if (!string.IsNullOrEmpty(password))
            {
                command += $" passwd=\"{password}\"";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendRemoveAsync(Connection connection, string name)
        {
            var command = $"user name=\"{name}\" remove";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
