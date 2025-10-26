namespace RXDKXBDM.Commands
{
    public class Delete : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, bool isDir)
        {
            var command = $"delete name=\"{path}\"";
            if (isDir)
            {
                command += " dir";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
