using RXDKXBDM;
using System.Collections.ObjectModel;

namespace RXDKNeighborhood.ViewModels
{
    public enum DriveItemType
    {
        Drive,
        Directory,
        File
    }

    //name="crashdump.xdmp" sizehi=0x0 sizelo=0x8002000 createhi=0x01c3cc0a createlo=0xc39b9b00 changehi=0x01c3cc0a changelo=0xcbf3d600


    public class DriveItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public ulong Size { get; set; }

        public DateTime Created { get; set; }

        public DateTime Changed { get; set; }

        public string ImageUrl { get; set; }

        public DriveItemType Type { get; set; }

        public bool HasProerties => true;

        public bool HasDownload => Type != DriveItemType.Drive;

        public bool HasDelete => Type != DriveItemType.Drive;

        public bool HasLaunch => Type == DriveItemType.File && Name.EndsWith(".xbe", StringComparison.CurrentCultureIgnoreCase);

        public DriveItem()
        {
            Type = DriveItemType.Drive;
            Name = string.Empty;
            Path = string.Empty;
            Size = 0;
            Created = DateTime.MinValue;
            Changed = DateTime.MinValue;
            ImageUrl = string.Empty;
        }
    }
}