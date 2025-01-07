namespace RXDKXBDM.Commands
{
    public abstract partial class Command
    {
        public enum ResponseCodes
        {
            ParseResponseInvalidLength = 450,
            ParseResponseInvalidCode = 451,
            TrySendStringFailed = 452,
            TryRecieveString = 453,
        }
    }
}
