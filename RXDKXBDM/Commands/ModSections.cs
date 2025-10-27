using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class ModSections : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection, string name)
        {
            var command = $"modsections name=\"{name}\"";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, socketResponse.Body);
            return commandResponse;
        }
    }
}
