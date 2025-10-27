using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class PsSnap : Command
    {
        public static async Task<CommandResponse<string[]>> SendAsync(Connection connection, uint x, uint y, uint? flags, uint? marker)
        {
            var command = $"pssnap x=0x{x:x} y=0x{y:x}";
            if (flags != null)
            {
                command += $" flags=0x{flags:x}";
            }
            if (marker != null)
            {
                command += $" marker=0x{marker:x}";
            }
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string[]>(socketResponse.ResponseCode, socketResponse.Body);
            return commandResponse;
        }
    }
}
