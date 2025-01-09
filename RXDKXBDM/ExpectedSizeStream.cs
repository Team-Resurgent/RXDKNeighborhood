using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM
{
    public abstract class ExpectedSizeStream : Stream
    {
        public abstract long ExpectedSize { get; }
    }
}
