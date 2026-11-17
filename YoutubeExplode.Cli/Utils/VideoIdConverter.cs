using YoutubeExplode.Videos;

namespace YoutubeExplode.Cli.Utils;

internal class VideoIdConverter() : IdentifierConverter<VideoId>(VideoId.Parse);
