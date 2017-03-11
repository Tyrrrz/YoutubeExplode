YoutubeExplode
===================


Zero-dependency .NET library that parses public metadata on Youtube videos


**Download:**

The library is distributed as a [nuget package](https://www.nuget.org/packages/YoutubeExplode): `Install-Package YoutubeExplode`

You can also find the last stable version in [releases](https://github.com/Tyrrrz/YoutubeExplode/releases)

**Features:**

- Retrieves video info, media stream info, closed caption track info
- Handles signed and restricted videos
- Works with media streams of all types: mixed, adaptive, dash
- Extracts video IDs from playlists
- Allows to check if the video exists
- Lets user download media streams and caption tracks
- Metadata is exposed using enums and other strong types
- Static methods to validate IDs and to parse them from URLs
- Asynchronous API
- Optimized for best performance
- Service injection via constructor
- Comprehensive XML documentation
- Supports most platforms via .NET Standard 1.1 ([reference](https://github.com/dotnet/standard/blob/master/docs/versions.md))

**Usage example:**

Check out project's [wiki](https://github.com/Tyrrrz/YoutubeExplode/wiki) for usage examples and other information

**Libraries used:**

Console demo:

- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions) - my set of various extensions for rapid development

Wpf demo:

- [GalaSoft.MVVMLight](http://www.mvvmlight.net) - MVVM rapid development
- [MaterialDesignXAML](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit) - MaterialDesign UI
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions) - my set of various extensions for rapid development
- [Tyrrrz.WpfExtensions](https://github.com/Tyrrrz/WpfExtensions) - my set of various WPF extensions for rapid development
 
**Screenshots:**

![](http://www.tyrrrz.me/projects/images/ytexplode_1.png)
![](http://www.tyrrrz.me/projects/images/ytexplode_2.png)
