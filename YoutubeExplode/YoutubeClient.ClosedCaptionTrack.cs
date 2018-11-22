using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models.ClosedCaptions;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo info)
        {
            info.GuardNotNull(nameof(info));

            // Get parser
            var parser = await GetClosedCaptionTrackAjaxParserAsync(info.Url).ConfigureAwait(false);

            // Parse captions
            var closedCaptions = new List<ClosedCaption>();
            foreach (var closedCaptionParser in parser.GetClosedCaptions())
            {
                // Parse info
                var text = closedCaptionParser.ParseText();

                // Skip caption tracks without text
                if (text.IsBlank())
                    continue;

                var offset = closedCaptionParser.ParseOffset();
                var duration = closedCaptionParser.ParseDuration();

                var caption = new ClosedCaption(text, offset, duration);
                closedCaptions.Add(caption);
            }

            return new ClosedCaptionTrack(info, closedCaptions);
        }

        /// <inheritdoc />
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info, Stream output,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            info.GuardNotNull(nameof(info));
            output.GuardNotNull(nameof(output));

            // Get the track
            var track = await GetClosedCaptionTrackAsync(info).ConfigureAwait(false);

            // Save to file as SRT
            using (var writer = new StreamWriter(output, Encoding.UTF8, 1024, true))
            {
                for (var i = 0; i < track.Captions.Count; i++)
                {
                    // Make sure cancellation was not requested
                    cancellationToken.ThrowIfCancellationRequested();

                    var caption = track.Captions[i];
                    var buffer = new StringBuilder();

                    // Line number
                    buffer.AppendLine((i + 1).ToString());

                    // Time start --> time end
                    buffer.Append(caption.Offset.ToString(@"hh\:mm\:ss\,fff"));
                    buffer.Append(" --> ");
                    buffer.Append((caption.Offset + caption.Duration).ToString(@"hh\:mm\:ss\,fff"));
                    buffer.AppendLine();

                    // Actual text
                    buffer.AppendLine(caption.Text);

                    // Write to stream
                    await writer.WriteLineAsync(buffer.ToString()).ConfigureAwait(false);

                    // Report progress
                    progress?.Report((i + 1.0) / track.Captions.Count);
                }
            }
        }

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0

        /// <inheritdoc />
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info, string filePath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            filePath.GuardNotNull(nameof(filePath));

            using (var output = File.Create(filePath))
                await DownloadClosedCaptionTrackAsync(info, output, progress, cancellationToken).ConfigureAwait(false);
        }

#endif
    }
}