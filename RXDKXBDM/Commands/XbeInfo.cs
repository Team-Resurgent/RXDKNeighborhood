using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class XbeInfo : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>>> SendAsync(Connection connection, string name)
        {
            string command = $"xbeinfo";
            if (string.IsNullOrEmpty(name))
            {
                command += " running";
            }
            else
            {
                command += $" name=\"{name}\"";
            }
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<IDictionary<string, string>>(socketResponse.ResponseCode, Utils.BodyToDictionary(socketResponse.Body));
            return commandResponse;
        }
    }
}
