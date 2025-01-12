namespace XBDMTest.Commands
{
    public static class RebootCommand
    {
        public static ResultCode Execute(RebootFlags flags, string xbeName)
        {
            var warn = (flags & RebootFlags.DMBOOT_WARM) != 0 ? " WARM" : string.Empty;

            var wait = string.Empty;
            if ((flags & RebootFlags.DMBOOT_STOP) != 0)
            {
                wait = " STOP";
            }
            else if ((flags & RebootFlags.DMBOOT_WAIT) != 0)
            {
                wait = " WAIT";
            }

            var command = string.Empty;
            if (string.IsNullOrEmpty(xbeName))
            {
                string debug = (flags & RebootFlags.DMBOOT_NODEBUG) != 0 ? " NODEBUG" : string.Empty;
                command = string.Format("REBOOT{0}{1}{2}", wait, warn, debug);
            }
            else
            {
                string debug = (flags & RebootFlags.DMBOOT_NODEBUG) != 0 ? string.Empty : " DEBUG";
                command = string.Format("magicboot title={0}{1}", xbeName, debug);
            }

            ResultCode hr = Protocol.HrDoOpenSharedConnection(Globals.GlobalSharedConnection, out var connection);
            if (Utils.IsSuccess(hr) && connection != null)
            {
                hr = Protocol.HrDoOneLineCmd(connection, command);
                Protocol.DoCloseSharedConnection(Globals.GlobalSharedConnection, connection);
            }

            return hr;
        }
    }
}
