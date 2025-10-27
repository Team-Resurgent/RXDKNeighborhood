using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class KeyXchg : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"keyxchg";
            var socketResponse = await SendCommandAndSetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
