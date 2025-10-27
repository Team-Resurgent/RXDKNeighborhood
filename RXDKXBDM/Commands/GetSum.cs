using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetSum : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint addr, uint length, uint blocksize, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getsum addr=0x{addr:x} length=0x{length:x} blocksize=0x{blocksize:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
