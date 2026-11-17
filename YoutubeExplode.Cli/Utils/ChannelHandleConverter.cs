using YoutubeExplode.Channels;

namespace YoutubeExplode.Cli.Utils;

internal class ChannelHandleConverter() : IdentifierConverter<ChannelHandle>(ChannelHandle.Parse);
