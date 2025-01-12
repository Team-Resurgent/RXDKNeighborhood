namespace RXDKXBDM
{
    public abstract class ExpectedSizeStream : Stream
    {
        public abstract long ExpectedSize { get; set; }
    }
}
