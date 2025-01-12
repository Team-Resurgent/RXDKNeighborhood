using RXDKXBDM.Models;

namespace RXDKXBDM.Commands
{
    public class Screenshot : Command
    {
        public static async Task<CommandResponse<ScreenshotItem>> SendAsync(Connection connection, CancellationToken cancellationToken, DownloadStream outputSteam)
        {
            var socketResponse = await SendCommandAndGetBinaryScreenshotResponseAsync(connection, "screenshot", cancellationToken, outputSteam);
            var commandResponse = new CommandResponse<ScreenshotItem>(socketResponse.ResponseCode, socketResponse.Screenshot);
            return commandResponse;
        }
    }
}
