using RXDKXBDM.Commands.Helpers;
using RXDKXBDM.Models;

namespace RXDKXBDM.Commands
{
    public class GetContext : Command
    {
        public static async Task<CommandResponse<ContextItem?>> SendAsync(Connection connection, uint thread, bool control, bool integer, bool full, bool floatingpoint)
        {
            var command = $"getcontext thread=0x{thread:x}";
            if (control) 
            {
                command += " control";
            }
            if (integer)
            {
                command += " int";
            }
            if (full)
            {
                command += " full";
            }
            if (floatingpoint)
            {
                command += " fp";
            }
            var socketResponse = await SendCommandAndGetMultilineResponseAsync(connection, command);
            if (Utils.IsSuccess(socketResponse.ResponseCode))
            {
                var context = Utils.BodyToDictionary(socketResponse.Body);
                var contextItem = new ContextItem
                {
                    Ebp = Utils.GetDictionaryIntFromKey(context, "Ebp"),
                    Esp = Utils.GetDictionaryIntFromKey(context, "Esp"),
                    Eip = Utils.GetDictionaryIntFromKey(context, "Eip"),
                    EFlags = Utils.GetDictionaryIntFromKey(context, "EFlags"),
                    Eax = Utils.GetDictionaryIntFromKey(context, "Eax"),
                    Ebx = Utils.GetDictionaryIntFromKey(context, "Ebx"),
                    Ecx = Utils.GetDictionaryIntFromKey(context, "Ecx"),
                    Edx = Utils.GetDictionaryIntFromKey(context, "Edx"),
                    Edi = Utils.GetDictionaryIntFromKey(context, "Edi"),
                    Esi = Utils.GetDictionaryIntFromKey(context, "Esi"),
                    Cr0NpxState = Utils.GetDictionaryIntFromKey(context, "Cr0NpxState"),
                };
                return new CommandResponse<ContextItem?>(ResponseCode.SUCCESS_OK, contextItem);
            }
            return new CommandResponse<ContextItem?>(socketResponse.ResponseCode, null);
        }
    }
}
