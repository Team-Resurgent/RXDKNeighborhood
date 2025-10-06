namespace RXDKXBDM.Commands
{
    public class SendFile : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, long size, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"sendfile name=\"{path}\" length=\"0x{size:x}\"";
            var socketResponse = await SendCommandAndSetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
