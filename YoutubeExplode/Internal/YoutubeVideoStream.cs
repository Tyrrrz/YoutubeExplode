using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal
{
    internal class YoutubeVideoStream : Stream
    {
        private readonly HttpClient _httpClient;

        private const int MaxSegmentSize = 9_898_989;

        private readonly string _url;

        Stream _currentStream;

        public YoutubeVideoStream(HttpClient httpClient, string url, long length)
        {
            _url = url;
            _httpClient = httpClient;
            Length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position { get; set; }

        public override void Flush() => throw new System.NotSupportedException();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (Position >= Length)
                return 0;

            count = Math.Min(MaxSegmentSize - 1, count);
            if(_currentStream == null)
                _currentStream = await _httpClient.GetStreamAsync(_url, Position, Position + MaxSegmentSize).ConfigureAwait(false);
            
            var bytesRead = await _currentStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(true);
            Position += bytesRead;
            if (bytesRead == 0)
            {
                _currentStream.Dispose();
                _currentStream = null;
                bytesRead = await ReadAsync(buffer, offset, count, cancellationToken);
            }
            return bytesRead;
        }
        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = Math.Max(0, offset);
                    break;
                case SeekOrigin.Current:
                    Position = Math.Max(0, Math.Min(Length, Position + offset));
                    break;
                case SeekOrigin.End:
                    Position = Math.Max(0, Math.Min(Length, Length - offset));
                    break;
            }
            _currentStream.Dispose();
            _currentStream = null;
            return Position;
        }

        public override void SetLength(long value) => throw new System.NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new System.NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _currentStream?.Dispose();
        }
    }
}