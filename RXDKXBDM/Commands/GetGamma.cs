using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetGamma : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = "getgamma";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
