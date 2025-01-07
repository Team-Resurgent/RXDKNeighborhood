using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM.Commands
{
    public abstract partial class Command
    {
        internal CommandResponse<string> ParseResponse(string response)
        {
            if (response.Length < 5 || response[3] != '-')
            {
                return new CommandResponse<string>((int)ResponseCodes.ParseResponseInvalidLength, string.Empty);
            }

            if (int.TryParse(response.AsSpan(0, 3), out var responseCode) == false)
            {
                return new CommandResponse<string>((int)ResponseCodes.ParseResponseInvalidCode, string.Empty);
            }

            var responseString = response.Substring(4).Trim();
            return new CommandResponse<string>(responseCode, responseString);
        }

        internal async Task<CommandResponse<string>> SendCommandAsync(Connection connection, string command)
        {
            if (await connection.TrySendStringAsync($"{command}\r\n") != ConnectionState.Success)
            {
                return new CommandResponse<string>((int)ResponseCodes.TrySendStringFailed, string.Empty);
            }
            var response = await connection.TryRecieveStringAsync();
            if (response.Item1 != ConnectionState.Success)
            {
                return new CommandResponse<string>((int)ResponseCodes.TryRecieveString, string.Empty);
            }

            var commandResponse = ParseResponse(response.Item2);
            return commandResponse;
        }
    }
}
