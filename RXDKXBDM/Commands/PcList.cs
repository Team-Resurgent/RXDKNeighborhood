using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class PcList : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection, uint addr, uint length)
        {
            var command = $"pclist";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, socketResponse.Body);
            return commandResponse;
        }
    }
}
