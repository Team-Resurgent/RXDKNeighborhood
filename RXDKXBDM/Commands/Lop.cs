using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum LopType
    {
        Stop,
        Info
    }

    public class Lop : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string cmd, uint evnt, uint counter)
        {
            var command = $"lop cmd=\"{cmd}\" start event=0x{evnt:x} counter=0x{counter:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string cmd, LopType lopType)
        {
            var command = $"lop cmd=\"{cmd}\"";
            if (lopType == LopType.Stop)
            {
                command += " stop";
            }
            else if (lopType == LopType.Info)
            {
                command += " info";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
