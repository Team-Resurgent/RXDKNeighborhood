using static RXDKXBDM.Commands.Command;

namespace RXDKXBDM.Commands
{
    public class SocketResponse
    {
        public ResponseCode ResponseCode { get; set; }

        public string Response { get; set; }

        public string[] Body { get; set; }

        public SocketResponse()
        {
            ResponseCode = ResponseCode.XBDM_SUCCESS_OK;
            Response = string.Empty;;
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
