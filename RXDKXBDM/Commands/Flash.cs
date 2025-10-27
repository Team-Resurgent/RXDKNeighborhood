using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class Flash : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint length, uint crc, bool ignoreversionchecking, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"sendfile length=0x{length:x} crc=0x{crc}";
            if (ignoreversionchecking)
            {
                command += " ignoreversionchecking";
            }
            var socketResponse = await SendCommandAndSetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
