using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class IsBreak : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint addr)
        {
            var command = $"isbreak addr=0x{addr:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
