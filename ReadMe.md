# YoutubeExplode

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/YoutubeExplode/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/YoutubeExplode/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode)
[![NuGet](https://img.shields.io/nuget/v/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)
[![NuGet](https://img.shields.io/nuget/dt/YoutubeExplode.svg)](https://nuget.org/packages/YoutubeExplode)

YoutubeExplode is a library that provides an interface to query metadata of YouTube videos, playlists and channels, as well as to resolve and download video streams and closed caption tracks. Behind a layer of abstraction, the library parses raw page content and uses reverse-engineered AJAX requests to retrieve information. As it doesn't use the official API, there's also no need for an API key and there are no usage quotas.

## Screenshots

![](http://www.tyrrrz.me/Projects/YoutubeExplode/Images/1.png)

## Download

- Using NuGet: `Install-Package YoutubeExplode`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode)

## Features

- Retrieves information about channels, videos, playlists, media streams and closed caption tracks
- Handles normal, legacy, signed, restricted, non-embeddable and unlisted videos
- Works with media streams of all types - muxed, embedded adaptive, dash adaptive
- Downloads videos by exposing their media content as a stream
- Parses and downloads closed caption tracks
- Uses strong types and enums for all metadata
- Provides methods to validate IDs and to parse IDs from URLs
- Fully asynchronous API
- Targets .NET Framework 4.5+, .NET Core 1.0+ and .NET Standard 1.1+
- No need for an API key and no usage quotas

## Usage

Check out project's [wiki](https://github.com/Tyrrrz/YoutubeExplode/wiki) for usage examples and other information.

You can also use the demo projects as a reference point.

## Libraries used

- [AngleSharp](https://github.com/AngleSharp/AngleSharp)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [GalaSoft.MVVMLight](http://www.mvvmlight.net)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
- [Tyrrrz.WpfExtensions](https://github.com/Tyrrrz/WpfExtensions)
