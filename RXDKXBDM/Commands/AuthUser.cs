using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class AuthUser : Command
    {
        public enum AuthUserType
        {
            Password,
            Response
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, AuthUserType authUserType, ulong authUserValue, string? username)
        {
            var command = $"authuser";
            if (authUserType == AuthUserType.Password)
            {
                command += $" passwd=0q{authUserValue:x}";
            }
            else if (authUserType == AuthUserType.Response)
            {
                command += $" resp=0q{authUserValue:x}";
            }
            if (username == null)
            {
                command += " admin";
            }
            else
            {
                command += $" name=\"{username}\"";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
