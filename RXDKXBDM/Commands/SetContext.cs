using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class SetContext : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint thread, uint? ext)
        {
            var command = $"setcontext thread=0x{thread:x}";
            if (ext != null) 
            {
                command += $" ext=0x{ext:x}";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
