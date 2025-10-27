using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetSurface : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint id)
        {
            var command = $"getsurface id=0x{id:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
