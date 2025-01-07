using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class DriveFreeSpace : Command
    {
        public static async Task<CommandResponse<IDictionary<string, string>?>> SendAsync(Connection connection, string path)
        {
            var command = $"drivefreespace name=\"{path}\\\"";
            var response = await SendCommandAndGetResponseAsync(connection, command);
            var result = new List<Dictionary<string, string>>();
            if (response.IsSuccess())
            {
                return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, Utils.MultilineResponseToDictionary(response.ResponseValue));
            }
            return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, null);
        }
    }
}
