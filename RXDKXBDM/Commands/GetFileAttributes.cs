namespace RXDKXBDM.Commands
{
    public class GetFileAttributes : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>>> SendAsync(Connection connection, string path)
        {
            var command = $"getfileattributes name=\"{path}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<IDictionary<string, string>>(socketResponse.ResponseCode, Utils.BodyToDictionary(socketResponse.Body));
            return commandResponse;
        }
    }
}
