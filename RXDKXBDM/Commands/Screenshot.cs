namespace RXDKXBDM.Commands
{
    public class Screenshot : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, CancellationToken cancellationToken, ExpectedSizeStream outputStream)
        {
            var socketResponse = await SendCommandAndGetResponseAsync(connection, "screenshot");
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
