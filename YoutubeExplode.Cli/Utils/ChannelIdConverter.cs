using YoutubeExplode.Channels;

namespace YoutubeExplode.Cli.Utils;

internal class ChannelIdConverter() : IdentifierConverter<ChannelId>(ChannelId.Parse);
