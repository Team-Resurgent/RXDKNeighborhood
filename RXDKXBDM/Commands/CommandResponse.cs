namespace RXDKXBDM.Commands
{
    public class CommandResponse<T>
    {
        public int ResponseCode { get; }

        public T ResponseValue { get; }

        public bool IsSuccess()
        {
            return ResponseCode >= 200 && ResponseCode <= 299;
        }

        public CommandResponse(int responseCode, T responseValue)
        {
            ResponseCode = responseCode;
            ResponseValue = responseValue;
        }
    }
}
