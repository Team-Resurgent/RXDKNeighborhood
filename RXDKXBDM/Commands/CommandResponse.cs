using RXDKXBDM.Models;

namespace RXDKXBDM.Commands
{
    public class SocketResponse
    {
        public ResponseCode ResponseCode { get; set; }

        public string Response { get; set; }

        public SocketResponse()
        {
            ResponseCode = ResponseCode.SUCCESS_OK;
            Response = string.Empty;
        }
    }

    public class MultiLineSocketResponse
    {
        public ResponseCode ResponseCode { get; set; }

        public string Response { get; set; }

        public string[] Body { get; set; }

        public MultiLineSocketResponse(SocketResponse socketResponse)
        {
            ResponseCode = socketResponse.ResponseCode;
            Response = socketResponse.Response;
            Body = [];
        }
    }

    public class ScreenshotSocketResponse
    {
        public ResponseCode ResponseCode { get; set; }

        public string Response { get; set; }

        public ScreenshotItem Screenshot { get; set; }

        public ScreenshotSocketResponse()
        {
            ResponseCode = ResponseCode.SUCCESS_OK;
            Response = string.Empty;
            Screenshot = new ScreenshotItem();
        }

        public ScreenshotSocketResponse(SocketResponse socketResponse)
        {
            ResponseCode = socketResponse.ResponseCode;
            Response = socketResponse.Response;
            Screenshot = new ScreenshotItem();
        }

        public ScreenshotSocketResponse(ScreenshotItem screenshot)
        {
            ResponseCode = ResponseCode.SUCCESS_OK;
            Response = string.Empty;
            Screenshot = screenshot;
        }
    }


    public class CommandResponse<T>
    {
        public ResponseCode ResponseCode { get; }

        public T ResponseValue { get; }

        public CommandResponse(ResponseCode responseCode, T responseValue)
        {
            ResponseCode = responseCode;
            ResponseValue = responseValue;
        }
    }
}
