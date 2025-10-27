using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{

    public class FileEof : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name, uint size)
        {
            var command = $"fileeof name=\"{name}\" size=0x{size:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
