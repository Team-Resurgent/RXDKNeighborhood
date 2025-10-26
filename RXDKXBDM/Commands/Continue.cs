namespace RXDKXBDM.Commands
{
    public class Continue : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint thread, bool exception)
        {
            var command = $"continue thread={thread}";
            if (exception)
            {
                command += " exception";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
