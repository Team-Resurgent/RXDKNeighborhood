using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RXDKXBDM.Commands
{
    public class DirList : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>[]?>> SendAsync(Connection connection, string path)
        {
            var tempPath = path.EndsWith("\\") ? path : $"{path}\\";
            var command = $"dirlist name=\"{tempPath}\"";
            var result = new List<Dictionary<string, string>>();
            var response = await SendCommandAndGetResponseAsync(connection, command);
            if (response.IsSuccess())
            {
                return new CommandResponse<IDictionary<string, string>[]?>(response.ResponseCode, Utils.MultilineResponseToDictionaryArray(response.ResponseValue));
            }
            return new CommandResponse<IDictionary<string, string>[]?>(response.ResponseCode, null);
        }
    }
}
