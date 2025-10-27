using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class UserList : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection)
        {
            var command = $"userlist";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, socketResponse.Body);
            return commandResponse;
        }
    }
}
