namespace RXDKXBDM.Commands
{
    public abstract partial class Command
    {
        internal static async Task<SocketResponse> SendCommandAndGetResponseAsync(Connection connection, string command, CancellationToken? cancellationToken = null, ExpectedSizeStream? binaryResponseStream = null)
        {
            var sendResponse = await connection.TrySendStringAsync($"{command}\r\n");
            if (Utils.IsSuccess(sendResponse.ResponseCode) == false)
            {
                return sendResponse;
            }
            var recieveResponse = await connection.TryRecieveBodyAsync(cancellationToken, binaryResponseStream);
            return recieveResponse;
        }

        internal static async Task<SocketResponse> SendCommandAsync(Connection connection, string command)
        {
            var response = await connection.TrySendStringAsync($"{command}\r\n");
            return response;
        }
    }
}
