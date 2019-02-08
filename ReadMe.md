# YoutubeExplode

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/YoutubeExplode/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/YoutubeExplode/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode)
[![NuGet](https://img.shields.io/nuget/v/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![NuGet](https://img.shields.io/nuget/dt/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)

YoutubeExplode is a library that provides an interface to query metadata of YouTube videos, playlists and channels, as well as to resolve and download video streams and closed caption tracks. Behind a layer of abstraction, the library parses raw page content and uses reverse-engineered AJAX requests to retrieve information. As it doesn't use the official API, there's also no need for an API key and there are no usage quotas.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode): `Install-Package YoutubeExplode`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode)

## Features

- Retrieves information about videos, playlists, channels, media streams and closed caption tracks
- Handles all types of videos, including legacy, signed, restricted, non-embeddable and unlisted videos
- Works with media streams of all types -- muxed, embedded adaptive, dash adaptive
- Downloads videos by exposing their media content as a stream
- Supports media stream seeking and segmentation to circumvent throttling
- Parses and downloads closed caption tracks
- All metadata properties are exposed using strong types and enums
- Provides static methods to validate IDs and to parse IDs from URLs
- Fully asynchronous API
- Targets .NET Framework 4.5+, .NET Core 1.0+ and .NET Standard 1.1+
- No need for an API key and no usage quotas

## Screenshots

![](http://www.tyrrrz.me/Projects/YoutubeExplode/Images/1.png)

## Usage

YoutubeExplode has a single entry point, the `YoutubeClient` class -- all available integration API can be accessed by calling methods of this class.

A lot of helper methods are provided as extensions for models, make sure to include corresponding `using` statements to see them.

Media streams come in 3 forms -- `Muxed` (video & audio), `Audio` (audio only) and `Video` (video only). Highest qualities are not available in muxed streams so you'll have to download separate streams and multiplex them yourself using tools like [ffmpeg](https://www.ffmpeg.org/).

You can also use [YoutubeExplode.Converter](https://github.com/Tyrrrz/YoutubeExplode.Converter) to take care of multiplexing for you.

### Parse ID from URL

```c#
var url = "https://www.youtube.com/watch?v=bnsUkE8i0tU";
var id = YoutubeClient.ParseVideoId(url); // "bnsUkE8i0tU"
```

### Get video info

```c#
var client = new YoutubeClient();
var video = await client.GetVideoAsync("bnsUkE8i0tU");

var title = video.Title; // "Infected Mushroom - Spitfire [Monstercat Release]"
var author = video.Author; // "Monstercat"
var duration = video.Duration; // 00:07:14
```

### Download video

```c#
var client = new YoutubeClient();
var streamInfoSet = await client.GetVideoMediaStreamInfosAsync("bnsUkE8i0tU");

var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
var ext = streamInfo.Container.GetFileExtension();
await client.DownloadMediaStreamAsync(streamInfo, $"downloaded_video.{ext}");
```

### Extract closed captions

```c#
var client = new YoutubeClient();
var trackInfos = await client.GetVideoClosedCaptionTrackInfosAsync("_QdPW8JrYzQ");

var trackInfo = trackInfos.First(t => t.Language.Code == "en");
var track = await client.GetClosedCaptionTrackAsync(trackInfo);

var caption = track.GetByTime(TimeSpan.FromSeconds(61));
var text = caption.Text; // "And the game was afoot."
```

### Get playlist info

```c#
var client = new YoutubeClient();
var playlist = await client.GetPlaylistAsync("PLQLqnnnfa_fAkUmMFw5xh8Kv0S5voEjC9");

var title = playlist.Title; // "Igorrr - Hallelujah"
var author = playlist.Author; // "randomusername604"

var video = playlist.Videos.First();
var videoTitle = video.Title; // "Igorrr - Tout Petit Moineau"
var videoAuthor = video.Author; // "Igorrr Official"
```

## Libraries used

- [AngleSharp](https://github.com/AngleSharp/AngleSharp)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [GalaSoft.MVVMLight](http://www.mvvmlight.net)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [NUnit](https://github.com/nunit/nunit)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)