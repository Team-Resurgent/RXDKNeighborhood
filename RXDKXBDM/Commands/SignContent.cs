using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class SignContent : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name, uint? titleid)
        {
            var command = $"signcontent name=\"{name}\"";
            if (titleid != null)
            {
                command += $" titleid=0x{titleid:x}";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
