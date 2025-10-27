using RXDKXBDM.Commands.Helpers;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using System.Net;

namespace RXDKXBDM.Commands
{
    public class AltAddr : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection)
        {
            var command = $"altaddr";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            if (socketResponse.ResponseCode == ResponseCode.SUCCESS_OK)
            {
                var itemProperties = Utils.StringToDictionary(socketResponse.Response);
                if (itemProperties.ContainsKey("addr"))
                {
                    var address = Utils.GetDictionaryIntFromKey(itemProperties, "addr");
                    var bytes = BitConverter.GetBytes(address);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }
                    var ip = new IPAddress(bytes).ToString();
                    return new CommandResponse<string>(socketResponse.ResponseCode, ip);
                }
            }
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
