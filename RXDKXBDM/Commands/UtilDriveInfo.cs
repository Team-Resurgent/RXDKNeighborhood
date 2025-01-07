using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Specialized;

namespace RXDKXBDM.Commands
{
    public class UtilDriveInfo : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>?>> SendAsync(Connection connection)
        {
            var response = await SendCommandAndGetResponseAsync(connection, "getutildrvinfo");
            if (response.IsSuccess())
            {
                var result = Utils.StringToDictionary(response.ResponseValue);
                return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, result);
            }
            return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, null);
        }
    }
}
