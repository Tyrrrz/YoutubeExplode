using System.Collections.Generic;
using System.Text.Json.Serialization;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Utils;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    Converters = [
        typeof(VideoIdConverter),
        typeof(PlaylistIdConverter),
        typeof(ChannelIdConverter),
        typeof(UserNameConverter),
        typeof(ChannelSlugConverter),
        typeof(ChannelHandleConverter),
        typeof(StreamInfoConverter),
        typeof(SearchResultConverter),
    ]
)]
[JsonSerializable(typeof(Video))]
[JsonSerializable(typeof(Playlist))]
[JsonSerializable(typeof(Channel))]
[JsonSerializable(typeof(ClosedCaptionTrackInfo))]
[JsonSerializable(typeof(ClosedCaptionTrack))]
[JsonSerializable(typeof(ClosedCaptionManifest))]
[JsonSerializable(typeof(StreamManifest))]
[JsonSerializable(typeof(AudioOnlyStreamInfo))]
[JsonSerializable(typeof(VideoOnlyStreamInfo))]
[JsonSerializable(typeof(MuxedStreamInfo))]
[JsonSerializable(typeof(VideoSearchResult))]
[JsonSerializable(typeof(PlaylistSearchResult))]
[JsonSerializable(typeof(ChannelSearchResult))]
[JsonSerializable(typeof(IReadOnlyList<ISearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<VideoSearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<PlaylistSearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<ChannelSearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<PlaylistVideo>))]
internal partial class JsonSerializerContext : System.Text.Json.Serialization.JsonSerializerContext;
