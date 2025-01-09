namespace RXDKXBDM.Commands
{
    public class UtilDriveInfo : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>>> SendAsync(Connection connection)
        {
            var socketResponse = await SendCommandAndGetResponseAsync(connection, "getutildrvinfo");
            var commandResponse = new CommandResponse<IDictionary<string, string>>(socketResponse.ResponseCode, Utils.BodyToDictionary(socketResponse.Body));
            return commandResponse;
        }
    }
}
