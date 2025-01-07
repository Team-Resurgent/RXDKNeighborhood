using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class DebugName : Command
    {
        public async Task<CommandResponse<string>> SendAsync(Connection connection)
        {
            return await SendCommandAsync(connection, "dbgname");
        }
    }
}
