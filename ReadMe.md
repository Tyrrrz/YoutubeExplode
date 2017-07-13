# YoutubeExplode

Library that lets you query various Youtube metadata, unwrap playlists, download videos, extract closed captions and much more.
Does not require an API key and as such does not impose any usage quotas.

## Screenshots

![](http://www.tyrrrz.me/projects/images/ytexplode_1.png)

## Download

- Using nuget: `Install-Package YoutubeExplode`
- Demo: [go to release page](https://github.com/Tyrrrz/YoutubeExplode/releases)

## Features

- Retrieves information on channels, videos, playlists, media streams and closed caption tracks
- Handles normal, legacy, signed, restricted, non-embeddable and unlisted videos
- Works with media streams of all types: mixed, embedded adaptive, dash adaptive
- Downloads videos by exposing their media content as a stream
- Extracts and parses closed caption tracks
- Strong types and enums for everything
- Static methods to validate IDs and to parse IDs from URLs
- No need for an API key and no usage quotas
- Fully asynchronous
- Targets all .NET platforms
- No external dependencies

## Usage

Check out project's [wiki](https://github.com/Tyrrrz/YoutubeExplode/wiki) for usage examples and other information.

You can also use the demo projects as a reference point.

## Libraries used

- [GalaSoft.MVVMLight](http://www.mvvmlight.net)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
- [Tyrrrz.WpfExtensions](https://github.com/Tyrrrz/WpfExtensions)
