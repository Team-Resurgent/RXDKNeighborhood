namespace RXDKXBDM.Models
{
    [Flags]
    public enum DirectoryItemFlag
    {
        File = 1,
        Directory = 2,
        ReadOnly = 4,
        Hidden = 8,
    }

    public class FileSystemItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public ulong Size { get; set; }

        public DateTime Created { get; set; }

        public DateTime Changed { get; set; }

        public DirectoryItemFlag Flags { get; set; }

        public bool IsReadOnly => (Flags & DirectoryItemFlag.ReadOnly) == DirectoryItemFlag.ReadOnly;

        public bool IsHidden => (Flags & DirectoryItemFlag.Hidden) == DirectoryItemFlag.Hidden;

        public bool IsFile => (Flags & DirectoryItemFlag.File) == DirectoryItemFlag.File;

        public bool IsDirectory => (Flags & DirectoryItemFlag.Directory) == DirectoryItemFlag.Directory;

        public bool CanDownload => IsFile || IsDirectory;

        public bool CanRename => IsFile || IsDirectory;

        public bool CanDelete => IsFile || IsDirectory;

        public bool CanLaunch => IsFile && Name.EndsWith(".xbe", StringComparison.CurrentCultureIgnoreCase);

        public bool CanShowProperties => IsFile || IsDirectory;

        public FileSystemItem()
        {
            Flags = DirectoryItemFlag.File;
            Name = string.Empty;
            Path = string.Empty;
            Size = 0;
            Created = DateTime.MinValue;
            Changed = DateTime.MinValue;
        }
    }
}