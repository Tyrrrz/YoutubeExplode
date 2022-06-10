using System;
using System.Threading.Tasks;
using YoutubeExplode.DemoConsole.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.DemoConsole;

// This demo prompts for video ID and downloads one media stream.
// It's intended to be very simple and straight to the point.
// For a more involved example - check out the WPF demo.
public static class Program
{
    public static async Task Main()
    {
        Console.Title = "YoutubeExplode Demo";

        var youtube = new YoutubeClient();

        // Get the video ID
        Console.Write("Enter YouTube video ID or URL: ");
        var videoId = VideoId.Parse(Console.ReadLine() ?? "");

        // Get available streams and choose the best muxed (audio + video) stream
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
        if (streamInfo is null)
        {
            // Available streams vary depending on the video and it's possible
            // there may not be any muxed streams at all.
            // See the readme to learn how to handle adaptive streams.
            Console.Error.WriteLine("This video has no muxed streams.");
            return;
        }

        // Download the stream
        var fileName = $"{videoId}.{streamInfo.Container.Name}";

        Console.Write(
            $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
        );

        using (var progress = new ConsoleProgress())
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

        Console.WriteLine("Done");
        Console.WriteLine($"Video saved to '{fileName}'");
    }
}