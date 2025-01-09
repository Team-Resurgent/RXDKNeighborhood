﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public class ActiveTitle : Command
    {
        public async Task<CommandResponse<string>> SendAsync(Connection connection)
        {
            var socketResponse = await SendCommandAndGetResponseAsync(connection, "xbeinfo running");
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
