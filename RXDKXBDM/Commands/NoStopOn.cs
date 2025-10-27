using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class NoStopOn : Command
    {
        public static async Task<CommandResponse<string>> SendAllAsync(Connection connection)
        {
            var command = $"nostopon all";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendOptionsAsync(Connection connection, bool fce, bool debugstr, bool createthread)
        {
            var command = $"nostopon";
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
