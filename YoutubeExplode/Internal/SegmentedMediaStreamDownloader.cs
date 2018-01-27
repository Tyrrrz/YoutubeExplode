#if NET45 || NETSTANDARD2_0 || NETCOREAPP1_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private void UnlockConnectionLimit()
        {
#if NET45 || NETSTANDARD2_0
            // This only works on .net45 and .netstd20
            // On other frameworks the download will be slower
            ServicePointManager.FindServicePoint(new Uri(_mediaStreamInfo.Url)).ConnectionLimit = 999;
#endif
        }

        private HttpRequestMessage CreateSegmentedRequest(long from, long to)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _mediaStreamInfo.Url);
            request.Headers.Range = new RangeHeaderValue(from, to);

            return request;
        }

        private string GetSegmentFilePath(string filePath, int segmentIndex)
        {
            return $"{filePath}.{segmentIndex}";
        }

        public async Task DownloadAsync(string filePath)
        {
            // TODO: missing progress reporting and cancellation

            // Increase connection limit so multiple requests can be performed simultaneously
            UnlockConnectionLimit();

            // Create tasks that download segments
            var tasks = new List<Task>();
            for (var i = 0; i < _segmentCount; i++)
            {
                // Assemble segment info
                var segmentFilePath = GetSegmentFilePath(filePath, i);
                var from = i * _segmentSize;
                var to = (i + 1) * _segmentSize - 1;

                // Run a task to download segment
                var task = Task.Run(async () =>
                {
                    using (var request = CreateSegmentedRequest(from, to))
                    using (var response = await _httpService.PerformRequestAsync(request).ConfigureAwait(false))
                    using (var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var output = File.Create(segmentFilePath))
                        await input.CopyToAsync(output).ConfigureAwait(false);
                });
                tasks.Add(task);
            }

            // Await until all segments are downloaded
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Combine all segments into one file
            using (var output = File.Create(filePath))
            {
                for (var i = 0; i < _segmentCount; i++)
                {
                    var segmentFilePath = GetSegmentFilePath(filePath, i);

                    // Append data to output file
                    using (var input = File.OpenRead(segmentFilePath))
                        await input.CopyToAsync(output).ConfigureAwait(false);

                    // Delete segment
                    File.Delete(segmentFilePath);
                }
            }
        }
    }
}
#endif