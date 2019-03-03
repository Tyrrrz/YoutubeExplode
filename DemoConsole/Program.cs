using System;
using System.Threading.Tasks;
using DemoConsole.Internal;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace DemoConsole
{
    public static class Program
    {
        /// <summary>
        /// If given a YouTube URL, parses video id from it.
        /// Otherwise returns the same string.
        /// </summary>
        private static string NormalizeVideoId(string input)
        {
            return YoutubeClient.TryParseVideoId(input, out var videoId) 
                ? videoId
                : input;
        }

        private static async Task MainAsync()
        {
            // Client
            var client = new YoutubeClient();

            // Get the video ID
            Console.Write("Enter YouTube video ID or URL: ");
            var videoId = Console.ReadLine();
            videoId = NormalizeVideoId(videoId);

            // Get media stream info set
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            // Choose the best muxed stream
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

            // Compose file name, based on metadata
            var fileExtension = streamInfo.Container.GetFileExtension();
            var fileName = $"{videoId}.{fileExtension}";

            // Download video
            Console.Write($"Downloading stream: {streamInfo.VideoQualityLabel} / {fileExtension}... ");
            using (var progress = new InlineProgress())
                await client.DownloadMediaStreamAsync(streamInfo, fileName, progress);

            Console.WriteLine($"Video saved to '{fileName}'");
            Console.ReadKey();
        }

        public static void Main(string[] args)
        {
            // This demo prompts for video ID and downloads one media stream
            // It's intended to be very simple and straight to the point
            // For a more complicated example - check out the WPF demo

            Console.Title = "YoutubeExplode Demo";

            // Main method in consoles cannot be asynchronous so we run everything synchronously
            MainAsync().GetAwaiter().GetResult();
        }
    }
}
