namespace RXDKXBDM.Commands
{
    public class Reboot : Command
    {
        public static async Task<ResponseCode> SendAsync(Connection connection, bool warm)
        {
            var command = "reboot";
            if (warm)
            {
                command += " warm";
            }
            var socketResponse = await SendCommandAsync(connection, command);
            return socketResponse.ResponseCode;
        }
    }
}
