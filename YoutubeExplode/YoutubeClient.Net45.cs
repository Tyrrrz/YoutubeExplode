#if NET45
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;
#endif

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
#if NET45

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken, int bufferSize)
        {
            if (mediaStreamInfo == null)
                throw new ArgumentNullException(nameof(mediaStreamInfo));
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            // Get and create streams
            var input = await GetMediaStreamAsync(mediaStreamInfo).ConfigureAwait(false);
            var output = File.Create(filePath, bufferSize);

            // Get and save to file with progress reporting
            if (progress != null)
            {
                using (input)
                using (output)
                {
                    var buffer = new byte[bufferSize];
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
                        progress.Report(1.0 * totalBytesRead / input.Length);
                    } while (bytesRead > 0);
                }
            }
            // Get and save to file without progress reporting
            else
            {
                using (input)
                using (output)
                {
                    await input.CopyToAsync(output, bufferSize, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
            => await DownloadMediaStreamAsync(mediaStreamInfo, filePath, progress, cancellationToken, 4096)
                .ConfigureAwait(false);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath,
            IProgress<double> progress)
            => await DownloadMediaStreamAsync(mediaStreamInfo, filePath, progress, CancellationToken.None)
                .ConfigureAwait(false);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath)
            => await DownloadMediaStreamAsync(mediaStreamInfo, filePath, null)
                .ConfigureAwait(false);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath, IProgress<double> progress, CancellationToken cancellationToken, int bufferSize)
        {
            if (closedCaptionTrackInfo == null)
                throw new ArgumentNullException(nameof(closedCaptionTrackInfo));
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            // Get and create streams
            var closedCaptionTrack = await GetClosedCaptionTrackAsync(closedCaptionTrackInfo).ConfigureAwait(false);
            var output = File.Create(filePath, bufferSize);
            var sw = new StreamWriter(output, Encoding.Unicode, bufferSize);

            // Save to file as SRT
            using (output)
            using (sw)
            {
                for (int i = 0; i < closedCaptionTrack.Captions.Count; i++)
                {
                    // Make sure cancellation was not requested
                    cancellationToken.ThrowIfCancellationRequested();

                    var closedCaption = closedCaptionTrack.Captions[i];
                    var buffer = new StringBuilder();

                    // Line number
                    buffer.AppendLine((i + 1).ToString());

                    // Time start --> time end
                    buffer.Append(closedCaption.Offset.ToString(@"hh\:mm\:ss\,fff"));
                    buffer.Append(" --> ");
                    buffer.Append((closedCaption.Offset + closedCaption.Duration).ToString(@"hh\:mm\:ss\,fff"));
                    buffer.AppendLine();

                    // Actual text
                    buffer.AppendLine(closedCaption.Text);

                    // Write to stream
                    await sw.WriteLineAsync(buffer.ToString()).ConfigureAwait(false);

                    // Report progress
                    progress?.Report((i + 1.0) / closedCaptionTrack.Captions.Count);
                }
            }
        }

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath, IProgress<double> progress, CancellationToken cancellationToken)
            => await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progress,
                    cancellationToken, 4096)
                .ConfigureAwait(false);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath, IProgress<double> progress)
            => await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progress,
                    CancellationToken.None)
                .ConfigureAwait(false);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath)
            => await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, null)
                .ConfigureAwait(false);

#endif
    }
}