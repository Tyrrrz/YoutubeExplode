using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Converter.Tests.Utils;

internal class RateLimitedHttpHandler(TimeSpan requestInterval) : HttpClientHandler
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTimeOffset _lastRequestTimestamp = DateTimeOffset.MinValue;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            // Wait until the request interval has passed since the last request
            var timeToWait = requestInterval - (DateTimeOffset.Now - _lastRequestTimestamp);
            if (timeToWait > TimeSpan.Zero)
                await Task.Delay(timeToWait, cancellationToken);

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            // Update the last request timestamp
            _lastRequestTimestamp = DateTimeOffset.Now;

            return response;
        }
        finally
        {
            _lock.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _lock.Dispose();
    }
}
