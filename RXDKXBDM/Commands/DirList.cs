namespace RXDKXBDM.Commands
{
    public class DirList : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>[]>> SendAsync(Connection connection, string path)
        {
            var tempPath = path.EndsWith("\\") ? path : $"{path}\\";
            var command = $"dirlist name=\"{tempPath}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<IDictionary<string, string>[]>(socketResponse.ResponseCode, Utils.BodyToDictionaryArray(socketResponse.Body));
            return commandResponse;
        }
    }
}
