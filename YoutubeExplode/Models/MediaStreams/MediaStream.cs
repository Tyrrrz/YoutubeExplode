using System.IO;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Stream that contains raw media data.
    /// </summary>
    public class MediaStream : Stream
    {
        private readonly Stream _innerStream;

        /// <summary>
        /// Metadata associated with this stream.
        /// </summary>
        public MediaStreamInfo Info { get; }

        /// <inheritdoc />
        public override bool CanRead => _innerStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _innerStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => Info.Size;

        /// <inheritdoc />
        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        /// <summary />
        public MediaStream(MediaStreamInfo mediaStreamInfo, Stream innerStream)
        {
            Info = mediaStreamInfo.GuardNotNull(nameof(mediaStreamInfo));
            _innerStream = innerStream.GuardNotNull(nameof(innerStream));
        }

        /// <summary />
        ~MediaStream()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public override void Flush() => _innerStream.Flush();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => _innerStream.SetLength(value);

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

        /// <summary>
        /// Disposes resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _innerStream.Dispose();
        }
    }
}