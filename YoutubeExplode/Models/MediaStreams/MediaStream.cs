using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Stream that contains raw media data.
    /// </summary>
    public class MediaStream : Stream
    {
        private readonly Stream _stream;

        /// <summary>
        /// Metadata associated with this stream.
        /// </summary>
        [NotNull]
        public MediaStreamInfo Info { get; }

        /// <inheritdoc />
        public override bool CanRead => _stream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _stream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _stream.CanWrite;

        /// <inheritdoc />
        public override long Length => Info.Size;

        /// <inheritdoc />
        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        /// <summary>
        /// Initializes an instance of <see cref="MediaStream"/>.
        /// </summary>
        public MediaStream(MediaStreamInfo info, Stream stream)
        {
            Info = info.GuardNotNull(nameof(info));
            _stream = stream.GuardNotNull(nameof(stream));
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        /// <inheritdoc />
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        /// <inheritdoc />
        public override void Flush() => _stream.Flush();

        /// <inheritdoc />
        public override void SetLength(long value) => _stream.SetLength(value);

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        /// <summary>
        /// Disposes resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _stream.Dispose();
        }
    }
}