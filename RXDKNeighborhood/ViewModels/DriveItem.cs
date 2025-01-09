namespace RXDKNeighborhood.ViewModels
{
    [Flags]
    public enum DriveItemFlag
    {
        File = 1,
        Directory = 2,
        ReadOnly = 4,
        Hidden = 8,
        Drive = 16
    }

    public class DriveItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public ulong Size { get; set; }

        public DateTime Created { get; set; }

        public DateTime Changed { get; set; }

        public string ImageUrl { get; set; }

        public DriveItemFlag Flags { get; set; }

        public bool HasProerties => true;

        public bool HasDownload => (Flags & DriveItemFlag.File) == DriveItemFlag.File || (Flags & DriveItemFlag.Directory) == DriveItemFlag.Directory;

        public bool HasDelete => (Flags & DriveItemFlag.File) == DriveItemFlag.File || (Flags & DriveItemFlag.Directory) == DriveItemFlag.Directory;

        public bool HasLaunch => (Flags & DriveItemFlag.File) == DriveItemFlag.File && Name.EndsWith(".xbe", StringComparison.CurrentCultureIgnoreCase);

        public string CombinePath()
        {
            if ((Flags & DriveItemFlag.Drive) == DriveItemFlag.Drive)
            {
                return Path;
            }
            return System.IO.Path.Combine(Path, Name);
        }

        public DriveItem()
        {
            Flags = DriveItemFlag.Drive;
            Name = string.Empty;
            Path = string.Empty;
            Size = 0;
            Created = DateTime.MinValue;
            Changed = DateTime.MinValue;
            ImageUrl = string.Empty;
        }
    }
}