using RXDKXBDM.Models;

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

            if (connection.TryRecieveBinarySize(out var expectedSize) == false)
            {
                return new SocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            expectedSizeStream.ExpectedSize = expectedSize;

            if (connection.TryStreamBinaryData(expectedSizeStream, cancellationToken) == false)
            {
                return new SocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            return new SocketResponse { Response = "OK", ResponseCode = ResponseCode.SUCCESS_OK };
        }

        internal static async Task<ScreenshotSocketResponse> SendCommandAndGetBinaryScreenshotResponseAsync(Connection connection, string command, CancellationToken cancellationToken, DownloadStream outputStream)
        {
            var sendResponse = await connection.TrySendStringAsync($"{command}\r\n");
            if (Utils.IsSuccess(sendResponse.ResponseCode) == false)
            {
                return new ScreenshotSocketResponse(sendResponse);
            }
            var recieveHeaderResponse = connection.TryRecieveHeaderResponse();
            if (recieveHeaderResponse.ResponseCode != ResponseCode.SUCCESS_BINRESPONSE)
            {
                return new ScreenshotSocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            if (connection.TryRecieveLine(out var line) == false)
            {
                return new ScreenshotSocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            var properties = Utils.StringToDictionary(line);

            var frameBufferSize = Utils.GetDictionaryIntFromKey(properties, "framebuffersize");

            using var memoryStream = new MemoryStream();
            using var downloadStream = new DownloadStream(memoryStream);
            downloadStream.ExpectedSize = frameBufferSize;

            if (connection.TryStreamBinaryData(downloadStream, cancellationToken) == false)
            {
                return new ScreenshotSocketResponse { Response = "Unexpected Result", ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR };
            }

            var screenshot = new ScreenshotItem(properties, memoryStream.ToArray());

            return new ScreenshotSocketResponse(sendResponse) { Response = "OK", ResponseCode = ResponseCode.SUCCESS_OK, Screenshot = screenshot };
        }

        internal static async Task<SocketResponse> SendCommandAsync(Connection connection, string command)
        {
            var response = await connection.TrySendStringAsync($"{command}\r\n");
            return response;
        }
    }
}
