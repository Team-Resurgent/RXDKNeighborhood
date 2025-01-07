namespace RXDKXBDM.Commands
{
    public abstract partial class Command
    {
        public enum ResponseCodes
        {
            ParseResponseInvalidLength = 600,
            ParseResponseInvalidCode = 601,
            TrySendStringFailed = 602,
            TryRecieveString = 603,
        }
    }
}
