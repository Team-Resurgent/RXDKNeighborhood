using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class VsSnap : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint first, uint last, uint? flags, uint? marker, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"vssnap first=0x{first:x} last=0x{last:x}";
            if (flags != null)
            {
                command += $" flags=0x{flags:x}";
            }
            if (marker != null)
            {
                command += $" marker=0x{marker:x}";
            }
            var socketResponse = await SendCommandAndGetOptionalBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
