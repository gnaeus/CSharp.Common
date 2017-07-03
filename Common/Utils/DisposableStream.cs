using System;
using System.IO;

namespace Common.Utils
{
    /// <summary>
    /// Wrapper for <see cref="Stream"/> that dispose
    /// <paramref name="boundObject"/> when stream is disposed.
    /// </summary>
    public sealed class DisposableStream : Stream
    {
        private IDisposable BoundObject;
        private Stream WrappedStream;

        public DisposableStream(IDisposable boundObject, Stream wrappedStream)
        {
            BoundObject = boundObject;
            WrappedStream = wrappedStream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (BoundObject != null) {
                    BoundObject.Dispose();
                    BoundObject = null;
                }
                if (WrappedStream != null) {
                    WrappedStream.Dispose();
                    WrappedStream = null;
                }
            }
        }

        public override bool CanRead
        {
            get { return WrappedStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return WrappedStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return WrappedStream.CanWrite; }
        }

        public override long Length
        {
            get { return WrappedStream.Length; }
        }

        public override long Position
        {
            get { return WrappedStream.Position; }
            set { WrappedStream.Position = value; }
        }

        public override void Flush()
        {
            WrappedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return WrappedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return WrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            WrappedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WrappedStream.Write(buffer, offset, count);
        }
    }
}
