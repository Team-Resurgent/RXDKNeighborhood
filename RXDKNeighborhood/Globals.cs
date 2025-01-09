using RXDKXBDM;

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
