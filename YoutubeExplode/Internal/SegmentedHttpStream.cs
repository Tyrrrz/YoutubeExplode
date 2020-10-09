using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.ReverseEngineering;

namespace YoutubeExplode.Internal
{
    internal class SegmentedHttpStream : Stream
    {
        private readonly YoutubeHttpClient _httpClient;
        private readonly string _url;
        private readonly long? _segmentSize;

        private Stream? _currentStream;
        private long _position;

        public SegmentedHttpStream(YoutubeHttpClient httpClient, string url, long length, long? segmentSize)
        {
            _url = url;
            _httpClient = httpClient;
            Length = length;
            _segmentSize = segmentSize;
        }

        [ExcludeFromCodeCoverage]
        public override bool CanRead => true;

        [ExcludeFromCodeCoverage]
        public override bool CanSeek => true;

        [ExcludeFromCodeCoverage]
        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0)
                    throw new IOException("An attempt was made to move the position before the beginning of the stream.");

                if (_position == value)
                    return;

                _position = value;
                ClearCurrentStream();
            }
        }

        private void ClearCurrentStream()
        {
            _currentStream?.Dispose();
            _currentStream = null;
        }

        private long GetNewPosition(long offset, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // If full length has been exceeded - return 0
            if (Position >= Length)
                return 0;

            // If current stream is not set - resolve it
            if (_currentStream == null)
            {
                if (_segmentSize == null)
                {
                    // The current is not RateLimited - _segmentSize will be null
                    _currentStream = await _httpClient.GetStreamAsync(_url, Position);
                }
                else
                {
                    // The current is RateLimited - use _segmentSize with Position to set 'to' stream position
                    _currentStream = await _httpClient.GetStreamAsync(_url, Position, Position + _segmentSize - 1);
                }
            }

            // Read from current stream
            var bytesRead = await _currentStream.ReadAsync(buffer, offset, count, cancellationToken);

            // Advance the position (using field directly to avoid clearing stream)
            _position += bytesRead;

            // If no bytes have been read - resolve a new stream
            if (bytesRead == 0)
            {
                // Clear current stream
                ClearCurrentStream();

                // Recursively read again
                bytesRead = await ReadAsync(buffer, offset, count, cancellationToken);
            }

            return bytesRead;
        }

        [ExcludeFromCodeCoverage]
        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        [ExcludeFromCodeCoverage]
        public override long Seek(long offset, SeekOrigin origin) => Position = GetNewPosition(offset, origin);

        [ExcludeFromCodeCoverage]
        public override void Flush() => throw new NotSupportedException();

        [ExcludeFromCodeCoverage]
        public override void SetLength(long value) => throw new NotSupportedException();

        [ExcludeFromCodeCoverage]
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                ClearCurrentStream();
        }
    }
}