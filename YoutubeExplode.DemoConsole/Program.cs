using System;
using System.Threading.Tasks;
using YoutubeExplode.DemoConsole.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.DemoConsole
{
    // This demo prompts for video ID and downloads one media stream.
    // It's intended to be very simple and straight to the point.
    // For a more involved example - check out the WPF demo.

    public static class Program
    {
        public static async Task<int> Main()
        {
            Console.Title = "YoutubeExplode Demo";

            var youtube = new YoutubeClient();

            // Read the video ID
            Console.Write("Enter YouTube video ID or URL: ");
            var videoId = VideoId.Parse(Console.ReadLine() ?? "");

            // Get available streams and choose the best muxed (audio + video) stream
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
            if (streamInfo is null)
            {
                // Available streams vary depending on the video and
                // it's possible there may not be any muxed streams.
                Console.Error.WriteLine("This video has no muxed streams.");
                return 1;
            }

            // Download the stream
            Console.Write(
                $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
            );

            var fileName = $"{videoId}.{streamInfo.Container.Name}";

            using (var progress = new InlineProgress()) // display progress in console
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

            Console.WriteLine($"Video saved to '{fileName}'");

            return 0;
        }
    }
}