using RXDKXBDM.Commands.Helpers;

namespace RXDKXBDM.Commands
{
    public class SetFileAttributes : Command
    {
        public static async Task<CommandResponse<string>> SendAsync(Connection connection, string path, DateTime created, DateTime changed, bool hidden, bool readOnly)
        {
            var createdValues = Utils.DateTimeToDictionary(created);
            var changedValues = Utils.DateTimeToDictionary(changed);
            var command = $"setfileattributes name=\"{path}\" createhi={createdValues["hi"]} createlo={createdValues["lo"]} changehi={createdValues["hi"]} changelo={createdValues["lo"]}";
            command += hidden ? " hidden=1" : " hidden=0";
            command += readOnly ? " readonly=1" : " readonly=0";

            var socketResponse = await SendCommandAndGetResponseAsync(connection, command);
            var commandResponse = new CommandResponse<string>(socketResponse.ResponseCode, socketResponse.Response);
            return commandResponse;
        }
    }
}
