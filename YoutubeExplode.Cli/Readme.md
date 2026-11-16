# YoutubeExplode.Cli

[![Version](https://img.shields.io/nuget/v/YoutubeExplode.Cli.svg)](https://nuget.org/packages/YoutubeExplode.Cli)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.Cli.svg)](https://nuget.org/packages/YoutubeExplode.Cli)

**YoutubeExplode.Cli** is a command-line wrapper for **YoutubeExplode** that exposes the library's API through terminal commands.
It's a low-level alternative to tools like `yt-dlp`, which can be used either as a standalone downloader or as a probing/debugging tool.

## Install

- 📦 [NuGet](https://nuget.org/packages/YoutubeExplode.Cli): `dotnet tool install --global YoutubeExplode.Cli`

> [!IMPORTANT]
> The `videos download` command relies on [FFmpeg](https://ffmpeg.org/download.html) to mux streams together.
> Ensure that `ffmpeg` is available on the `PATH`, or specify its location using the `--ffmpeg-path` option or the `FFMPEG_PATH` environment variable (both a path to the executable and a path to its directory are accepted).

## Usage

Commands that return an object write it as JSON to the standard output, and commands that accept an object read it as JSON from the standard input.
Progress and other diagnostic messages are written to the standard error, so they never interfere with piped data.

```
youtubeexplode videos get <id-or-url>
youtubeexplode videos download <id-or-url> [--output <path>] [--container <name>] [--preset <preset>] [--ffmpeg-path <path>]

youtubeexplode videos streams get-manifest <id-or-url>
youtubeexplode videos streams get [url]
youtubeexplode videos streams download [url] --output <path>

youtubeexplode videos closed-captions get-manifest <id-or-url>
youtubeexplode videos closed-captions get [url]
youtubeexplode videos closed-captions download [url] --output <path>

youtubeexplode playlists get <id-or-url>
youtubeexplode playlists get-videos <id-or-url>

youtubeexplode channels get <id-or-url> [--kind <Id|UserName|Slug|Handle>]

youtubeexplode search get-results <query>
youtubeexplode search get-videos <query>
youtubeexplode search get-playlists <query>
youtubeexplode search get-channels <query>
```

Use `--help` on any command to see its full list of options.

### Downloading a video

```bash
youtubeexplode videos download "https://youtube.com/watch?v=u_yIGGhubZs" --output "video.mp4"
```

### Working with streams

The stream commands accept either a stream URL, or a stream info JSON object (as returned by `videos streams get-manifest`) on the standard input.
Note that a URL alone is only enough if it contains the content length (`clen`) parameter, which is the case for URLs provided by YouTube.

```bash
# Download the highest-bitrate audio-only stream and pipe its binary data to a file
youtubeexplode videos streams get-manifest u_yIGGhubZs \
    | jq -c '[.[] | select(has("audioCodec") and (has("videoCodec") | not))] | max_by(.bitrate.bitsPerSecond)' \
    | youtubeexplode videos streams get > audio.bin
```

### Processing multiple items

Commands that return lists produce a single JSON array, so they can be combined with tools like `jq` to process many items.
For example, to download all videos in a playlist:

```bash
youtubeexplode playlists get-videos "PLxxxxxx" | jq -r '.[].id' | while read -r id; do
    youtubeexplode videos download "$id" --output "downloads/$id.mp4"
done
```
