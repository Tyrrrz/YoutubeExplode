# YoutubeExplode.Converter

[![Version](https://img.shields.io/nuget/v/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode.Converter)

**YoutubeExplode.Converter** is an extension package for **YoutubeExplode** that provides the capability to download YouTube videos by muxing separate streams into a single file.
This package relies on [FFmpeg](https://ffmpeg.org) under the hood.

## Install

- 📦 [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `dotnet add package YoutubeExplode.Converter`

## Usage

**YoutubeExplode.Converter** exposes its functionality by enhancing **YoutubeExplode**'s clients with additional extension methods.
To use them, simply add the corresponding namespace and follow the examples below.

> **Warning**:
> This package requires the [FFmpeg](https://ffmpeg.org) executable to work, which can be downloaded [here](https://ffbinaries.com/downloads).
> Ensure that it's located in your application's probe directory or on the system's `PATH`, or provide a custom location directly using various overloads.

### Downloading videos

You can download a video directly through one of the extension methods provided on `VideoClient`.
For example, to download a video in the specified format using the highest quality streams, simply call `DownloadAsync(...)` with the video ID and the destination path:

```csharp
using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();

var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";
await youtube.Videos.DownloadAsync(videoUrl, "video.mp4");
```

Under the hood, this resolves the video's media streams, downloads the best candidates based on format, bitrate, framerate, and quality, and muxes them together into a single file.

> **Note**:
> If the specified output format is a known audio-only container (e.g. `mp3` or `ogg`) then only the audio stream is downloaded.

> **Warning**:
> Stream muxing is a resource-intensive process.
> You can improve the execution speed by making sure that both the input streams and the output file use the same format, which eliminates the need for transcoding.
> Currently, YouTube provides adaptive streams only in `mp4` and `webm` containers, with the highest quality video streams (e.g. 4K) only available in `webm`.

### Customizing the conversion process

To configure various aspects of the conversion process, use the following overload of `DownloadAsync(...)`:

```csharp
using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();
var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";

await youtube.Videos.DownloadAsync(videoUrl, "video.mp4", o => o
    .SetContainer("webm") // override format
    .SetPreset(ConversionPreset.UltraFast) // change preset
    .SetFFmpegPath("path/to/ffmpeg") // custom FFmpeg location
);
```

### Manually selecting streams

If you need precise control over which streams are used for the muxing process, you can also provide them yourself instead of relying on the automatic resolution:

```csharp
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();

// Get stream manifest
var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";
var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

// Select streams (1080p60 / highest bitrate audio)
var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
var videoStreamInfo = streamManifest.GetVideoStreams().First(s => s.VideoQuality.Label == "1080p60");
var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

// Download and process them into one file
await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder("video.mp4").Build());
```