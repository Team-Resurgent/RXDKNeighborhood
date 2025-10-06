namespace RXDKXBDM.Commands
{
    public class Delete : Command
    {
        public static async Task<ResponseCode> SendAsync(Connection connection, string path, bool isDir)
        {
            var command = $"delete name=\"{path}\"";
            if (isDir)
            {
                command += " dir";
            }
            var socketResponse = await SendCommandAsync(connection, command);
            return socketResponse.ResponseCode;
        }
    }
}
