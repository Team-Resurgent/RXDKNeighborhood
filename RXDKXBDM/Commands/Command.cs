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
        internal static async Task<SocketResponse> SendCommandAndGetResponseAsync(Connection connection, string command, ExpectedSizeStream? binaryResponseStream = null)
        {
            if (await connection.TrySendStringAsync($"{command}\r\n") != ConnectionState.Success)
            {
                return new SocketResponse { ResponseCode = ResponseCode.TrySendStringFailed, Response = "SendCommandAndGetResponseAsync TrySendStringAsync Failed" };
            }
            var response = await connection.TryRecieveBodyAsync(binaryResponseStream);
            return response;
        }

        internal static async Task<SocketResponse> SendCommandAsync(Connection connection, string command)
        {
            if (await connection.TrySendStringAsync($"{command}\r\n") != ConnectionState.Success)
            {
                return new SocketResponse { ResponseCode = ResponseCode.TrySendStringFailed, Response = "SendCommandAsync TrySendStringAsync Failed" };
            }
            return new SocketResponse { ResponseCode = ResponseCode.XBDM_SUCCESS_OK, Response = "OK" };
        }
    }
}
