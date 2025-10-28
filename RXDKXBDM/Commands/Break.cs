using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum BreakType
    {
        Addr,
        Read,
        Write,
        Size
    }

    public class Break : Command
    {
        public static async Task<CommandResponse<string>> SendNowAsync(Connection connection)
        {
            var command = $"break now";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendStartAsync(Connection connection)
        {
            var command = $"break start";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendClearAllAsync(Connection connection)
        {
            var command = $"break clearall";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendRemoveAsync(Connection connection, uint addr)
        {
            var command = $"break clear addr=0x{addr:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAddAsync(Connection connection, uint addr)
        {
            var command = $"break addr=0x{addr:x}";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}


//if (FGetDwParam(sz, "read", (DWORD*)&pvAddr))
//    dwType = DMBREAK_READWRITE;
//else if (FGetDwParam(sz, "write", (DWORD*)&pvAddr))
//    dwType = DMBREAK_WRITE;
//else if (FGetDwParam(sz, "execute", (DWORD*)&pvAddr))
//    dwType = DMBREAK_EXECUTE;
//if (dwType == DMBREAK_NONE)
//    /* Never saw a valid command */
//    hr = E_FAIL;
//else if (fClear || FGetNamedDwParam(sz, "size", &dwSize, szResp))
//{
//    szResp[0] = 0;
//    hr = DmSetDataBreakpoint(pvAddr, fClear ? DMBREAK_NONE : dwType,
//        dwSize);