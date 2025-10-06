using System.IO;

namespace RXDKXBDM.Models
{
    public class ScreenshotItem
    {
        public uint Forrmat { get; set; }

        public uint Pitch { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }

        public byte[] Data { get; set; }

        public ScreenshotItem()
        {
            Forrmat = 0;
            Pitch = 0;
            Width = 0;
            Height = 0;
            Data = [];
        }

        public ScreenshotItem(IDictionary<string, string> properties, byte[] data)
        {
            Forrmat = Utils.GetDictionaryIntFromKey(properties, "format");
            Pitch = Utils.GetDictionaryIntFromKey(properties, "pitch");
            Width = Utils.GetDictionaryIntFromKey(properties, "width");
            Height = Utils.GetDictionaryIntFromKey(properties, "height");
            Data = data;
        }
    }
}