# YoutubeExplode.Cli

[![Version](https://img.shields.io/nuget/v/YoutubeExplode.Cli.svg)](https://nuget.org/packages/YoutubeExplode.Cli)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.Cli.svg)](https://nuget.org/packages/YoutubeExplode.Cli)

**YoutubeExplode.Cli** is a command-line wrapper for **YoutubeExplode** that exposes the library's API through terminal commands.

## Install

- 📦 [NuGet](https://nuget.org/packages/YoutubeExplode.Cli): `dotnet tool install --global YoutubeExplode.Cli`

## Usage

Commands that return an object write it as JSON to the standard output, and commands that accept an object read it as JSON from the standard input.
Progress and other diagnostic messages are written to the standard error, so they never interfere with piped data.

```
yte video get <id-or-url>
yte video download <id-or-url> [--output <path>] [--container <name>] [--preset <preset>] [--ffmpeg-path <path>]

yte video stream manifest <id-or-url>
yte video stream get
yte video stream download --output <path>

yte video cc manifest <id-or-url>
yte video cc get
yte video cc download --output <path>

yte playlist get <id-or-url>
yte playlist videos <id-or-url>

yte channel get <id-or-url> [--kind <Id|UserName|Slug|Handle>]

yte search <query> [--filter <None|Videos|Playlists|Channels>]
```

Use `--help` on any command to see its full list of options.

### Downloading a video

```bash
yte video download "https://youtube.com/watch?v=u_yIGGhubZs" --output "video.mp4"
```

### Working with streams

The stream commands read a stream info JSON object (as returned by `video stream manifest`) from the standard input.

```bash
# Download the highest-bitrate audio-only stream and pipe its binary data to a file
yte video stream manifest u_yIGGhubZs \
    | jq -c '[.streams[] | select(has("audioCodec") and (has("videoCodec") | not))] | max_by(.bitrate.bitsPerSecond)' \
    | yte video stream get > audio.bin
```

### Processing multiple items

Commands that return lists produce a single JSON array, so they can be combined with tools like `jq` to process many items.
For example, to download all videos in a playlist:

```bash
yte playlist videos "PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6" | jq -r '.[].id' | while read -r id; do
    yte video download "$id" --output "downloads/$id.mp4"
done
```
