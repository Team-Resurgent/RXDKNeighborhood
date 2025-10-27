using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public enum DefTitleType
    {
        None,
        Launcher
    }

    public class DefTitle : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, DefTitleType defTitleType)
        {
            var command = "deftitle";
            if (defTitleType == DefTitleType.None)
            {
                command += " none";
            }
            else if (defTitleType == DefTitleType.Launcher)
            {
                command += $" launcher";
            }
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }

        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string dir, string name)
        {
            var command = $"deftitle dir=\"{dir}\" name=\"{name}\"";
            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
