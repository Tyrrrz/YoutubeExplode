using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.Streams;

// Works around YouTube's rate throttling, provides seeking support and some resiliency
internal class PlaybackStream : Stream
{
    private readonly HttpClient _http;
    private readonly string _url;
    private readonly long? _segmentSize;

    private Stream? _segmentStream;
    private long _actualPosition;

    [ExcludeFromCodeCoverage]
    public override bool CanRead => true;

    [ExcludeFromCodeCoverage]
    public override bool CanSeek => true;

    [ExcludeFromCodeCoverage]
    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position { get; set; }

    public PlaybackStream(HttpClient http, string url, long length, long? segmentSize)
    {
        _url = url;
        _http = http;
        Length = length;
        _segmentSize = segmentSize;
    }

    private void ResetSegment()
    {
        _segmentStream?.Dispose();
        _segmentStream = null;
    }

    private async ValueTask<Stream> ResolveSegmentAsync(CancellationToken cancellationToken = default)
    {
        if (_segmentStream is not null)
            return _segmentStream;

        var from = Position;

        var to = _segmentSize is not null
            ? Position + _segmentSize - 1
            : null;

        // YouTube sometimes return 5XX errors, so we need to handle that
        var stream = await Retry.WhileExceptionAsync(
            async innerCancellationToken => await _http.GetStreamAsync(_url, from, to, true, innerCancellationToken),
            ex =>
                ex is HttpRequestException hrex &&
                hrex.TryGetStatusCode() is { } status &&
                (int) status >= 500,
            5,
            cancellationToken
        );

        return _segmentStream = stream;
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default) =>
        await ResolveSegmentAsync(cancellationToken);

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        while (true)
        {
            // Check if consumer changed position between reads
            if (_actualPosition != Position)
                ResetSegment();

            // Check if finished reading (exit condition)
            if (Position >= Length)
                return 0;

            var stream = await ResolveSegmentAsync(cancellationToken);
            var bytesRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
            _actualPosition = Position += bytesRead;

            if (bytesRead != 0)
                return bytesRead;

            // Reached the end of the segment, try to load the next one
            ResetSegment();
        }
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
            ResetSegment();
    }
}