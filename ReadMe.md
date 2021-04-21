# YoutubeExplode

[![Build](https://github.com/Tyrrrz/YoutubeExplode/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/YoutubeExplode/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/YoutubeExplode/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/YoutubeExplode)
[![Version](https://img.shields.io/nuget/v/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

⚠️ **Project status: maintenance mode** (bug fixes only).

YoutubeExplode is a library that provides an interface to query metadata of YouTube videos, playlists and channels, as well as to resolve and download video streams and closed caption tracks.
Behind a layer of abstraction, the library parses raw page content and uses reverse-engineered requests to retrieve information.
As it doesn't use the official API, there's also no need for an API key and there are no usage quotas.

This library is used in [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader), a desktop application for downloading and converting YouTube videos.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode): `dotnet add package YoutubeExplode`

## Screenshots

![demo](.screenshots/demo.png)

### Videos

#### Retrieving video metadata

The following example shows how you can extract various metadata from a YouTube video:

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

Every YouTube video has a number of streams available.
These streams may have different containers, video quality, bitrate, etc.

On top of that, depending on the content of the stream, the streams are further divided into 3 categories:

- Muxed streams -- contain both video and audio
- Audio-only streams -- contain only audio
- Video-only streams -- contain only video

You can request the manifest containing available streams for a particular video by calling `Streams.GetManifestAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var streamManifest = await youtube.Videos.Streams.GetManifestAsync("u_yIGGhubZs");
```

Once you get the manifest, you can filter through the streams and select the one you're interested in downloading:

```csharp
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

// Get highest quality muxed stream
var streamInfo = streamManifest.GetMuxed().GetWithHighestVideoQuality();

// ...or highest bitrate audio-only stream
var streamInfo = streamManifest.GetAudioOnly().GetWithHighestBitrate();

// ...or highest quality MP4 video-only stream
var streamInfo = streamManifest
    .GetVideoOnly()
    .Where(s => s.Container == Container.Mp4)
    .GetWithHighestVideoQuality()
```

Finally, you can resolve the actual stream represented by the specified metadata using `Streams.GetAsync(...)` or download it directly to a file with `Streams.DownloadAsync(...)`:

```csharp
using YoutubeExplode;

// Get the actual stream
var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

// Download the stream to a file
await youtube.Videos.Streams.DownloadAsync(streamInfo, $"video.{streamInfo.Container}");
```

> While it may be tempting to just always use muxed streams, given that they contain both audio and video, it's important to note that they are limited in quality and don't go beyond 720p30.
If you want to download the video in maximum quality, you need to download the audio-only and video-only streams separately and then mux them together on your own using tools like [FFmpeg](http://ffmpeg.org).
You can also use [YoutubeExplode.Converter](https://github.com/Tyrrrz/YoutubeExplode.Converter) which wraps FFmpeg and provides an extension point for YoutubeExplode to download videos directly.

#### Downloading closed captions

Closed captions can be downloaded similarly to media streams.
To get the list of available closed caption tracks, call `ClosedCaptions.GetManifestAsync(...)`:

```csharp
using YoutubeExplode;

var youtube = new YoutubeClient();

var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync("u_yIGGhubZs");
```

Then retrieve metadata for a particular track:

```csharp
using YoutubeExplode;

// Find closed caption track in English
var trackInfo = trackManifest.GetByLanguage("en-US");
```

Finally, get the content of the track by using `ClosedCaptions.GetAsync(...)`:

```csharp
var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

// Get the caption displayed at 0:35
var caption = track.GetByTime(TimeSpan.FromSeconds(35));
var text = caption.Text;
```

You can also download the closed caption track to a file in SRT format:

```csharp
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

You can also enumerate videos without waiting for the whole list to load:

```csharp
using YoutubeExplode;

await foreach (var video in youtube.Playlists.GetVideosAsync("PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6"))
{
    var title = video.Title;
    var author = video.Author;
}
```

If you need precise control over how many requests you send to YouTube, use `Playlists.GetVideoBatchesAsync(...)` which returns videos wrapped in batches:

```csharp
using YoutubeExplode;

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

You can also get channel metadata by username using `Channels.GetByUserAsync(...)`:

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

You can execute a search query and get the results by calling `Search.GetResultsAsync(...)`:

```csharp
using YoutubeExplode;

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
        }
        case PlaylistSearchResult playlistResult:
        {
            var id = playlistResult.Id;
            var title = playlistResult.Title;
        }
        case ChannelSearchResult channelResult
        {
            var id = channelResult.Id;
            var title = channelResult.Title;
        }
    }
}
```

To limit the results to a specific type, use `Search.GetVideosAsync(...)`, `Search.GetPlaylistsAsync(...)`, or `Search.GetChannelsAsync(...)`:

```csharp
using YoutubeExplode;
using YoutubeExplode.Common;

var videos = await youtube.Search.GetVideosAsync("blender tutorials");
```

Similarly to playlists, you can also enumerate results in batches by calling `Search.GetResultBatchesAsync(...)`:

```csharp
using YoutubeExplode;

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
            case ChannelSearchResult channelResult
            {
                // ...
            }
        }
    }
}
```

## Etymology

The "Explode" in YoutubeExplode comes from the name of a PHP function that splits up strings, [`explode()`](https://www.php.net/manual/en/function.explode.php). When I was just starting development on this library, most of the reference source code I read was written in PHP, hence the inspiration for the name.
