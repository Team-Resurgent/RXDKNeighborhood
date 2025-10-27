using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetMem2 : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint addr, uint length, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getmem2 addr=0x{addr:x} length=0x{length:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
