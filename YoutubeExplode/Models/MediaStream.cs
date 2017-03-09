using System;
using System.IO;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Media stream
    /// </summary>
    public class MediaStream : Stream
    {
        private readonly Stream _innerStream;

        /// <summary>
        /// Meta data associated with this media stream
        /// </summary>
        public MediaStreamInfo Info { get; }

        /// <inheritdoc />
        public override bool CanRead => _innerStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _innerStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _innerStream.CanWrite;

        /// <inheritdoc />
        public override long Length => Info.FileSize;

        /// <inheritdoc />
        public override long Position
        {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        internal MediaStream(Stream innerStream, MediaStreamInfo streamInfo)
        {
            if (innerStream == null)
                throw new ArgumentNullException(nameof(innerStream));
            if (streamInfo == null)
                throw new ArgumentNullException(nameof(streamInfo));

            _innerStream = innerStream;
            Info = streamInfo;
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _innerStream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _innerStream.Dispose();
        }
    }
}