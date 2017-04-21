﻿#if NET45
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
            var input = await GetMediaStreamAsync(mediaStreamInfo);
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
                        totalBytesRead += bytesRead =
                            await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        await output.WriteAsync(buffer, 0, bytesRead, cancellationToken);
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
                    await input.CopyToAsync(output, bufferSize, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
            => await DownloadMediaStreamAsync(mediaStreamInfo, filePath, progress, cancellationToken, 4096);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath,
            IProgress<double> progress)
            => await DownloadMediaStreamAsync(mediaStreamInfo, filePath, progress, CancellationToken.None);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo, string filePath)
            => await DownloadMediaStreamAsync(mediaStreamInfo, filePath, null);

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
            var closedCaptionTrack = await GetClosedCaptionTrackAsync(closedCaptionTrackInfo);
            var output = File.Create(filePath, bufferSize);
            var sw = new StreamWriter(output, Encoding.UTF8, bufferSize);

            // Save to file as SRT
            using (output)
            using (sw)
            {
                for (int i = 0; i < closedCaptionTrack.Captions.Count; i++)
                {
                    var closedCaption = closedCaptionTrack.Captions[i];

                    // Line number
                    await sw.WriteLineAsync((i + 1).ToString());

                    // Time start --> time end
                    await sw.WriteAsync(closedCaption.Offset.ToString(@"hh\:mm\:ss\,fff"));
                    await sw.WriteAsync(" --> ");
                    await sw.WriteAsync((closedCaption.Offset + closedCaption.Duration).ToString(@"hh\:mm\:ss\,fff"));
                    await sw.WriteLineAsync();

                    // Actual text
                    await sw.WriteLineAsync(closedCaption.Text);

                    // Separator
                    await sw.WriteLineAsync();

                    progress?.Report((i + 1.0) / closedCaptionTrack.Captions.Count);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath, IProgress<double> progress, CancellationToken cancellationToken)
            => await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progress,
                cancellationToken, 4096);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath, IProgress<double> progress)
            => await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progress,
                CancellationToken.None);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo,
            string filePath)
            => await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, null);

#endif
    }
}