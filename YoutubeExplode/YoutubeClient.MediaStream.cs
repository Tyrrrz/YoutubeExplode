using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo info)
        {
            info.GuardNotNull(nameof(info));

            // Maximum segment stream size
            const long segmentSize = 9_898_989; // this number was carefully devised through research

            // Determine if stream is rate-limited
            var isRateLimited = !Regex.IsMatch(info.Url, @"ratebypass[=/]yes");

            // Determine if the stream is longer than one segment
            var isMultiSegment = info.Size > segmentSize;

            // If rate-limited and long enough - split into segments and wrap into one stream
            if (isRateLimited && isMultiSegment)
            {
                // Determine segment count
                var segmentCount = (int) Math.Ceiling(1.0 * info.Size / segmentSize);

                // Create resolvers for multi stream
                var resolvers = new List<Func<Task<Stream>>>();
                for (var i = 0; i < segmentCount; i++)
                {
                    // Determine segment range
                    var from = i * segmentSize;
                    var to = (i + 1) * segmentSize - 1;

                    // Create resolver for this segment
                    var resolver = new Func<Task<Stream>>(() => _httpClient.GetStreamAsync(info.Url, from, to));
                    resolvers.Add(resolver);
                }

                // Create multi stream from segment resolvers
                var stream = new AsyncMultiStream(resolvers);
                return new MediaStream(info, stream);
            }
            // If not rate-limited or not long enough - get the stream in one segment
            else
            {
                // Get the whole stream as one segment
                var stream = await _httpClient.GetStreamAsync(info.Url, 0, info.Size).ConfigureAwait(false);
                return new MediaStream(info, stream);
            }
        }

        /// <inheritdoc />
        public async Task DownloadMediaStreamAsync(MediaStreamInfo info, Stream output,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            info.GuardNotNull(nameof(info));
            output.GuardNotNull(nameof(output));

            using (var input = await GetMediaStreamAsync(info).ConfigureAwait(false))
                await input.CopyToAsync(output, progress, cancellationToken).ConfigureAwait(false);
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