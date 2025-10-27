using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class XtlInfo : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection)
        {
            var command = $"xtlinfo";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, socketResponse.Body);
            return commandResponse;
        }
    }
}
