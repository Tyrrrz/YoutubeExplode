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
        private const long MaxSegmentSize = 9_898_989;
        private readonly string _url;
        private readonly long _size;
        public YoutubeVideoStream(HttpClient httpClient, string url, long size)
        {
            _url = url;
            _httpClient = httpClient;
            _size = size;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _size;

        public override long Position { get; set; }

        public override void Flush() => throw new System.NotSupportedException();
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count > MaxSegmentSize - 1)
                count = (int)MaxSegmentSize - 1;
            using (var currentStream = await _httpClient.GetStreamAsync(_url, Position, Position + count).ConfigureAwait(false))
            {
                var bytesRead = await currentStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                Position += bytesRead;
                return bytesRead;
            }
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
            return Position;
        }

        public override void SetLength(long value) => throw new System.NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new System.NotSupportedException();
    }
}