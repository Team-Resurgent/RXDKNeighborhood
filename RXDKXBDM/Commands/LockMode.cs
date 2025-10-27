using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum LockModeType
    {
        Unlock,
        BoxId
    }

    public class LockMode : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, LockModeType lockModeType, bool encrypt)
        {
            var command = $"lockmode";
            if (lockModeType == LockModeType.Unlock)
            {
                command += " unlock";
            }
            else if (lockModeType != LockModeType.BoxId)
            {
                command += " boxid";
            }
            if (encrypt)
            {
                command += " encrypt";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
