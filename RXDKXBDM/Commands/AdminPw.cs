using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class AdminPw : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, ulong? password)
        {
            var command = $"adminpw";
            if (password == null)
            {
                command += " none";
            }
            else
            {
                command += $" passwd=0q{password:x}";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
