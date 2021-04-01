using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Utils
{
    // Special abstraction that works around YouTube's stream throttling
    // and provides seeking support.
    internal class SegmentedHttpStream : Stream
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly long? _segmentSize;

        private Stream? _currentStream;
        private long _position;

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

                ResetCurrentStream();
            }
        }

        public SegmentedHttpStream(HttpClient httpClient, string url, long length, long? segmentSize)
        {
            _url = url;
            _httpClient = httpClient;
            Length = length;
            _segmentSize = segmentSize;
        }

        private void ResetCurrentStream()
        {
            _currentStream?.Dispose();
            _currentStream = null;
        }

        private async ValueTask<Stream> ResolveCurrentStreamAsync()
        {
            if (_currentStream is not null)
                return _currentStream;

            var from = Position;

            var to = _segmentSize is not null
                ? Position + _segmentSize - 1
                : null;

            var stream = await _httpClient.GetStreamAsync(_url, from, to);

            return _currentStream = stream;
        }

        public async ValueTask PrepareAsync() => await ResolveCurrentStreamAsync();

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            if (Position >= Length)
                return 0;

            var stream = await ResolveCurrentStreamAsync();

            var bytesRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
            _position += bytesRead;

            // Stream reached the end of the segment - reset and read again
            if (bytesRead == 0)
            {
                ResetCurrentStream();
                return await ReadAsync(buffer, offset, count, cancellationToken);
            }

            return bytesRead;
        }

        [ExcludeFromCodeCoverage]
        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        [ExcludeFromCodeCoverage]
        public override long Seek(long offset, SeekOrigin origin) => Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        [ExcludeFromCodeCoverage]
        public override void Flush() =>
            throw new NotSupportedException();

        [ExcludeFromCodeCoverage]
        public override void SetLength(long value) =>
            throw new NotSupportedException();

        [ExcludeFromCodeCoverage]
        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ResetCurrentStream();
            }
        }
    }
}