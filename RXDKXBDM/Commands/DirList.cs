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
        //name="crashdump.xdmp" sizehi=0x0 sizelo=0x8002000 createhi=0x01c3cc0a createlo=0xc39b9b00 changehi=0x01c3cc0a changelo=0xcbf3d600

        public async Task<CommandResponse<IDictionary<string, string>[]?>> SendAsync(Connection connection, string path)
        {
            var result = new List<Dictionary<string, string>>();
            var tempPath = path.EndsWith("\\") ? path : $"{path}\\";
            var command = $"dirlist name=\"{tempPath}\"";
            var response = await SendCommandAsync(connection, command);
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
                    result.Add(Utils.StringToDictionary(line));
                }
                return new CommandResponse<IDictionary<string, string>[]?>(response.ResponseCode, [.. result]);
            }
            return new CommandResponse<IDictionary<string, string>[]?>(response.ResponseCode, null);
        }
    }
}
