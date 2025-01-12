namespace RXDKXBDM.Commands
{
    public abstract partial class Command
    {
        internal static async Task<SocketResponse> SendCommandAndGetResponseAsync(Connection connection, string command)
        {
            var sendResponse = await connection.TrySendStringAsync($"{command}\r\n");
            if (Utils.IsSuccess(sendResponse.ResponseCode) == false)
            {
                return sendResponse;
            }
            var recieveResponseHeader = connection.TryRecieveHeaderResponse();
            return recieveResponseHeader;
        }

        internal static async Task<MultiLineSocketResponse> SendCommandAndGetMultilineResponseAsync(Connection connection, string command)
        {
            var sendResponse = await connection.TrySendStringAsync($"{command}\r\n");
            if (Utils.IsSuccess(sendResponse.ResponseCode) == false)
            {
                return new MultiLineSocketResponse(sendResponse);
            }
            var recieveHeaderResponse = connection.TryRecieveHeaderResponse();
            if (recieveHeaderResponse.ResponseCode != ResponseCode.SUCCESS_MULTIRESPONSE)
            {
                return new MultiLineSocketResponse(sendResponse) { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }
            var body = connection.GetMultiLineResponse();
            return new MultiLineSocketResponse(sendResponse) { Body = body };
        }

        internal static async Task<SocketResponse> SendCommandAndGetBinaryResponseAsync(Connection connection, string command, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var sendResponse = await connection.TrySendStringAsync($"{command}\r\n");
            if (Utils.IsSuccess(sendResponse.ResponseCode) == false)
            {
                return sendResponse;
            }
            var recieveHeaderResponse = connection.TryRecieveHeaderResponse();
            if (recieveHeaderResponse.ResponseCode != ResponseCode.SUCCESS_BINRESPONSE)
            {
                return new SocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }
            var success = connection.TryRecieveBinarySize(out var expectedSize);
            if (success == false)
            {
                return new SocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            expectedSizeStream.ExpectedSize = expectedSize;

            var success2 = connection.TryStreamBinaryData(expectedSizeStream, cancellationToken);
            if (success2 == false)
            {
                return new SocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            return new SocketResponse { Response = "OK", ResponseCode = ResponseCode.SUCCESS_OK };
        }

        internal static async Task<SocketResponse> SendCommandAsync(Connection connection, string command)
        {
            var response = await connection.TrySendStringAsync($"{command}\r\n");
            return response;
        }
    }
}
