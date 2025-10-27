using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class ServName : Command
    {
        public static async Task<CommandResponse<string>> SendNoneAsync(Connection connection, uint stackdepth, uint flags)
        {
            var command = $"servname none";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendIdAsync(Connection connection, uint id)
        {
            var command = $"servname id=0x{id:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendNameAsync(Connection connection, string name)
        {
            var command = $"servname name=\"{name}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
