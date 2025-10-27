using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class SetConfig : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint index, uint value)
        {
            var command = $"setconfig index=0x{index:x} value=0x{value:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
