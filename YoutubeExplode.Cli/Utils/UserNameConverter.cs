using YoutubeExplode.Channels;

namespace YoutubeExplode.Cli.Utils;

internal class UserNameConverter() : IdentifierConverter<UserName>(UserName.Parse);
