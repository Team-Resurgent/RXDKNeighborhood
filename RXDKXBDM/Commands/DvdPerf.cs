using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class DvdPerf : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool start)
        {
            var command = $"dvdperf";
            command += start ? " start" : " stop";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
