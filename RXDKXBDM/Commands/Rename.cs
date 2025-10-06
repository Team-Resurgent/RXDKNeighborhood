namespace RXDKXBDM.Commands
{
    public class Rename : Command
    {
        public static async Task<ResponseCode> SendAsync(Connection connection, string path, string newName)
        {
            var command = $"rename name=\"{path}\" newname=\"{newName}\"";
            var socketResponse = await SendCommandAsync(connection, command);
            return socketResponse.ResponseCode;
        }
    }
}
