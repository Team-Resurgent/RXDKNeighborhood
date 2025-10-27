using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class SysFileUpd : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string name, uint size, uint crc, CancellationToken cancellationToken, ExpectedSizeStream expectedSizeStream)
        {
            var command = $"sysfileupd name=\"{name}\" size=0x{size:x} crc=0x{crc:x}";
            var socketResponse = await SendCommandAndGetBinaryResponseAsync(connection, command, cancellationToken, expectedSizeStream);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
