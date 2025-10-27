using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetMem : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection, uint addr, uint length)
        {
            var command = $"getmem addr=0x{addr:x} length=0x{length:x}";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, socketResponse.Body);
            return commandResponse;
        }
    }
}
