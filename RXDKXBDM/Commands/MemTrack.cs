using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class MemTrack : Command
    {
        public static async Task<CommandResponse<string>> SendEnableAsync(Connection connection, uint stackdepth, uint flags)
        {
            var command = $"memtrack cmd=\"emable\" stackdepth=0x{stackdepth:x} flags=0x{flags}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendEnableOnceAsync(Connection connection, uint stackdepth, uint flags)
        {
            var command = $"memtrack cmd=\"emableonce\" stackdepth=0x{stackdepth:x} flags=0x{flags}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendDisableAsync(Connection connection)
        {
            var command = $"memtrack cmd=\"disable\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendSaveAsync(Connection connection, string filename)
        {
            var command = $"memtrack cmd=\"save\" filename=\"{filename}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendQueryStackDepthAsync(Connection connection)
        {
            var command = $"memtrack cmd=\"querystackdepth\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendQueryTypeAsync(Connection connection, uint type)
        {
            var command = $"memtrack cmd=\"querytype\" type=0x{type:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendQueryFlagsAsync(Connection connection)
        {
            var command = $"memtrack cmd=\"queryflags\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
