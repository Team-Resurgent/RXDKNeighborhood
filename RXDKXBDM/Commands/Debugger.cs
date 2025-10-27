using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum DebuggerType
    {
        Connect,
        Disconnect
    }

    public class Debugger : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, DebuggerType debuggerType)
        {
            var command = $"debugger";
            if (debuggerType == DebuggerType.Connect)
            {
                command += $" connect";
            }
            else if (debuggerType == DebuggerType.Disconnect)
            {
                command += $" disconnect";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
