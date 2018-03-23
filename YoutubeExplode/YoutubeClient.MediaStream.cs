using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Services;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo info)
        {
            info.GuardNotNull(nameof(info));

            // Get stream
            var stream = await _httpService.GetStreamAsync(info.Url).ConfigureAwait(false);

            return new MediaStream(info, stream);
        }

        /// <inheritdoc />
        public async Task DownloadMediaStreamAsync(MediaStreamInfo info, Stream output,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            info.GuardNotNull(nameof(info));
            output.GuardNotNull(nameof(output));

            // Keep track of bytes copied for progress reporting
            var totalBytesCopied = 0L;

            // Determine if stream is rate-limited
            var isRateLimited = !Regex.IsMatch(info.Url, @"ratebypass[=/]yes");

            // Download rate-limited streams in segments
            if (isRateLimited)
            {
                // Determine segment count
                const long segmentSize = 9_898_989; // this number was carefully devised through research
                var segmentCount = (int) Math.Ceiling(1.0 * info.Size / segmentSize);

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
            // Download non-limited streams directly
            else
            {
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

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0

        /// <inheritdoc />
        public async Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            filePath.GuardNotNull(nameof(filePath));

            using (var output = File.Create(filePath))
                await DownloadMediaStreamAsync(info, output, progress, cancellationToken).ConfigureAwait(false);
        }

#endif
    }
}