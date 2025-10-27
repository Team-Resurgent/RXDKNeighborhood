using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class PdbInfo : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint addr)
        {
            var command = $"pdbinfo addr=0x{addr:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
