using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class MagicBoot : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, bool debug)
        {
            var command = $"magicboot title=\"{path}\"";
            if (debug) 
            {
                command += " debug";
            }
            return await SendCommandAsync(connection, command);
        }
    }
}
