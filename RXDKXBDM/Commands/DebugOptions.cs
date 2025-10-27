using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class DebugOptions : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool? crashdump, bool? dpctrace)
        {
            var command = $"debugoptions";
            if (crashdump != null)
            {
                command += $" crashdump={(crashdump == true ? 1 : 0)}";
            }
            if (dpctrace != null)
            {
                command += $" dpctrace={(dpctrace == true ? 1 : 0)}";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
