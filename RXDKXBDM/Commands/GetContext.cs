using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class GetContext : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, uint thread, bool control, bool integer, bool full, bool floatingpoint)
        {
            var command = $"getcontext thread=0x{thread:x}";
            if (control) 
            {
                command += " control";
            }
            if (integer)
            {
                command += " int";
            }
            if (full)
            {
                command += " full";
            }
            if (floatingpoint)
            {
                command += " fp";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
