namespace XBDMTest.Commands
{
    [Flags]
    public enum RebootFlags
    {
        DMBOOT_WAIT = 1,
        DMBOOT_WARM = 2,
        DMBOOT_NODEBUG = 4,
        DMBOOT_STOP = 8
    }
}
