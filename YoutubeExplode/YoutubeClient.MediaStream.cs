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

            // Save to file
            var downloader = new SegmentedMediaStreamDownloader(_httpService, info);
            await downloader.DownloadAsync(filePath).ConfigureAwait(false);
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