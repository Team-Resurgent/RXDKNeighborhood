using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class MagicBoot : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, bool debug)
        {
            var command = $"magicboot title=\"{path}\"";
            if (debug) 
            {
                command += " debug";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
