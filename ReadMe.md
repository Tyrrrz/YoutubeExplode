YoutubeExplode
===================


Zero-dependency .NET library that parses public metadata on Youtube videos

**Screenshots:**

![](http://www.tyrrrz.me/projects/images/ytexplode_1.png)

**Download:**

- Using nuget: `Install-Package YoutubeExplode`
- Manually: [go to release page](https://github.com/Tyrrrz/YoutubeExplode/releases)

**Features:**

- Retrieves information on videos, playlists, media streams and closed caption tracks
- Handles normal, legacy, signed, restricted, non-embeddable and unlisted videos
- Works with media streams of all types: mixed, embedded adaptive, dash adaptive
- Downloads videos by exposing the media content as a stream
- Downloads and parses closed caption tracks
- Strong types and enums for everything
- Static methods to validate IDs and to parse them from URLs
- No need for an API key and no usage quotas
- Async/await all the way
- Optimized for best performance
- Service injection via constructor
- Comprehensive XML documentation
- Targets [.NET Standard 1.1](https://github.com/dotnet/standard/blob/master/docs/versions.md)

**Usage example:**

Check out project's [wiki](https://github.com/Tyrrrz/YoutubeExplode/wiki) for usage examples and other information

**Libraries used:**

- [GalaSoft.MVVMLight](http://www.mvvmlight.net)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
- [Tyrrrz.WpfExtensions](https://github.com/Tyrrrz/WpfExtensions)
 