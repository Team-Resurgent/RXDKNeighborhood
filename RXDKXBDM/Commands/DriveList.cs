using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RXDKXBDM.Commands
{
    public class DriveList : Command
    {
        public async Task<CommandResponse<string[]?>> SendAsync(Connection connection)
        {
            var response = await SendCommandAsync(connection, "drivelist");
            if (response.IsSuccess())
            {
                var result = response.ResponseValue.Select(c => c.ToString()).Order().ToArray();
                return new CommandResponse<string[]?>(response.ResponseCode, result);
            }
            return new CommandResponse<string[]?>(response.ResponseCode, null);
        }
    }
}
