using RXDKXBDM.Models;

namespace RXDKXBDM.Commands
{
    public class DirList : Command
    {
        public static async Task<CommandResponse<FileSystemItem[]>> SendAsync(Connection connection, string path)
        {
            var tempPath = path.EndsWith("\\") ? path : $"{path}\\";
            var command = $"dirlist name=\"{tempPath}\"";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            if (Utils.IsSuccess(socketResponse.ResponseCode))
            {
                var fileSystemItems = new List<FileSystemItem>();

                var dirList = Utils.BodyToDictionaryArray(socketResponse.Body);
                for (var i = 0; i < dirList.Length; i++)
                {
                    var itemProperties = dirList[i];

                    var name = Utils.GetDictionaryString(itemProperties, "name");
                    var size = Utils.GetDictionaryLongFromKeys(itemProperties, "sizehi", "sizelo");
                    var create = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "createhi", "createlo"));
                    var change = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "changehi", "changelo"));
                    var imageUrl = itemProperties.ContainsKey("directory") ? "directory.png" : "file.png";

                    var flags = itemProperties.ContainsKey("directory") ? DirectoryItemFlag.Directory : DirectoryItemFlag.File;
                    if (itemProperties.ContainsKey("readonly"))
                    {
                        flags |= DirectoryItemFlag.ReadOnly;
                    }
                    if (itemProperties.ContainsKey("hidden"))
                    {
                        flags |= DirectoryItemFlag.Hidden;
                    }

                    var fileSystemItem = new FileSystemItem { Name = name, Path = path, Size = size, Created = create, Changed = change, Flags = flags };
                    fileSystemItems.Add(fileSystemItem);
                }

                return new CommandResponse<FileSystemItem[]>(ResponseCode.SUCCESS_OK, fileSystemItems.ToArray());
            }

            return new CommandResponse<FileSystemItem[]>(socketResponse.ResponseCode, []);
        }
    }
}
