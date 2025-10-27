using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum KdType
    {
        Emable,
        Disable,
        Except,
        ExceptIf
    }

    public class Kd : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, KdType kdType)
        {
            var command = $"kd";
            if (kdType == KdType.Emable)
            {
                command += " enable";
            }
            else if (kdType == KdType.Disable)
            {
                command += " disable";
            }
            else if (kdType == KdType.Except)
            {
                command += " except";
            }
            else if (kdType == KdType.ExceptIf)
            {
                command += " exceptif";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}