using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class StopOn : Command
    {
        public static async Task<CommandResponse<string>> SendAllAsync(Connection connection)
        {
            var command = $"stopon all";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendOptionsAsync(Connection connection, bool fce, bool debugstr, bool createthread)
        {
            var command = $"stopon";
            if (fce)
            {
                command += $" fce";
            }
            if (debugstr)
            {
                command += $" debugstr";
            }
            if (createthread)
            {
                command += $" createthread";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
