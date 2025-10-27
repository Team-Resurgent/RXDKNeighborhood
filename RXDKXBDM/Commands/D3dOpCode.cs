using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class D3dOpCode : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint p0, uint p1, uint p2, uint p3, uint p4, uint p5)
        {
            var command = $"d3dopcode p0=0x{p0:x} p1=0x{p1:x} p2=0x{p2:x} p3=0x{p3:x} p4=0x{p4:x} p5=0x{p5:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
