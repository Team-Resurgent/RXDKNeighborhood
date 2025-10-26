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
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool now, bool start, bool clearall, bool clear)
        {
            var command = $"break";
            if (now)
            {
                command += $" now";
            }
            if (start)
            {
                command += $" start";
            }
            if (clearall)
            {
                command += $" clearall";
            }
            if (clear)
            {
                command += $" clear";
            }

            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, bool now, bool start, bool clearall, bool clear, BreakType breakType, uint breakValue)
        {
            var command = $"break";
            if (now)
            {
                command += $" now";
            }
            if (start)
            {
                command += $" start";
            }
            if (clearall)
            {
                command += $" clearall";
            }
            if (clear)
            {
                command += $" clear";
            }

            if (breakType == BreakType.Addr)
            {
                command += $" addr=0x{breakValue:x2}";
            }
            else if (breakType == BreakType.Read)
            {
                command += $" read=0x{breakValue:x2}";
            }
            else if (breakType == BreakType.Write)
            {
                command += $" write=0x{breakValue:x2}";
            }
            else if (breakType == BreakType.Size)
            {
                command += $" size=0x{breakValue:x2}";
            }

            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
