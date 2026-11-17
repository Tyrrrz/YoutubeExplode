using YoutubeExplode.Channels;

namespace YoutubeExplode.Cli.Utils;

internal class ChannelSlugConverter() : IdentifierConverter<ChannelSlug>(ChannelSlug.Parse);
