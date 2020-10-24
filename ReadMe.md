# YoutubeExplode

[![Build](https://github.com/Tyrrrz/YoutubeExplode/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/YoutubeExplode/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/YoutubeExplode/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/YoutubeExplode)
[![Version](https://img.shields.io/nuget/v/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)
[![Extra Services](https://img.shields.io/badge/extra%20services-xs:code-blue.svg)](https://xscode.com/Tyrrrz/YoutubeExplode)

**Project status: maintenance mode** (bug fixes only).

YoutubeExplode is a library that provides an interface to query metadata of YouTube videos, playlists and channels, as well as to resolve and download video streams and closed caption tracks. Behind a layer of abstraction, the library parses raw page content and uses reverse-engineered AJAX requests to retrieve information. As it doesn't use the official API, there's also no need for an API key and there are no usage quotas.

This library is used in [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader), a desktop application for downloading and converting YouTube videos.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode): `dotnet add package YoutubeExplode`

## Features

- Retrieve metadata on videos, playlists, channels, streams, and closed captions
- Execute search queries and get resulting videos
- Get or download video streams, with support for seeking
- Get closed captions or download them as SRT files
- Works with .NET Standard 2.0+, .NET Core 2.0+, .NET Framework 4.6.1+

## Screenshots

![demo](.screenshots/demo.png)

## Usage

- [Getting metadata of a video](#getting-metadata-of-a-video)
- [Downloading a video stream](#downloading-a-video-stream)
- [Working with playlists](#working-with-playlists)
- [Extracting closed captions](#extracting-closed-captions)

### Getting metadata of a video

The following example shows how you can extract various metadata from a YouTube video:

```csharp
var youtube = new YoutubeClient();

// You can specify video ID or URL
var video = await youtube.Videos.GetAsync("https://youtube.com/watch?v=bnsUkE8i0tU");

var title = video.Title; // "Infected Mushroom - Spitfire [Monstercat Release]"
var author = video.Author; // "Monstercat"
var duration = video.Duration; // 00:07:14
```

### Downloading a video stream

Every YouTube video has a number of streams available. These streams may have different containers, video quality, bitrate, etc.

On top of that, depending on the content of the stream, the streams are further divided into 3 categories:

- Muxed streams -- contain both video and audio
- Audio-only streams -- contain only audio
- Video-only streams -- contain only video

You can request the stream manifest to get available streams for a particular video:

```csharp
var youtube = new YoutubeClient();

var streamManifest = await youtube.Videos.Streams.GetManifestAsync("bnsUkE8i0tU");
```

Once you get the manifest, you can filter through the streams and choose the one you're interested in downloading:

```csharp
// Get highest quality muxed stream
var streamInfo = streamManifest.GetMuxed().WithHighestVideoQuality();

// ...or highest bitrate audio-only stream
var streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();

// ...or highest quality MP4 video-only stream
var streamInfo = streamManifest
    .GetVideoOnly()
    .Where(s => s.Container == Container.Mp4)
    .WithHighestVideoQuality()
```

Finally, you can get the actual `Stream` object represented by the metadata:

```csharp
if (streamInfo != null)
{
    // Get the actual stream
    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

    // Download the stream to file
    await youtube.Videos.Streams.DownloadAsync(streamInfo, $"video.{streamInfo.Container}");
}
```

While it may be tempting to just always use muxed streams, it's important to note that they are limited in quality.
Muxed streams don't go beyond 720p30.

If you want to download the video in maximum quality, you need to download the audio-only and video-only streams separately and then mux them together on your own.
There are tools like [FFmpeg](http://ffmpeg.org/) that let you do that.
You can also use [YoutubeExplode.Converter](https://github.com/Tyrrrz/YoutubeExplode.Converter) which wraps FFmpeg and provides an extension point for YoutubeExplode to download videos directly.

### Working with playlists

Among other things, YoutubeExplode also supports playlists:

```csharp
var youtube = new YoutubeClient();

// Get playlist metadata
var playlist = await youtube.Playlists.GetAsync("PLQLqnnnfa_fAkUmMFw5xh8Kv0S5voEjC9");

var title = playlist.Title; // "Igorrr - Hallelujah"
var author = playlist.Author; // "randomusername604"

// Enumerate through playlist videos
await foreach (var video in youtube.Playlists.GetVideosAsync(playlist.Id))
{
    var videoTitle = video.Title;
    var videoAuthor = video.Author;
}

// Get all playlist videos
var playlistVideos = await youtube.Playlists.GetVideosAsync(playlist.Id);

// Get first 20 playlist videos
var somePlaylistVideos = await youtube.Playlists
    .GetVideosAsync(playlist.Id)
    .BufferAsync(20);
```

### Extracting closed captions

Similarly to streams, you can extract closed captions by getting the manifest and choosing the track you're interested in:

```csharp
var youtube = new YoutubeClient();

var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync("_QdPW8JrYzQ");

// Select a closed caption track in English
var trackInfo = trackManifest.TryGetByLanguage("en");

if (trackInfo != null)
{
    // Get the actual closed caption track
    var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

    // Get the caption displayed at 1:01
    var caption = track.TryGetByTime(TimeSpan.FromSeconds(61));
    var text = caption?.Text; // "And the game was afoot."
}
```

You can also download closed caption tracks as SRT files:

```csharp
var trackInfo = trackManifest.TryGetByLanguage("en");

if (trackInfo != null)
{
    await youtube.Videos.ClosedCaptions.DownloadAsync(trackInfo, "cc_track.srt");
}
```

## Etymology

The "Explode" in YoutubeExplode comes from the name of a PHP function that splits up strings, [`explode()`](https://www.php.net/manual/en/function.explode.php). When I was just starting development on this library, most of the reference source code I read was written in PHP, hence the inspiration for the name.