using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class Rename : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, string newName)
        {
            var command = $"rename name=\"{path}\" newname=\"{newName}\""; 
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
