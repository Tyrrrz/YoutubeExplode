using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Services;

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0
using System;
using System.IO;
using System.Threading;
#endif

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <summary>
        /// Gets the actual media stream associated with given metadata.
        /// </summary>
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo info)
        {
            info.GuardNotNull(nameof(info));

            // Get stream
            var stream = await _httpService.GetStreamAsync(info.Url).ConfigureAwait(false);

            return new MediaStream(info, stream);
        }

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0

        /// <summary>
        /// Gets the actual media stream associated with given metadata
        /// and downloads it to a file.
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            info.GuardNotNull(nameof(info));
            filePath.GuardNotNull(nameof(filePath));

            // Keep track of bytes copied for progress reporting
            var totalBytesCopied = 0L;

            // Determine if stream is rate-limited
            var isRateLimited = !Regex.IsMatch(info.Url, @"ratebypass[=/]yes");

            // Download rate-limited streams in segments
            if (isRateLimited)
            {
                // Determine segment size
                const int segmentCount = 10;
                var segmentSize = (long) Math.Ceiling(1.0 * info.Size / segmentCount);

                using (var output = File.Create(filePath))
                {
                    for (var i = 0; i < segmentCount; i++)
                    {
                        // Determine segment range
                        var from = i * segmentSize;
                        var to = (i + 1) * segmentSize - 1;

                        // Create request with range
                        var request = new HttpRequestMessage(HttpMethod.Get, info.Url);
                        request.Headers.Range = new RangeHeaderValue(from, to);

                        // Download segment
                        using (request)
                        using (var response = await _httpService.PerformRequestAsync(request).ConfigureAwait(false))
                        using (var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            int bytesCopied;
                            do
                            {
                                // Copy
                                bytesCopied = await input.CopyChunkToAsync(output, cancellationToken)
                                    .ConfigureAwait(false);

                                // Report progress
                                totalBytesCopied += bytesCopied;
                                progress?.Report(1.0 * totalBytesCopied / info.Size);
                            } while (bytesCopied > 0);
                        }
                    }
                }
            }
            // Download non-limited streams directly
            else
            {
                using (var output = File.Create(filePath))
                using (var input = await GetMediaStreamAsync(info).ConfigureAwait(false))
                {
                    int bytesCopied;
                    do
                    {
                        // Copy
                        bytesCopied = await input.CopyChunkToAsync(output, cancellationToken)
                            .ConfigureAwait(false);

                        // Report progress
                        totalBytesCopied += bytesCopied;
                        progress?.Report(1.0 * totalBytesCopied / info.Size);
                    } while (bytesCopied > 0);
                }
            }
        }

        /// <summary>
        /// Gets the actual media stream associated with given metadata
        /// and downloads it to a file.
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath, IProgress<double> progress)
            => DownloadMediaStreamAsync(info, filePath, progress, CancellationToken.None);

        /// <summary>
        /// Gets the actual media stream associated with given metadata
        /// and downloads it to a file.
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath)
            => DownloadMediaStreamAsync(info, filePath, null);

#endif
    }
}