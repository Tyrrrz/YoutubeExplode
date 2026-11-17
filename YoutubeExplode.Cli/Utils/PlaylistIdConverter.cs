using YoutubeExplode.Playlists;

namespace YoutubeExplode.Cli.Utils;

internal class PlaylistIdConverter() : IdentifierConverter<PlaylistId>(PlaylistId.Parse);
