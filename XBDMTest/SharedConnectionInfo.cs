using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XBDMTest
{
    public class SharedConnectionInfo
    {
        public string SzXboxName { get; set; } = string.Empty;
        public IPAddress? UlXboxIPAddr { get; set; }
        public uint DwConnectionTimeout { get; set; }
        public uint DwConversationTimeout { get; set; }
        public object SharedConnectionLock { get; } = new object(); 
        public Connection? SharedConnection { get; set; }
        public int TidShared { get; set; }

        // Flags represented as properties
        public bool FAllowSharing { get; set; }
        public bool FBadSysTime { get; set; }
        public bool FGotTimeCorrection { get; set; }
        public bool FAddDiff { get; set; }
        public bool FCacheAddr { get; set; }
        public bool FSecureConnection { get; set; }

        public ulong LiTimeDiff { get; set; } // Equivalent to ULARGE_INTEGER

        public uint DwFlags
        {
            get
            {
                uint flags = 0;
                flags |= FAllowSharing ? (1u << 0) : 0;
                flags |= FBadSysTime ? (1u << 1) : 0;
                flags |= FGotTimeCorrection ? (1u << 2) : 0;
                flags |= FAddDiff ? (1u << 3) : 0;
                flags |= FCacheAddr ? (1u << 4) : 0;
                flags |= FSecureConnection ? (1u << 5) : 0;
                return flags;
            }
            set
            {
                FAllowSharing = (value & (1u << 0)) != 0;
                FBadSysTime = (value & (1u << 1)) != 0;
                FGotTimeCorrection = (value & (1u << 2)) != 0;
                FAddDiff = (value & (1u << 3)) != 0;
                FCacheAddr = (value & (1u << 4)) != 0;
                FSecureConnection = (value & (1u << 5)) != 0;
            }
        }
    }
}
