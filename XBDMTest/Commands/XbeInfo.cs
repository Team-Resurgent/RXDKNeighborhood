namespace XBDMTest.Commands
{
    public class XbeInfo
    {
        public string LaunchPath { get; set; }

        public ulong TimeStamp { get; set; }
        public ulong CheckSum { get; set; }

        public ulong StackSize { get; set; }

        public XbeInfo()
        {
            LaunchPath = string.Empty;
            TimeStamp = 0;
            CheckSum = 0;
            StackSize = 0;
        }
    }
}
