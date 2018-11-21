using System;
using System.IO;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
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
            if (!YoutubeClient.TryParseVideoId(input, out var id))
                id = input;
            return id;
        }

        /// <summary>
        /// Turns file size in bytes into human-readable string.
        /// </summary>
        private static string NormalizeFileSize(long fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }

        private static async Task MainAsync()
        {
            // Client
            var client = new YoutubeClient();

            // Get the video ID
            Console.Write("Enter YouTube video ID or URL: ");
            var id = Console.ReadLine();
            id = NormalizeVideoId(id);
            Console.WriteLine();

            // Get the video info
            Console.Write("Obtaining general video info... ");
            var video = await client.GetVideoAsync(id);
            Console.WriteLine('✓');
            Console.WriteLine($"> {video.Title} by {video.Author}");
            Console.WriteLine();

            // Get media stream info set
            Console.Write("Obtaining media stream info set... ");
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
            Console.WriteLine('✓');
            Console.WriteLine("> " +
                              $"{streamInfoSet.Muxed.Count} muxed streams, " +
                              $"{streamInfoSet.Video.Count} video-only streams, " +
                              $"{streamInfoSet.Audio.Count} audio-only streams");
            Console.WriteLine();

            // Get the best muxed stream
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            Console.WriteLine("Selected muxed stream with highest video quality:");
            Console.WriteLine("> " +
                              $"{streamInfo.VideoQualityLabel} video quality | " +
                              $"{streamInfo.Container} format | " +
                              $"{NormalizeFileSize(streamInfo.Size)}");
            Console.WriteLine();

            // Compose file name, based on metadata
            var fileExtension = streamInfo.Container.GetFileExtension();
            var fileName = $"{video.Title}.{fileExtension}";

            // Replace illegal characters in file name
            fileName = fileName.Replace(Path.GetInvalidFileNameChars(), '_');

            // Download video
            Console.Write("Downloading... ");
            using (var progress = new ProgressBar())
                await client.DownloadMediaStreamAsync(streamInfo, fileName, progress);
            Console.WriteLine();

            Console.WriteLine($"Video saved to '{fileName}'");
            Console.ReadKey();
        }

        public static void Main(string[] args)
        {
            // This demo prompts for video ID, gets video info and downloads one media stream
            // It's intended to be very simple and straight to the point
            // For a more complicated example - check out the WPF demo

            Console.Title = "YoutubeExplode Demo";

            // Main method in consoles cannot be asynchronous so we run everything synchronously
            MainAsync().GetAwaiter().GetResult();
        }
    }
}
