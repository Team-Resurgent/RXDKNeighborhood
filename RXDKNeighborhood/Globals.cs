using RXDKXBDM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKNeighborhood
{
    public static class Globals
    {
        private static Connection mConnection = new Connection();
        public static Connection GlobalConnection
        {
            get
            {
                return mConnection;
            }
        }
    }
}
