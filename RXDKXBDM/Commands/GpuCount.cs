using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GpuCount : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool enable)
        {
            var command = $"gpucount";
            command += enable ? " enabled" : " disable";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
