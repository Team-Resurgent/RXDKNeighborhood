using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetUtilDriveInfo : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>>> SendAsync(Connection connection)
        {
            var socketResponse = await SendCommandAndGetResponseAsync(connection, "getutildrvinfo");
            if (socketResponse.ResponseCode == ResponseCode.SUCCESS_OK)
            {
                return new CommandResponse<IDictionary<string, string>>(socketResponse.ResponseCode, Utils.StringToDictionary(socketResponse.Response));
            }
            return new CommandResponse<IDictionary<string, string>>(socketResponse.ResponseCode, new Dictionary<string, string>());
        }
    }
}