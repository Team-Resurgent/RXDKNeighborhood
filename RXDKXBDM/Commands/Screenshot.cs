namespace RXDKXBDM.Commands
{
    public class Screenshot : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, ExpectedSizeStream outputStream)
        {
            var socketResponse = await SendCommandAndGetResponseAsync(connection, "screenshot", outputStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
