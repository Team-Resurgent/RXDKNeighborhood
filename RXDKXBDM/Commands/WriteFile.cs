using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class WriteFile : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name, uint offset, uint length, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"writefile name=\"{name}\" offset=0x{offset:x} length=0x{length:x}";
            var socketResponse = await SendCommandAndSetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
