namespace RXDKXBDM.Commands
{
    public class Threads : Command
    {
        public static async Task<CommandResponse<uint[]>> SendAsync(Connection connection)
        {
            var command = $"threads";
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            if (Utils.IsSuccess(socketResponse.ResponseCode))
            {
                var threads = new List<uint>();
                for (int i = 0; i < socketResponse.Body.Length; i++)
                {
                    var line = socketResponse.Body[i];
                    if (uint.TryParse(line, out var threadId))
                    {
                        threads.Add(threadId);
                    }
                }
                return new CommandResponse<uint[]>(ResponseCode.SUCCESS_OK, threads.ToArray());
            }
            return new CommandResponse<uint[]>(socketResponse.ResponseCode, []);
        }
    }
}
