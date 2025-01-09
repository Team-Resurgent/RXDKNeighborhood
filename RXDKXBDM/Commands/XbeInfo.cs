namespace RXDKXBDM.Commands
{
    public class XbeInfo : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>>> SendAsync(Connection connection, string name)
        {
            string command = $"xbeinfo";
            if (string.IsNullOrEmpty(name))
            {
                command += " running";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<IDictionary<string, string>>(socketResponse.ResponseCode, Utils.BodyToDictionary(socketResponse.Body));
            return commandResponse;
        }
    }
}
