namespace RXDKXBDM.Commands
{
    public class Title : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name, string? dir, bool cmdLine)
        {
            var command = $"title name=\"{name}\"";
            if (!string.IsNullOrEmpty(dir))
            {
                command += $" dir=\"{dir}\"";
            }
            if (cmdLine)
            {
                command += " cmdline";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
