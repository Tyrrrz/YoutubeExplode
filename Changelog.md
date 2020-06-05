### v5.0.5 (23-May-2020)

- Fixed an issue where sometimes the content length of a stream was equal to `1` due to an error in parsing.
- Fixed an issue where an exception was thrown on videos that contained unplayable media streams. These streams are now ignored.
- Fixed an issue where trying to get stream manifest on 360° videos resulted in an exception.
- Fixed an issue where `Engagement.ToString()` was incorrectly formatting likes and dislikes. (Thanks [@bcook254](https://github.com/bcook254))

### v5.0.4 (10-May-2020)

- Fixed an issue where the search query was not correctly escaped in `SearchClient`. (Thanks [@Calle](https://github.com/calledude))
- Fixed an issue where an exception "The given key was not present in the dictionary" was thrown when trying to get streams for some videos, due to recent YouTube changes.

### v5.0.3 (01-May-2020)

- Fixed an issue where streams couldn't be extracted for some videos.

### v5.0.2 (24-Apr-2020)

- Added `TryParse` static method to `ChannelId`, `UserName`, `PlaylistId`, `VideoId` objects.
- Added an extension method to make it simpler to buffer an asynchronous list of videos in-memory. You can now do `var videos = await youtube.Playlist.GetVideosAsync(...)` on top of enumerating it with `await foreach`. The readme has been updated with new usage examples.
- Simplified exception messages.

### v5.0.1 (13-Apr-2020)

- Extended `ClosedCaption` with `Parts` property that contains separate parts of a caption, along with their individual timings. Note that not all tracks contain this information.
- Fixed an issue where searching for videos sometimes failed with an exception.
- Added missing operators for `FileSize`, `Bitrate`, `Framerate`, `VideoResolution`, `Container`, `Language`, `VideoId`, `PlaylistId`, `ChannelId`, `UserName`.

### v5.0 (12-Apr-2020)

- **Reworked the entire library from the ground up.**
- Video, playlist, channel IDs and usernames are now encapsulated in corresponding domain objects. This means that you no longer have to parse IDs manually -- e.g. if a method accepts a parameter of type `VideoId`, you can either specify an ID (`bnsUkE8i0tU`) or a URL (`https://youtube.com/watch?v=bnsUkE8i0tU`).
- Playlist videos and search results are now returned as `IAsyncEnumerable` so you can enumerate through them without worrying about making too many or too few requests. If you want to buffer them in-memory, you can use an extension method called `BufferAsync()`.
- Improved exceptions, exception messages, and everything related to exceptions. Additionally, all exception types now derive from `YoutubeExplodeException`, making them easier to catch.
- Added built-in retry mechanisms to work around transient errors on YouTube's side.
- Improved resilience of the library in general.
- Fixed an issue where attempts to download some videos were periodically causing 403 Forbidden.
- Fixed a metric ton of YouTube-related issues.
- Many, many others improvements that I didn't think to mention.
- Dropped .NET Framework v4.5 target in favor of v4.6.1 and .NET Standard v1.1 target in favor of v2.0.

### v4.7.16 (16-Mar-2020)

- Fixed an issue where attempts to download some videos were periodically causing 403 Forbidden.

### v4.7.15 (11-Mar-2020)

- Fixed some issues revolving around videos marked with "content warning".

### v4.7.14 (10-Mar-2020)

- Fixed an issue where `GetMediaStreamAsync` and `DownloadMediaStreamAsync` threw an exception due to recent YouTube changes. As a side effect, age-restricted videos may no longer work, at least until a new workaround is found.

### v4.7.13 (10-Feb-2020)

- Fixed an issue where `GetPlaylistAsync` only returned 200 videos for some larger playlists. Thanks [@polynoman](https://github.com/polynoman).

### v4.7.12 (29-Dec-2019)

- Fixed an issue where some playlist IDs were incorrectly considered invalid.

### v4.7.11 (15-Dec-2019)

- Fixed an issue where `GetVideoMediaStreamInfosAsync` sometimes returned a set without any streams due to recent YouTube changes.
- Fixed an issue where "my mix" playlists were considered invalid.
- Added nullable reference type annotations and removed ReSharper annotations.

### v4.7.10 (23-Sep-2019)

- Fixed an issue where `GetVideoMediaStreamInfosAsync` threw an exception due to recent YouTube changes.

### v4.7.9 (15-Aug-2019)

- Updated signature deciphering to match recent YouTube changes.

### v4.7.8 (08-Aug-2019)

- Fixed an issue where `UploadDate` was incorrect on videos returned from `GetPlaylistAsync`, `SearchVideosAsync` and `GetChannelUploadsAsync`.

### v4.7.7 (30-Jul-2019)

- Fixed an issue where most methods threw `VideoUnavailableException` on all videos due to recent YouTube changes.

### v4.7.6 (10-Jul-2019)

- Fixed an issue where `GetVideoMediaStreamInfosAsync` threw an exception due to recent YouTube changes.
- Fixed how error reason is extracted from the watch page when a video is unavailable.

### v4.7.5 (04-Jul-2019)

- Dropped dependency on AngleSharp and replaced it with LtGt.
- Fixed an issue where `GetChannelUploadsAsync` always returned empty result.

### v4.7.4 (24-Jun-2019)

- Updated how videos in a playlist are resolved to match recent YouTube changes.

### v4.7.3 (21-Jun-2019)

- Updated signature deciphering to match recent YouTube changes.

### v4.7.2 (13-Jun-2019)

- Fixed an issue where `ArgumentException` was thrown on some videos due to recent YouTube changes.
- Improved exception messages to make them slightly more user-friendly.

### v4.7.1 (17-May-2019)

- Improved exception messages for cases when a video is blocked in user's country or is age-restricted and unembeddable.

### v4.7 (12-May-2019)

- Fixed an issue where YoutubeExplode always failed to extract media streams due to recent YouTube changes.

### v4.6.8 (30-Mar-2019)

- Improved performance in `GetVideoAsync` by optimizing description parser.

### v4.6.7 (16-Mar-2019)

- Fixed some more inconsistencies with how links in video descriptions are rendered.

### v4.6.6 (24-Feb-2019)

- Fixed an issue where `JsonReaderException` was thrown when downloading videos that were blocked on copyright grounds.

### v4.6.5 (16-Feb-2019)

- Fixed an issue where parser methods for channel ID and username failed if the URL contained query parameters.
- Fixed some inconsistencies with how links in video descriptions are rendered.

### v4.6.4 (16-Jan-2019)

- Pinned AngleSharp dependency to version 0.9.11 because newer versions contain breaking changes that are currently incompatible with YoutubeExplode.
- Improved the implementation of `GetChannelAsync` so that it's more fast and works on channels without any uploaded videos.

### v4.6.3 (13-Jan-2019)

- Fixed an issue where closed caption tracks were sometimes missing whitespace between words in auto-generated tracks.
- Added an extension method to get all distinct video quality labels from a set -- `MediaStreamInfoSet.GetAllVideoQualityLabels`.

### v4.6.2 (04-Jan-2019)

- Fixed an issue where `GetVideoMediaStreamInfosAsync` returned empty for live stream recording videos.

### v4.6.1 (03-Dec-2018)

- Fixed sporadic failures in `GetVideoAuthorChannelAsync` and `GetChannelIdAsync`.
- Re-added `VideoRequiresPurchaseException` as a child of `VideoUnplayableException`.

### v4.6 (23-Nov-2018)

- Switched majority of video-related parsing to a new approach, which allows circumventing signature deciphering, provides more info, and is marginally faster and more consistent. This makes `GetVideoMediaStreamInfosAsync` complete twice as fast, on average.
- Switched from itag-based property mapping to manual string parsing, which should be more stable in the long run.
- Added `MediaStreamInfoSet.ValidUntil` property which can be used to determine when the contained streams will expire.
- Fixed an issue where controversial videos could not be parsed.
- Removed `User-Agent` header from default request headers.
- Removed `VideoQuality.GetVideoQualityLabel` extension method.
- Removed `MediaStreamInfo.GetUrlExpiryDate` extension method.
- Removed `VideoRequiresPurchaseException` and replaced it with `VideoUnplayableException` which covers a wider spectrum of errors.
- `VideoUnavailableException` no longer has properties for error code and error reason. Error code was basically useless so it was removed, error reason is now part of the `Message` property.
- Removed `ParseException` entirely.
- Some enum values in `AudioEncoding`, `VideoEncoding` and `Container` types were marked as obsolete because they are no longer used by YouTube.

### v4.5.3 (07-Nov-2018)

- Fixed an issue where signature decipherer was throwing an exception due to recent YouTube changes.

### v4.5.2 (02-Nov-2018)

- Fixed an issue where `GetVideoAsync` was throwing an exception due to recent YouTube changes.

### v4.5.1 (24-Oct-2018)

- Fixed an issue where `GetVideoMediaStreamInfosAsync` was sometimes returning adaptive streams that were not working. There are very rare cases where it still might happen.

### v4.5 (20-Oct-2018)

- Fixed an issue where external links were truncated in `Video.Description` if they are too long.
- Added support for seeking in `MediaStream`.

### v4.4 (20-Oct-2018)

- Improved `GetVideoAsync`, `GetVideoAuthorChannelAsync` and `GetVideoClosedCaptionTrackInfosAsync` so that they don't fail on unavailable videos.
- Added extra result validation to `GetChannelIdAsync` to verify that the extracted value is indeed a valid channel ID.
- Added static methods to parse and validate YouTube usernames.

### v4.3.4 (16-Oct-2018)

- Fixed an issue where `JsonReaderException` was thrown on all videos due to recent YouTube changes.
- Added support for itag 394.
- Added `GetChannelIdAsync` method that retrieves channel ID from username.
- Added support for OL playlists.

### v4.3.3 (26-Sep-2018)

- Added support for AV1 video codec.

### v4.3.2 (11-Sep-2018)

- Fixed an issue where `ParseException` was thrown on signature-protected videos due to recent YouTube changes.

### v4.3.1 (28-Aug-2018)

- Fixed an issue where retrieving some streams may throw a 403 HTTP error due to recent YouTube changes.

### v4.3 (25-Jul-2018)

- Reworked `GetMediaStreamAsync` so that it implements the workaround for rate-limited streams, which was originally only available in `DownloadMediaStreamAsync`. This is achieved by returning a stream that internally sends multiple segmented requests in a sequence.
- Fixed `MediaStream.ReadAsync` not using the `ReadAsync` of the underlying stream.
- Fixed `GetVideoQualityLabel` so that it displays the framerate as rounded up to the nearest 10, instead of always displaying it as '60'.

### v4.2.8 (14-Jun-2018)

- Fixed an issue where some non-embeddable videos could not be processed.

### v4.2.7 (09-Jun-2018)

- Fixed an issue where non-embeddable age-restricted videos could not be processed.
- Fixed exception messages not being shown in Visual Studio's exception popup.
- Fixed xml docs on `Playlist.Type` property.

### v4.2.6 (09-Jun-2018)

- Fixed an issue where `VideoUnavailableException` would always be thrown for non-embeddable videos due to recent YouTube changes.

### v4.2.5 (20-May-2018)

- Relaxed validation rules for all playlist IDs because there are even more variations than expected.

### v4.2.4 (02-May-2018)

- Fixed an issue where `GetVideoAuthorChannelAsync` would always throw an exception due to recent YouTube changes.

### v4.2.3 (22-Apr-2018)

- Added an extension method to parse `MediaStreamInfo.Url` expiry date -- `MediaStreamInfo.GetUrlExpiryDate`.
- Replaced instances of `DateTime` with `DateTimeOffset`.
- Relaxed validation rules for RD playlist IDs because there are simply too many possible variations.

### v4.2.2 (31-Mar-2018)

- Fixed some playlist IDs being considered invalid even though they aren't.

### v4.2.1 (28-Mar-2018)

- Fixed an `OutOfMemoryException` issue that would occur when executing `GetVideoMediaStreamInfosAsync` on a large video.

### v4.2 (24-Mar-2018)

- Added overloads for `DownloadMediaStreamAsync` and `DownloadClosedCaptionTrackAsync` that accept `Stream` as output.
- Removed `IHttpService`, `HttpService` in favor of using unwrapped `HttpClient`.
- Added `IYoutubeClient` to aid in testing.

### v4.1.1 (20-Feb-2018)

- Fixed an issue where `GetClosedCaptionTrackAsync` would throw on some malformed automatic captions.
- Fixed an issue where some video qualities were not correctly identified due to itag inconsistency.
- Added support for 2880p video quality.

### v4.1 (02-Feb-2018)

- Implemented segmented downloading for rate-limited media streams. This fixes slow download speed of `DownloadMediaStreamAsync` caused by YouTube changes.
- Added `SearchVideosAsync` method to `YoutubeClient`. Can be used to search for videos using given query. The method returns `Video` objects but not all properties have valid values, due to how this internal API functions.
- Added `HlsLiveStreamUrl` to `MediaStreamInfoSet` which can be used to extract URL of M3U8 playlists for livestream videos.
- Added `UploadDate` to `Video`.
- Fixed incorrect return value in `GetVideoQualityLabel`/`VideoQualityLabel` in cases where FPS was below 60 but above 30.
- Renamed `GetRegularUrl` extension methods to `GetUrl`.
- Added some useful extension methods for models.
- Added some ReSharper annotations.

### v4.0 (15-Dec-2017)

- Reworked `YoutubeClient` API by splitting workflows into separate methods. What was done solely by `GetVideoAsync` is now done by `GetVideoAsync`, `GetVideoMediaStreamInfosAsync`, `GetVideoClosedCaptionTrackInfosAsync`, `GetVideoAuthorChannelAsync`.
- Media stream and closed caption track information is no longer part of the `Video` model. Extended channel information was also removed.
- `PlaylistVideo` has been removed as it now shares the same property set with `Video`. Existing usages of `PlaylistVideo` have been replaced with `Video`.
- Order of some parameters in `Playlist` constructor has been changed. Be careful if you initialize it yourself.
- Removed some properties from `Channel` that are no longer accessible due to YouTube changes.
- Exception messages now provide more information, without needing to check inside properties.
- `ValidatePlaylistId` and `(Try)ParsePlaylistId` are now more strict and check the first two characters in the ID as well.
- Added a lot of useful extensions methods. Refactored some existing methods to extensions.
- Fixed incompatibility with age-restricted videos due to YouTube changes.
- Fixed other issues that prevented the library from being usable due to YouTube changes.
- Added dependency on `Newtonsoft.Json` and `AngleSharp`.

This version has a lot of breaking changes and the migration isn't very straightforward. The readme has been updated with new usage examples and demo projects have been changed to work with new API.