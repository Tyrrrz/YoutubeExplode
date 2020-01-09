using System;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.ChanelHandler;
using YoutubeExplode.DemoConsole.Internal;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.DemoConsole
{
    public static class Program
    {
        /// <summary>
        /// If given a YouTube URL, parses video id from it.
        /// Otherwise returns the same string.
        /// </summary>
        private static string NormalizeVideoId(string input) =>
            YoutubeClient.TryParseVideoId(input, out var videoId) 
                ? videoId!
                : input;

        private static async Task MainAsync()
        {
            // Client
            var client = new YoutubeClient();

            Console.WriteLine("Channel ID");
            var channelId = Console.ReadLine();
            //var channel = await client.GetChannelAsync(channelId);
            var cp = new ChannelParser();
            string? continuationToken = null;
            do
            {
                ChannelPlaylists channel;
                if (continuationToken != null)
                {
                    channel = await cp.GetPlaylistJson(channelId, continuationToken);
                }
                else
                {
                    channel = await cp.GetPlaylistJson(channelId);
                }
                
                foreach (var channelPlaylist in channel.Playlists)
                {
                    Console.WriteLine(channelPlaylist.Title);
                }

                continuationToken = channel.ContinuationToken;
            } while (continuationToken != null);

            //var res = await cp.GetPlaylistJson(channelId);
            //var res2 = await cp.GetPlaylistJson(res.Id, res.ContinuationToken);
            //var channelVideos = await client.GetChannelUploadsAsync(channelId);

            // Get the video ID
            Console.Write("Enter YouTube video ID or URL: ");
            var videoId = Console.ReadLine();
            videoId = NormalizeVideoId(videoId);

            // Get media stream info set
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            // Choose the best muxed stream
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            if (streamInfo == null)
            {
                Console.WriteLine("This videos has no streams");
                return;
            }

            // Compose file name, based on metadata
            var fileExtension = streamInfo.Container.GetFileExtension();
            var fileName = $"{videoId}.{fileExtension}";

            // Download video
            Console.Write($"Downloading stream: {streamInfo.VideoQualityLabel} / {fileExtension}... ");
            using (var progress = new InlineProgress())
                await client.DownloadMediaStreamAsync(streamInfo, fileName, progress);

            Console.WriteLine($"Video saved to '{fileName}'");
        }

        public static void Main()
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
