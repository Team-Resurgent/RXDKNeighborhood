namespace RXDKXBDM.Commands
{
    public class SocketResponse
    {
        public ResponseCode ResponseCode { get; set; }

        public string Response { get; set; }

        public SocketResponse()
        {
            ResponseCode = ResponseCode.SUCCESS_OK;
            Response = string.Empty;;
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

    public class BinarySocketResponse
    {
        public ResponseCode ResponseCode { get; set; }

        public string Response { get; set; }

        public byte[] Body { get; set; }

        public BinarySocketResponse(SocketResponse socketResponse)
        {
            ResponseCode = socketResponse.ResponseCode;
            Response = socketResponse.Response;
            Body = [];
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
