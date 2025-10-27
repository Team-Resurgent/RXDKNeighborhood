using RXDKXBDM.Commands.Helpers;
using RXDKXBDM.Models;

namespace RXDKXBDM.Commands
{
    public class ThreadInfo : Command
    {
        public static async Task<CommandResponse<ThreadInfoItem>> SendAsync(Connection connection, uint thread)
        {
            var command = $"threadinfo thread=0x{thread:x}";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            var itemProperties = Utils.BodyToDictionary(socketResponse.Body);

            var threadItem = new ThreadInfoItem
            {
                Suspend = Utils.GetDictionaryIntFromKey(itemProperties, "suspend"),
                Priority = Utils.GetDictionaryIntFromKey(itemProperties, "priority"),
                TlsBase = Utils.GetDictionaryIntFromKey(itemProperties, "tlsbase"),
                Start = Utils.GetDictionaryIntFromKey(itemProperties, "start"),
                Base = Utils.GetDictionaryIntFromKey(itemProperties, "base"),
                Limit = Utils.GetDictionaryIntFromKey(itemProperties, "limit"),
                Created = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "createhi", "createlo"))
            };

            var commandResponse = new CommandResponse<ThreadInfoItem>(socketResponse.ResponseCode, threadItem);
            return commandResponse;
        }
    }
}
