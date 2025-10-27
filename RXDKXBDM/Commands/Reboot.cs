using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum WaitType
    {
        None,
        Stop,
        Wait
    }

    public class Reboot : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool warm, bool noDebug, WaitType waitType)
        {
            var command = "reboot";
            if (warm)
            {
                command += " warm";
            }
            if (noDebug)
            {
                command += " nodebug";
            }
            if (waitType != WaitType.None)
            {
                if (waitType == WaitType.Stop)
                {
                    command += " stop";
                }
                else if (waitType == WaitType.Wait)
                {
                    command += " wait";
                }
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            await Task.Delay(1000);
            await connection.Reconnect();
            return commandResponse;
        }
    }
}
