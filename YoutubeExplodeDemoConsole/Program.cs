using System;
using System.IO;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;

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
            Console.Title = "YoutubeExplode Demo";
            Console.WindowWidth = 85;
            Console.WindowHeight = 38;

            // Client
            var client = new YoutubeClient();

            // Get the video ID
            Console.WriteLine("Enter Youtube video ID or URL:");
            string id = Console.ReadLine();
            id = NormalizeId(id);

            Console.WriteLine("Loading . . .");
            Console.WriteLine();

            // Get the video info
            var videoInfo = client.GetVideoInfoAsync(id).Result;

            // Output some meta data
            Console.WriteLine($"{videoInfo.Title} | {videoInfo.ViewCount:N0} views | {videoInfo.AverageRating:0.##}* rating");
            Console.WriteLine("Streams:");
            for (int i = 0; i < videoInfo.Streams.Length; i++)
            {
                var streamInfo = videoInfo.Streams[i];
                string normFileSize = NormalizeFileSize(streamInfo.FileSize);

                // Video+audio streams (non-adaptive)
                if (streamInfo.AdaptiveMode == VideoStreamAdaptiveMode.None)
                {
                    Console.WriteLine($"\t[{i}] Mixed | {streamInfo.Type} | {streamInfo.QualityLabel} | {normFileSize}");
                }
                // Video only streams
                else if (streamInfo.AdaptiveMode == VideoStreamAdaptiveMode.Video)
                    Console.WriteLine($"\t[{i}] Video | {streamInfo.Type} | {streamInfo.QualityLabel} | {streamInfo.Fps} FPS | {normFileSize}");
                // Audio only streams
                else if (streamInfo.AdaptiveMode == VideoStreamAdaptiveMode.Audio)
                    Console.WriteLine($"\t[{i}] Audio | {streamInfo.Type} | {normFileSize}");
                // This should not happen
                else
                    throw new IndexOutOfRangeException();
            }

            // Get the stream index to download
            Console.WriteLine();
            Console.WriteLine("Enter corresponding number to download:");
            int streamIndex = Console.ReadLine().ParseInt();
            var selectedStream = videoInfo.Streams[streamIndex];

            Console.WriteLine("Loading . . .");
            Console.WriteLine();

            // Compose file name, based on meta data
            string fileName = $"{videoInfo.Title}.{selectedStream.FileExtension}".Without(Path.GetInvalidFileNameChars());

            // Download video
            using (var input = client.DownloadVideoAsync(selectedStream).Result)
            using (var output = File.Create(fileName))
                input.CopyTo(output);

            Console.WriteLine("Done!");
            Console.ReadKey();
            client.Dispose();
        }
    }
}