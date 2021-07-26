# YoutubeExplode

[![Build](https://github.com/Tyrrrz/YoutubeExplode/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/YoutubeExplode/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/YoutubeExplode/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/YoutubeExplode)
[![Version](https://img.shields.io/nuget/v/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

⚠️ **Project status: maintenance mode** (bug fixes only).

YoutubeExplode is a library that provides an interface to query metadata of YouTube videos, playlists and channels, as well as to resolve and download video streams and closed caption tracks.
Behind a layer of abstraction, the library parses raw page content and uses reverse-engineered requests to retrieve information.
As it doesn't use the official API, there's also no need for an API key and there are no usage quotas.

> This library is used in [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader), a desktop application for downloading and converting YouTube videos.

💬 **If you want to chat, join my [Discord server](https://discord.gg/2SUWKFnHSm)**.

## Download

📦 [NuGet](https://nuget.org/packages/YoutubeExplode): `dotnet add package YoutubeExplode`

> See also [YoutubeExplode.Converter](https://github.com/Tyrrrz/YoutubeExplode.Converter) for an extension package that combines YoutubeExplode with FFmpeg.

## Screenshots

![demo](.screenshots/demo.png)

## Usage

YoutubeExplode exposes its functionality through a single entry point -- the `YoutubeClient` class.
Create an instance of this class and use the provided operations on `Videos`, `Playlists`, `Channels`, and `Search` properties to send requests.

### Videos

#### Retrieving video metadata

To retrieve metadata associated with a YouTube video, call `Videos.GetAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

// You can specify both video ID or URL
var video = await youtube.Videos.GetAsync("https://youtube.com/watch?v=u_yIGGhubZs");

var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
var author = video.Author.Title; // "Blender"
var duration = video.Duration; // 00:07:20
```

#### Downloading video streams

Every YouTube video has a number of streams available, differing in containers, video quality, bitrate, framerate, and other properties.
Additionally, depending on the content of the stream, the streams are further divided into 3 categories:

- Muxed streams -- contain both video and audio
- Audio-only streams -- contain only audio
- Video-only streams -- contain only video

You can request the manifest that lists all available streams for a particular video by calling `Videos.Streams.GetManifestAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var streamManifest = await youtube.Videos.Streams.GetManifestAsync("u_yIGGhubZs");
```

Once you get the manifest, you can filter through the streams and select the ones you're interested in:

```csharp
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

// ...

// Get highest quality muxed stream
var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

// ...or highest bitrate audio-only stream
var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

// ...or highest quality MP4 video-only stream
var streamInfo = streamManifest
    .GetVideoOnlyStreams()
    .Where(s => s.Container == Container.Mp4)
    .GetWithHighestVideoQuality()
```

Finally, you can resolve the actual stream represented by the specified metadata using `Videos.Streams.GetAsync(...)` or download it directly to a file with `Videos.Streams.DownloadAsync(...)`:

```csharp
// ...

// Get the actual stream
var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

// Download the stream to a file
await youtube.Videos.Streams.DownloadAsync(streamInfo, $"video.{streamInfo.Container}");
```

> Note that while it may be tempting to just always use muxed streams, given that they contain both audio and video, it's important to note that they are limited in quality and don't go beyond 720p30.
If you want to download the video in maximum quality, you need to download the audio-only and video-only streams separately and then mux them together on your own using tools like [FFmpeg](http://ffmpeg.org).
You can also use [YoutubeExplode.Converter](https://github.com/Tyrrrz/YoutubeExplode.Converter) which wraps FFmpeg and provides an extension point for YoutubeExplode to download videos directly.

#### Downloading closed captions

Closed captions can be downloaded in a similar way to media streams.
To get the list of available closed caption tracks, call `Videos.ClosedCaptions.GetManifestAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync("u_yIGGhubZs");
```

Then retrieve metadata for a particular track:

```csharp
// ...

// Find closed caption track in English
var trackInfo = trackManifest.GetByLanguage("en");
```

Finally, use `Videos.ClosedCaptions.GetAsync(...)` to get the actual content of the track:

```csharp
// ...

var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

// Get the caption displayed at 0:35
var caption = track.GetByTime(TimeSpan.FromSeconds(35));
var text = caption.Text; // "collection acts as the parent collection"
```

You can also download the closed caption track in SRT file format with `Videos.ClosedCaptions.DownloadAsync(...)`:

```csharp
// ...

await youtube.Videos.ClosedCaptions.DownloadAsync(trackInfo, "cc_track.srt");
```

### Playlists

#### Retrieving playlist metadata

You can get metadata associated with a YouTube playlist by calling `Playlists.GetAsync(...)` method:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var playlist = await youtube.Playlists.GetAsync("PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6");

var title = playlist.Title; // "First Steps - Blender 2.80 Fundamentals"
var author = playlist.Author.Title; // "Blender"
```

#### Getting videos included in a playlist

To get the videos included in a playlist, call `Playlists.GetVideosAsync(...)`:

```csharp
using YoutubeExplode;
using YoutubeExplode.Common;

var youtube = new YoutubeClient();

// Get all playlist videos
var videos = await youtube.Playlists.GetVideosAsync("PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6");

// Get the first 20 playlist videos
var videosSubset = await youtube.Playlists
    .GetVideosAsync(playlist.Id)
    .CollectAsync(20);
```

You can also enumerate videos lazily without waiting for the whole list to load:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

await foreach (var video in youtube.Playlists.GetVideosAsync("PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6"))
{
    var title = video.Title;
    var author = video.Author;
}
```

If you need precise control over how many requests you send to YouTube, use `Playlists.GetVideoBatchesAsync(...)` which returns videos wrapped in batches:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

// Each batch corresponds to one request
await foreach (var batch in youtube.Playlists.GetVideoBatchesAsync("PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6"))
{
    foreach (var video in batch.Items)
    {
        var title = video.Title;
        var author = video.Author;
    }
}
```

### Channels

#### Retrieving channel metadata

You can get metadata associated with a YouTube channel by calling `Channels.GetAsync(...)` method:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var channel = await youtube.Channels.GetAsync("UCSMOQeBJ2RAnuFungnQOxLg");

var title = channel.Title; // "Blender"
```

You can also get channel metadata by username with `Channels.GetByUserAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var channel = await youtube.Channels.GetByUserAsync("Blender");

var id = channel.Id; // "UCSMOQeBJ2RAnuFungnQOxLg"
```

#### Getting channel uploads

To get a list of videos uploaded by a channel, call `Channels.GetUploadsAsync(...)`:

```csharp
using YoutubeExplode;
using YoutubeExplode.Common;

var youtube = new YoutubeClient();

var videos = await youtube.Channels.GetUploadsAsync("UCSMOQeBJ2RAnuFungnQOxLg");
```

### Searching

You can execute a search query and get the results by calling `Search.GetResultsAsync(...)`.
Each result may represent either a video, a playlist, or a channel, so you need to use pattern matching to handle corresponding cases:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

await foreach (var result in youtube.Search.GetResultsAsync("blender tutorials"))
{
    // Use pattern matching to handle different results (videos, playlists, channels)
    switch (result)
    {
        case VideoSearchResult videoResult:
        {
            var id = videoResult.Id;
            var title = videoResult.Title;
            var duration = videoResult.Duration;
            break;
        }
        case PlaylistSearchResult playlistResult:
        {
            var id = playlistResult.Id;
            var title = playlistResult.Title;
            break;
        }
        case ChannelSearchResult channelResult:
        {
            var id = channelResult.Id;
            var title = channelResult.Title;
            break;
        }
    }
}
```

To limit results to a specific type, use `Search.GetVideosAsync(...)`, `Search.GetPlaylistsAsync(...)`, or `Search.GetChannelsAsync(...)`:

```csharp
using YoutubeExplode;
using YoutubeExplode.Common;

var youtube = new YoutubeClient();

var videos = await youtube.Search.GetVideosAsync("blender tutorials");
var playlists = await youtube.Search.GetPlaylistsAsync("blender tutorials");
var channels = await youtube.Search.GetChannelsAsync("blender tutorials");
```

Similarly to playlists, you can also enumerate results in batches by calling `Search.GetResultBatchesAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

// Each batch corresponds to one request
await foreach (var batch in youtube.Search.GetResultBatchesAsync("blender tutorials"))
{
    foreach (var result in batch.Items)
    {
        switch (result)
        {
            case VideoSearchResult videoResult:
            {
                // ...
            }
            case PlaylistSearchResult playlistResult:
            {
                // ...
            }
            case ChannelSearchResult channelResult:
            {
                // ...
            }
        }
    }
}
```

## Etymology

The "Explode" in YoutubeExplode comes from the name of a PHP function that splits up strings, [`explode()`](https://www.php.net/manual/en/function.explode.php). When I was just starting development on this library, most of the reference source code I read was written in PHP, hence the inspiration for the name.
