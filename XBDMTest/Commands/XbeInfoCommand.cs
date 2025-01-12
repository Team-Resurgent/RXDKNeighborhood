using System.Text;
using System.Xml.Linq;

namespace XBDMTest.Commands
{


    public static class XbeInfoCommand
    {
        public static ResultCode Execute(string name, ref XbeInfo xbeInfo)
        {
            ResultCode hr;

            hr = Protocol.HrDoOpenSharedConnection(Globals.GlobalSharedConnection, out var connection);
            if (Utils.IsSuccess(hr) == false || connection == null)
            {
                return hr;
            }

            var command = string.IsNullOrEmpty(name) == false ? $"XBEINFO NAME=\"{name}\"" : "XBEINFO RUNNING";

            hr = Protocol.DmSendCommand(null, command, out _);
            if (hr != ResultCode.SUCCESS_MULTIRESPONSE)
            {
                if (Utils.IsSuccess(hr))
                {
                    hr = ResultCode.ERROR_UNEXPECTED;
                }
                Protocol.DoCloseSharedConnection(Globals.GlobalSharedConnection, connection);
                return hr;
            }

            while (true)
            {
                hr = Protocol.DmReceiveSocketLine(connection, out var response);
                if (Utils.IsSuccess(hr) == false || response == ".")
                {
                    break;
                }

                Utils.FGetSzParam(response, "name", out var launchPath);
                //Utils.FGetDwParam(response, "timestamp", out var timeStamp);
                //Utils.FGetDwParam(response, "checksum", out var checkSum);

                xbeInfo.LaunchPath = launchPath;
                //xbeInfo.TimeStamp = timeStamp;
                //xbeInfo.CheckSum = checkSum;
            }

            Protocol.DoCloseSharedConnection(Globals.GlobalSharedConnection, connection);
            return ResultCode.SUCCESS_OK;
        }
    }
}
