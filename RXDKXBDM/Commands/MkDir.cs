using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class MkDir : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path)
        {
            var command = $"mkdir name=\"{path}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
