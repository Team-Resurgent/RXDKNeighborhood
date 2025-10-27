using RXDKXBDM.Commands.Helpers;
using RXDKXBDM.Models;
using static System.Net.Mime.MediaTypeNames;

namespace RXDKXBDM.Commands
{
    public class Modules : Command
    {
        public static async Task<CommandResponse<ModeluleItem[]>> SendAsync(Connection connection)
        {
            var command = $"modules";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            if (Utils.IsSuccess(socketResponse.ResponseCode))
            {
                var modules = new List<ModeluleItem>();
                var moduleList = Utils.BodyToDictionaryArray(socketResponse.Body);
                for (var i = 0; i < moduleList.Length; i++)
                {
                    var itemProperties = moduleList[i];

                    var moduleItem = new ModeluleItem
                    {
                        Name = Utils.GetDictionaryString(itemProperties, "name"),
                        Base = Utils.GetDictionaryIntFromKey(itemProperties, "base"),
                        Size = Utils.GetDictionaryIntFromKey(itemProperties, "size"),
                        Check = Utils.GetDictionaryIntFromKey(itemProperties, "check"),
                        TimeStamp = Utils.GetDictionaryIntFromKey(itemProperties, "timestamp")
                    };
                    modules.Add(moduleItem);
                }
                return new CommandResponse<ModeluleItem[]>(ResponseCode.SUCCESS_OK, modules.ToArray());
            }
            return new CommandResponse<ModeluleItem[]>(socketResponse.ResponseCode, []);
        }
    }
}
