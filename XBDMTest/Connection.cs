using System.Net.Sockets;
using System.Threading.Channels;

namespace XBDMTest
{
    public class Connection : IDisposable
    {
        private Socket? mSocket;
        private bool mDisposed;
        private readonly byte[] mRawBuffer;

        public byte[] RawBuffer => mRawBuffer;

        public Socket? Socket => mSocket;

        public bool FAuthenticated { get; set; }

        public bool FAuthenticationAttempted { get; set; }

        public int IndexBiffer { get; set; }

        public int CurrentBufferSize { get; set; }

        //public SharedConnectionInfo SharedConnectionInfo { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed)
            {
                return;
            }
            if (disposing)
            {
                Close();
            }
            mDisposed = true;
        }

        public void Close()
        {
            if (mSocket != null)
            {
                try
                {
                    mSocket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    mSocket.Close();
                    mSocket.Dispose();
                    mSocket = null;
                }
            }
        }

        public Connection()
        {
            SharedConnectionInfo = new SharedConnectionInfo();
            mRawBuffer = new byte[1024];
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mDisposed = false;
        }
    }
}
