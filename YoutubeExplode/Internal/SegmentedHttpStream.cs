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

        // Maximum segment size for rate limited streams
        private const int MaxSegmentSize = 9_898_989; // this number was carefully devised through research

        private readonly string _url;

        Stream _currentStream;

        private long _position;
        private readonly int _maxSegmentSize;

        public SegmentedHttpStream(HttpClient httpClient, string url, long length, int maxSegmentSize = MaxSegmentSize)
        {
            _url = url;
            _httpClient = httpClient;
            Length = length;
            _maxSegmentSize = maxSegmentSize;
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
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Non-negative number required");
                }
                if (_position != value)
                {
                    _position = value;
                    ClearCurrentStream();
                }
            }
        }

        private void ClearCurrentStream()
        {
            _currentStream?.Dispose();
            _currentStream = null;
        }

        public override void Flush() => throw new System.NotSupportedException();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (Position >= Length)
                return 0;

            if (_currentStream is null)
                _currentStream = await _httpClient.GetStreamAsync(_url, Position, Position + _maxSegmentSize - 1).ConfigureAwait(false);

            var bytesRead = await _currentStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            Position += bytesRead;
            if (bytesRead == 0)
            {
                ClearCurrentStream();
                bytesRead = await ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
            return bytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

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
                    throw new ArgumentException(nameof(origin), "Invalid SeekOrigin");
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPosition = GetNewPosition(offset, origin);
            if (newPosition < 0)
                throw new IOException("An attempt was made to move the position before the beginning of the stream.");

            if (Position == newPosition)
                return Position;

            Position = newPosition;
            ClearCurrentStream();
            return Position;
        }

        public override void SetLength(long value) => throw new System.NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new System.NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                ClearCurrentStream();
        }
    }
}