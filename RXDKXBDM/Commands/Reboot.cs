namespace RXDKXBDM.Commands
{
    public enum WaitType
    {
        None,
        Stop,
        Wait
    }

    public class Reboot : Command
    {
        public static async Task<ResponseCode> SendAsync(Connection connection, bool warm, bool noDebug, WaitType waitType)
        {
            var command = "reboot";
            if (warm)
            {
                command += " warm";
            }
            if (noDebug)
            {
                command += " nodebug";
            }
            if (waitType == WaitType.None)
            {
                if (waitType == WaitType.Stop)
                {
                    command += " stop";
                }
                else if (waitType == WaitType.Wait)
                {
                    command += " wait";
                }
            }
            var socketResponse = await SendCommandAsync(connection, command);
            return socketResponse.ResponseCode;
        }
    }
}
