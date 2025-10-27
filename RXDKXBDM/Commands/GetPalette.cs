using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetPalette : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint stage, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getpalette stage=0x{stage:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
