using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode.Models;

namespace YoutubeExplode.NetFx
{
    /// <summary>
    /// .NET Framework compatible extensions for YoutubeExplode
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public static async Task DownloadMediaStreamAsync(this YoutubeClient client, MediaStreamInfo mediaStreamInfo,
            string filePath)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (mediaStreamInfo == null)
                throw new ArgumentNullException(nameof(mediaStreamInfo));
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            // Get and save to file
            using (var input = await client.GetMediaStreamAsync(mediaStreamInfo))
            using (var output = File.Create(filePath))
                await input.CopyToAsync(output);
        }

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public static async Task DownloadClosedCaptionTrackAsync(this YoutubeClient client,
            ClosedCaptionTrackInfo closedCaptionTrackInfo, string filePath)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (closedCaptionTrackInfo == null)
                throw new ArgumentNullException(nameof(closedCaptionTrackInfo));
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            // Get
            var closedCaptionTrack = await client.GetClosedCaptionTrackAsync(closedCaptionTrackInfo);

            // Save to file as SRT
            using (var output = File.Create(filePath))
            using (var sw = new StreamWriter(output))
            {
                for (int i = 0; i < closedCaptionTrack.Captions.Count; i++)
                {
                    var closedCaption = closedCaptionTrack.Captions[i];

                    await sw.WriteLineAsync((i + 1).ToString());
                    await sw.WriteAsync(closedCaption.Offset.ToString(@"hh\:mm\:ss\,fff"));
                    await sw.WriteAsync(" --> ");
                    await sw.WriteLineAsync((closedCaption.Offset + closedCaption.Duration).ToString(@"hh\:mm\:ss\,fff"));
                    await sw.WriteLineAsync(closedCaption.Text);
                    await sw.WriteLineAsync();
                }
            }
        }
    }
}