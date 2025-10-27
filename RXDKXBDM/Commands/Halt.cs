using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class Halt : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint thread)
        {
            var command = $"halt thread=0x{thread:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
