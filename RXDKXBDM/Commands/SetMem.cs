using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class SetMem : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint addr, string data)
        {
            var command = $"setmem addr=0x{addr:x} data=\"{data}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
