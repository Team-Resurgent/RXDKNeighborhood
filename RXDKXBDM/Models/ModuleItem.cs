namespace RXDKXBDM.Models
{
    public class ModeluleItem
    {
        public string Name { get; set; }

        public uint Base { get; set; }

        public uint Size { get; set; }

        public uint Check { get; set; }

        public uint TimeStamp { get; set; }

        public ModeluleItem()
        {
            Name = string.Empty;
            Base = 0;
            Size = 0;
            Check = 0;
            TimeStamp = 0;
        }
    }
}