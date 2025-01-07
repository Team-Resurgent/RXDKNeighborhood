using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class XbeInfo : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>?>> SendAsync(Connection connection, string name)
        {
            string command = $"xbeinfo";
            if (string.IsNullOrEmpty(name))
            {
                command += " running";
            }
            var result = new Dictionary<string, string>();
            var response = await SendCommandAndGetResponseAsync(connection, command);
            if (response.IsSuccess())
            {
                var lines = response.ResponseValue.Split("\r\n");
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line == ".")
                    {
                        break;
                    }
                    var temp = Utils.StringToDictionary(line);
                    var keys = temp.Keys.ToArray();
                    for (var j = 0; j < keys.Length; j++)
                    {
                        var key = keys[j];
                        result.Add(key, temp[key]);
                    }
                }
                return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, result);
            }
            return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, null);
        }
    }
}
