using RXDKXBDM.Commands.Helpers;
using System;

namespace RXDKXBDM.Commands
{
    public class GetMem2 : Command
    {
        public static async Task<CommandResponse<byte[]?>> SendAsync(Connection connection, uint addr, uint length)
        {
            using var memoryStream = new MemoryStream();
            using var downloadStream = new DownloadStream(memoryStream);

            var command = $"getmem2 addr=0x{addr:x} length=0x{length:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseWithNoLengthAsync(connection, command, length, default, downloadStream);
            if (socketResponse.ResponseCode == ResponseCode.SUCCESS_OK)
            {
                return new CommandResponse<byte[]?>(socketResponse.ResponseCode, memoryStream.ToArray());
            }
            return new CommandResponse<byte[]?>(socketResponse.ResponseCode, null);
        }
    }
}
