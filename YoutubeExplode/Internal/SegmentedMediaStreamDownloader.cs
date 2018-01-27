#if NET45 || NETSTANDARD2_0 || NETCOREAPP1_0
using System.Collections.Generic;
using System.IO;
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

        public SegmentedMediaStreamDownloader(IHttpService httpService, MediaStreamInfo mediaStreamInfo)
        {
            _httpService = httpService;
            _mediaStreamInfo = mediaStreamInfo;
        }

        public async Task DownloadAsync(string filePath)
        {
            var segmentCount = 10;
            var segmentSize = _mediaStreamInfo.Size / segmentCount;
            var tasks = new List<Task>();
            for (var i = 0; i < segmentCount; i++)
            {
                var id = i;
                var task = Task.Run(async () =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, _mediaStreamInfo.Url))
                    {
                        request.Headers.Range = new RangeHeaderValue(id * segmentSize, (id + 1) * segmentSize - 1);
                        using (var response = await _httpService.PerformRequestAsync(request).ConfigureAwait(false))
                        {
                            var partFilePath = filePath + $"_p{id}";
                            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            using (stream)
                            using (var fs = File.Create(partFilePath))
                                await stream.CopyToAsync(fs).ConfigureAwait(false);
                        }
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            using (var fs = File.Create(filePath))
            {
                for (var i = 0; i < segmentCount; i++)
                {
                    using (var asd = File.OpenRead(filePath + $"_p{i}"))
                        await asd.CopyToAsync(fs).ConfigureAwait(false);
                    File.Delete(filePath + $"_p{i}");
                }
            }
        }
    }
}
#endif