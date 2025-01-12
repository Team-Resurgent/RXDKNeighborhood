using RXDKXBDM;

namespace RXDKNeighborhood
{
    public class DownloadStream : ExpectedSizeStream
    {
        private Stream mStream;
        private Action<long, long> mProgress;

        public override bool CanRead => mStream.CanRead;

        public override bool CanSeek => mStream.CanSeek;

        public override bool CanWrite => mStream.CanWrite;

        public override long Length => mStream.Length;

        public override long ExpectedSize { get; set; }

        public override long Position
        {
            get => mStream.Position;
            set => mStream.Position = value;
        }

        public DownloadStream(Stream stream, Action<long, long> progress)
        {
            mStream = stream;
            mProgress = progress;
        }

        public override void Flush()
        {
            mStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return mStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return mStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            mStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            mStream.Write(buffer, offset, count);
            mProgress.Invoke(mStream.Position, ExpectedSize);
        }
    }
}
