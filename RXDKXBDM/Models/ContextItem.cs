namespace RXDKXBDM.Models
{
    public class ContextItem
    {
        public uint Ebp { get; set; }
        public uint Esp { get; set; }
        public uint Eip { get; set; }
        public uint EFlags { get; set; }
        public uint Eax { get; set; }
        public uint Ebx { get; set; }
        public uint Ecx { get; set; }
        public uint Edx { get; set; }
        public uint Edi { get; set; }
        public uint Esi { get; set; }
        public uint Cr0NpxState { get; set; }
    }
}
