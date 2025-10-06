namespace RXDKXBDM.Commands
{
    public class MkDir : Command
    {
        public static async Task<ResponseCode> SendAsync(Connection connection, string path)
        {
            var command = $"mkdir name=\"{path}\"";
            var socketResponse = await SendCommandAsync(connection, command);
            return socketResponse.ResponseCode;
        }
    }
}
