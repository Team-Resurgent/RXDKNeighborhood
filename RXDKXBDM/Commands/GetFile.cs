using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetFile : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getfile name=\"{path}\"";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, uint offset, uint size, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getfile name=\"{path}\" offset=0x{offset:x} size=0x{size:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
