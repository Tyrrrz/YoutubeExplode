using System;
using System.IO;
using System.Linq;
using Tyrrrz.Extensions;
using YoutubeExplode.NetFx;

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
            string id;
            if (!YoutubeClient.TryParseVideoId(input, out id))
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
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }

        public static void Main(string[] args)
        {
            // This demo downloads one media stream for the given video
            Console.Title = "YoutubeExplode Demo";
            Console.WindowWidth = 86;
            Console.WindowHeight = 38;

            // Client
            var client = new YoutubeClient();

            // Get the video ID
            Console.Write("Enter Youtube video ID or URL: ");
            string id = Console.ReadLine();
            id = NormalizeId(id);

            // Get the video info
            Console.WriteLine("Loading...");
            var videoInfo = client.GetVideoInfoAsync(id).GetAwaiter().GetResult();
            Console.WriteLine('-'.Repeat(15));

            // Print metadata
            Console.WriteLine($"Id: {videoInfo.Id} | Title: {videoInfo.Title} | Author: {videoInfo.Author}");
            
            // Get the most preferable stream
            Console.WriteLine("Looking for the best stream that has both video and audio tracks...");
            var streamInfo = videoInfo.Streams
                .Where(s => s.HasVideo && s.HasAudio)
                .OrderBy(s => s.Quality)
                .Last();
            string normalizedFileSize = NormalizeFileSize(streamInfo.FileSize);
            Console.WriteLine($"Quality: {streamInfo.QualityLabel} | Container: {streamInfo.ContainerType} | Size: {normalizedFileSize}");

            // Compose file name, based on metadata
            string fileName = $"{videoInfo.Title}.{streamInfo.FileExtension}".Except(Path.GetInvalidFileNameChars());

            // Download video
            Console.WriteLine($"Downloading to [{fileName}]...");
            Console.WriteLine('-'.Repeat(15));
            client.DownloadMediaStreamAsync(streamInfo, fileName).GetAwaiter().GetResult();

            Console.WriteLine("Download complete!");
            Console.ReadKey();
            client.Dispose();
        }
    }
}
