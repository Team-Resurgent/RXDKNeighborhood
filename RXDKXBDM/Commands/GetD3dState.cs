using RXDKXBDM.Commands.Helpers;
using System.Threading;

namespace RXDKXBDM.Commands
{
    public class GetD3dState : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"getd3dstate";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
