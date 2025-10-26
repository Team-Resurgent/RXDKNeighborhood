namespace RXDKXBDM.Models
{
    public class ThreadInfoItem
    {
        public uint Suspend { get; set; }

        public uint Priority { get; set; }

        public uint TlsBase { get; set; }

        public uint Start { get; set; }

        public uint Base { get; set; }

        public uint Limit { get; set; }

        public DateTime Created { get; set; }
    }
}