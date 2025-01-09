using Microsoft.Maui.Storage;
using RXDKXBDM;
using System.Diagnostics;

namespace RXDKNeighborhood
{
    public class DownloadStream : ExpectedSizeStream
    {
        private string mFilename;
        private Stream? mStream;
        private long mExpectedSize;
        private Action<long, long>? mProgress;
        private bool mDisposed;
        private Stopwatch mLastUpdate;

        protected override void Dispose(bool disposing)
        {
            if (mDisposed)
            {
                return;
            }

            mStream?.Dispose();
       
            base.Dispose(disposing);
            mDisposed = true;
        }

        public override long ExpectedSize { get { return mExpectedSize; } }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => mStream?.Length ?? 0;

        public override long Position 
        { 
            get => mStream?.Position ?? 0;
            set
            {
                if (mStream != null)
                {
                    mStream.Position = value;
                }
            }
        }

        public DownloadStream(string filename, Action<long, long>? progress)
        {
            mFilename = filename;
            mStream = null;
            mExpectedSize = 0;
            mProgress = progress;
            mDisposed = false;
            mLastUpdate = Stopwatch.StartNew();
        }

        public override void Flush()
        {
            mStream?.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mStream == null)
            {
                return 0;
            }
            return mStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (mStream == null)
            {
                return 0;
            }
            return mStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            mStream?.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Position == 0)
            {
                mExpectedSize = BitConverter.ToUInt32(buffer, offset);
                offset += 4;
                count -= 4;
            }
            if (mStream == null)
            {
                mStream = new FileStream(mFilename, FileMode.Create);
            }
            mStream.Write(buffer, offset, count);

            //if (mLastUpdate.ElapsedMilliseconds < 1000)
            //{
            //    return;
            //}

            Debug.Print($"{mStream.Length} of {mExpectedSize}");
            //mProgress.Invoke(mStream.Length, mExpectedSize);
           // mLastUpdate.Restart();
        }
    }
}
