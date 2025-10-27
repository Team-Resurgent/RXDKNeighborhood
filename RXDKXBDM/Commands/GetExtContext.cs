using RXDKXBDM.Commands.Helpers;
using System.Threading;

namespace RXDKXBDM.Commands
{
    public class GetExtContext : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint thread, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getextcontext thread=0x{thread:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
