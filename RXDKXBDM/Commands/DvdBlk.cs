using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class DvdBlk : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint block, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"dvdblk block=%0x{block}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
