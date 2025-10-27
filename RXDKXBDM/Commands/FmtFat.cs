using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class FmtFat : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint partition)
        {
            var command = $"fmtfat partition={partition}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
