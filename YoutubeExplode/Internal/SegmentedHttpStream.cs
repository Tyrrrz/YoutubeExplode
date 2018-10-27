using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal
{
    internal class SegmentedHttpStream : Stream
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly long _segmentSize;

        private Stream _currentStream;
        private long _position;

        public SegmentedHttpStream(HttpClient httpClient, string url, long length, long segmentSize)
        {
            _url = url;
            _httpClient = httpClient;
            Length = length;
            _segmentSize = segmentSize;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position
        {
            get => _position;
            set
            {
                value.GuardNotNegative(nameof(value));

                if (_position == value) return;

                _position = value;
                ClearCurrentStream();
            }
        }

        private void ClearCurrentStream()
        {
            _currentStream?.Dispose();
            _currentStream = null;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // If full length has been exceeded - return 0
            if (Position >= Length)
                return 0;

            // If current stream is not set - resolve it
            if (_currentStream == null)
            {
                _currentStream = await _httpClient.GetStreamAsync(_url, Position, Position + _segmentSize - 1)
                    .ConfigureAwait(false);
            }

            // Read from current stream
            var bytesRead = await _currentStream.ReadAsync(buffer, offset, count, cancellationToken)
                .ConfigureAwait(false);

            // Advance the position (using field directly to avoid clearing stream)
            _position += bytesRead;

            // If no bytes have been read - resolve a new stream
            if (bytesRead == 0)
            {
                // Clear current stream
                ClearCurrentStream();

                // Recursively read again
                bytesRead = await ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }

            return bytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer, offset, count).ConfigureAwait(false).GetAwaiter().GetResult();

        private long GetNewPosition(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return offset;
                case SeekOrigin.Current:
                    return Position + offset;
                case SeekOrigin.End:
                    return Length + offset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // Get new position
            var newPosition = GetNewPosition(offset, origin);
            if (newPosition < 0)
                throw new IOException("An attempt was made to move the position before the beginning of the stream.");

            // Change position
            return Position = newPosition;
        }

        #region Not supported

        public override void Flush() => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                ClearCurrentStream();
        }
    }
}