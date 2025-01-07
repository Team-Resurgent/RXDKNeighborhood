﻿using System;
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
                return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, Utils.MultilineResponseToDictionary(response.ResponseValue));
            }
            return new CommandResponse<IDictionary<string, string>?>(response.ResponseCode, null);
        }
    }
}
