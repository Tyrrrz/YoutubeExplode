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