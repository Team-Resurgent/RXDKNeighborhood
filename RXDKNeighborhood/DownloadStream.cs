﻿using RXDKXBDM;

namespace RXDKNeighborhood
{
    public class DownloadStream : ExpectedSizeStream
    {
        private Stream mStream;
        private Action<long, long> mProgress;
        private long mExpectedSize;

        public override long ExpectedSize { get { return mExpectedSize; } }

        public override bool CanRead => mStream.CanRead;

        public override bool CanSeek => mStream.CanSeek;

        public override bool CanWrite => mStream.CanWrite;

        public override long Length => mStream.Length;

        public override long Position
        {
            get => mStream?.Position ?? 0;
            set =>  mStream.Position = value;
        }

        public DownloadStream(Stream stream, Action<long, long> progress)
        {
            mStream = stream;
            mExpectedSize = 0;
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
            if (Position == 0)
            {
                mExpectedSize = BitConverter.ToUInt32(buffer, offset);
                offset += 4;
                count -= 4;
            }
            mStream.Write(buffer, offset, count);
            mProgress.Invoke(mStream.Position, mExpectedSize);
        }
    }
}
