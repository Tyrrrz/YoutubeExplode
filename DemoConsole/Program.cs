using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tyrrrz.Extensions;

namespace YoutubeExplode.DemoConsole
{
    public static class Program
    {
        /// <summary>
        /// If given a youtube url, parses video id from it.
        /// Otherwise returns the same string.
        /// </summary>
        private static string NormalizeId(string input)
        {
            if (!YoutubeClient.TryParseVideoId(input, out string id))
                id = input;
            return id;
        }

        /// <summary>
        /// Turns file size in bytes into human-readable string
        /// </summary>
        private static string NormalizeFileSize(long fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            int unit = 0;

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
            Console.Write("Enter Youtube video ID or URL: ");
            string id = Console.ReadLine();
            id = NormalizeId(id);

            // Get the video info
            Console.WriteLine("Loading...");
            var videoInfo = await client.GetVideoInfoAsync(id);
            Console.WriteLine('-'.Repeat(100));

            // Print metadata
            Console.WriteLine($"Id: {videoInfo.Id} | Title: {videoInfo.Title} | Author: {videoInfo.Author}");

            // Get the most preferable stream
            Console.WriteLine("Looking for the best stream that has both video and audio tracks...");
            var streamInfo = videoInfo.Streams
                .Where(s => s.ContainsVideo && s.ContainsAudio)
                .OrderBy(s => s.Quality)
                .Last();
            string normalizedFileSize = NormalizeFileSize(streamInfo.FileSize);
            Console.WriteLine($"Quality: {streamInfo.QualityLabel} | Container: {streamInfo.ContainerType} | Size: {normalizedFileSize}");

            // Compose file name, based on metadata
            string fileName = $"{videoInfo.Title}.{streamInfo.QualityLabel}.{streamInfo.FileExtension}";

            // Remove illegal characters from file name
            fileName = fileName.Except(Path.GetInvalidFileNameChars());

            // Download video
            Console.WriteLine($"Downloading to [{fileName}]...");
            Console.WriteLine('-'.Repeat(100));

            var progress = new Progress<double>(p => Console.Title = $"YoutubeExplode Demo [{p:P0}]");
            await client.DownloadMediaStreamAsync(streamInfo, fileName, progress);

            Console.WriteLine("Download complete!");
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
