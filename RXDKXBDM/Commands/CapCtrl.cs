using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum CapCtrlType
    {
        FastCapEnabled,
        Stop
    }

    public class CapCtrl : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name, uint buffersize)
        {
            var command = $"capctrl name=\"{name}\" buffersize=0x{buffersize:x} start";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, CapCtrlType capCtrlType)
        {
            var command = $"capctrl";
            if (capCtrlType == CapCtrlType.FastCapEnabled)
            {
                command += " fastcapenabled";
            }
            else if (capCtrlType == CapCtrlType.Stop)
            {
                command += " stop";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
