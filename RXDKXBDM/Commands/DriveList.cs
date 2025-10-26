using RXDKXBDM.Models;
using System.IO;

namespace RXDKXBDM.Commands
{
    public class DriveList : Command
    {
        public static async Task<CommandResponse<DriveItem[]>> SendAsync(Connection connection)
        {
            var command = "drivelist";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var result = socketResponse.Response.Select(c => c.ToString()).Order().ToArray();
            if (Utils.IsSuccess(socketResponse.ResponseCode))
            {
                var driveItems = new List<DriveItem>();
                for (var i = 0; i < result.Length; i++)
                {
                    var driveItem = new DriveItem($"{result[i]}:", result[i]);
                    driveItems.Add(driveItem);
                }
                return new CommandResponse<DriveItem[]>(ResponseCode.SUCCESS_OK, driveItems.ToArray());
            }
            return new CommandResponse<DriveItem[]>(socketResponse.ResponseCode, []);
        }
    }
}
