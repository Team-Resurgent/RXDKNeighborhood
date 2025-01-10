using RXDKXBDM.Models;

namespace RXDKXBDM.Commands
{
    public class DirList : Command
    {
        public static async Task<CommandResponse<DriveItem[]>> SendAsync(Connection connection, string path)
        {
            var tempPath = path.EndsWith("\\") ? path : $"{path}\\";
            var command = $"dirlist name=\"{tempPath}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            if (Utils.IsSuccess(socketResponse.ResponseCode))
            {
                var driveItems = new List<DriveItem>();

                var dirList = Utils.BodyToDictionaryArray(socketResponse.Body);
                for (var i = 0; i < dirList.Length; i++)
                {
                    var itemProperties = dirList[i];

                    var name = Utils.GetDictionaryString(itemProperties, "name");
                    var size = Utils.GetDictionaryLongFromKeys(itemProperties, "sizehi", "sizelo");
                    var create = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "createhi", "createlo"));
                    var change = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "changehi", "changelo"));
                    var imageUrl = itemProperties.ContainsKey("directory") ? "directory.png" : "file.png";

                    var flags = itemProperties.ContainsKey("directory") ? DriveItemFlag.Directory : DriveItemFlag.File;
                    if (itemProperties.ContainsKey("readonly"))
                    {
                        flags |= DriveItemFlag.ReadOnly;
                    }
                    if (itemProperties.ContainsKey("hidden"))
                    {
                        flags |= DriveItemFlag.Hidden;
                    }

                    var driveItem = new DriveItem { Name = name, Path = path, Size = size, Created = create, Changed = change, ImageUrl = imageUrl, Flags = flags };
                    driveItems.Add(driveItem);
                }

                return new CommandResponse<DriveItem[]>(ResponseCode.SUCCESS_OK, driveItems.ToArray());
            }

            return new CommandResponse<DriveItem[]>(socketResponse.ResponseCode, []);
        }
    }
}
