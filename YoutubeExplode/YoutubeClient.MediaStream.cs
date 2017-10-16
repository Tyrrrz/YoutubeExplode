using System;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Services;

#if NET45 || NETCOREAPP1_0
using System.IO;
using System.Threading;
#endif

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <summary>
        /// Gets the actual media stream represented by given metadata
        /// </summary>
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo info)
        {
            info.GuardNotNull(nameof(info));

            // Get stream
            var stream = await _httpService.GetStreamAsync(info.Url).ConfigureAwait(false);

            return new MediaStream(info, stream);
        }

#if NET45 || NETCOREAPP1_0

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            info.GuardNotNull(nameof(info));
            filePath.GuardNotNull(nameof(filePath));

            // Save to file
            using (var input = await GetMediaStreamAsync(info).ConfigureAwait(false))
            using (var output = File.Create(filePath))
            {
                var buffer = new byte[4 * 1024];
                int bytesRead;
                long totalBytesRead = 0;
                do
                {
                    // Read
                    totalBytesRead += bytesRead =
                        await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                    // Write
                    await output.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                    // Report progress
                    progress?.Report(1.0 * totalBytesRead / input.Length);
                } while (bytesRead > 0);
            }
        }

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath, IProgress<double> progress)
            => DownloadMediaStreamAsync(info, filePath, progress, CancellationToken.None);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath)
            => DownloadMediaStreamAsync(info, filePath, null);

#endif
    }
}