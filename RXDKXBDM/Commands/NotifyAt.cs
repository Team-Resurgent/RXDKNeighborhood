using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum NotifyAtType
    {
        None,
        Drop,
        Debug
    }

    public class NotifyAt : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, int port, string? address, NotifyAtType notifyAtType)
        {
            var command = $"notifyat port={port}";
            if (address != null)
            {
                command += $" address={address}";
            }
            if (notifyAtType != NotifyAtType.None)
            {
                if (notifyAtType == NotifyAtType.Drop)
                {
                    command += " drop";
                }
                else if (notifyAtType == NotifyAtType.Debug)
                {
                    command += " debug";
                }
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
