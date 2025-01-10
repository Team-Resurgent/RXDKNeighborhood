namespace RXDKXBDM.Models
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

        public bool IsReadOnly => (Flags & DriveItemFlag.ReadOnly) == DriveItemFlag.ReadOnly;

        public bool IsHidden => (Flags & DriveItemFlag.Hidden) == DriveItemFlag.Hidden;

        public bool IsDrive => (Flags & DriveItemFlag.Drive) == DriveItemFlag.Drive;

        public bool IsFile => (Flags & DriveItemFlag.File) == DriveItemFlag.File;

        public bool IsDirectory => (Flags & DriveItemFlag.Directory) == DriveItemFlag.Directory;

        public bool HasDownload => IsFile || IsDirectory;

        public bool HasDelete => IsFile || IsDirectory;

        public bool HasLaunch => IsFile && Name.EndsWith(".xbe", StringComparison.CurrentCultureIgnoreCase);

       

        public string CombinePath()
        {
            if (IsDrive)
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