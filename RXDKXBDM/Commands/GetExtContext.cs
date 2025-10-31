using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetExtContext : Command
    {
        public static async Task<CommandResponse<byte[]?>> SendAsync(Connection connection, uint thread)
        {
            using var memoryStream = new MemoryStream();
            using var downloadStream = new DownloadStream(memoryStream);

            var command = $"getextcontext thread=0x{thread:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, default, downloadStream);
            if (socketResponse.ResponseCode == ResponseCode.SUCCESS_OK)
            {
                return new CommandResponse<byte[]?>(socketResponse.ResponseCode, memoryStream.ToArray());
            }
            return new CommandResponse<byte[]?>(socketResponse.ResponseCode, null);
        }
    }
}
