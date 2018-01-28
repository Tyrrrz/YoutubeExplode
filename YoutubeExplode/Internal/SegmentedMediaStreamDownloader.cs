#if NET45 || NETSTANDARD2_0 || NETCOREAPP1_0
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Services;

namespace YoutubeExplode.Internal
{
    internal class SegmentedMediaStreamDownloader
    {
        private readonly IHttpService _httpService;
        private readonly MediaStreamInfo _mediaStreamInfo;
        private readonly int _segmentCount;
        private readonly long _segmentSize;

        public SegmentedMediaStreamDownloader(IHttpService httpService, MediaStreamInfo mediaStreamInfo)
        {
            _httpService = httpService;
            _mediaStreamInfo = mediaStreamInfo;

            // Calculate stuff
            // TODO: there's probably a minimal size that YouTube serves without rate limiting, need to research
            _segmentCount = 10;
            _segmentSize = (long) Math.Ceiling(1.0 * _mediaStreamInfo.Size / _segmentCount);
        }

        private HttpRequestMessage CreateSegmentedRequest(long from, long to)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _mediaStreamInfo.Url);
            request.Headers.Range = new RangeHeaderValue(from, to);

            return request;
        }

        public async Task DownloadAsync(string filePath, IProgress<double> progress,
            CancellationToken cancellationToken)
        {
            using (var output = File.Create(filePath))
            {
                var totalBytesCopied = 0L;
                for (var i = 0; i < _segmentCount; i++)
                {
                    // Determine range
                    var from = i * _segmentSize;
                    var to = (i + 1) * _segmentSize - 1;

                    // Download
                    using (var request = CreateSegmentedRequest(from, to))
                    using (var response = await _httpService.PerformRequestAsync(request).ConfigureAwait(false))
                    using (var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        int bytesCopied;
                        do
                        {
                            // Copy
                            bytesCopied = await input.CopyChunkToAsync(output, cancellationToken).ConfigureAwait(false);

                            // Report progress
                            totalBytesCopied += bytesCopied;
                            progress?.Report(1.0 * totalBytesCopied / _mediaStreamInfo.Size);
                        } while (bytesCopied > 0);
                    }
                }
            }
        }
    }
}
#endif