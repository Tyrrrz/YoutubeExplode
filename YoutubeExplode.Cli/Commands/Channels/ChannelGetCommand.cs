using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Channels;
using YoutubeExplode.Cli.Commands.Base;

namespace YoutubeExplode.Cli.Commands.Channels;

[Command("channel get", Description = "Gets the metadata of a channel.")]
public partial class ChannelGetCommand : CommandBase
{
    [CommandParameter(
        0,
        Name = "id-or-url",
        Description = "Channel ID, user name, slug, handle, or URL."
    )]
    public required string ChannelIdOrUrl { get; set; }

    [CommandOption(
        "kind",
        'k',
        Description = "Kind of the channel identifier (Id, UserName, Slug, Handle). "
            + "Detected automatically if not specified."
    )]
    public ChannelKind? Kind { get; set; }

    private ChannelKind DetectKind() =>
        ChannelId.TryParse(ChannelIdOrUrl) is not null ? ChannelKind.Id
        : ChannelHandle.TryParse(ChannelIdOrUrl) is not null ? ChannelKind.Handle
        : UserName.TryParse(ChannelIdOrUrl) is not null ? ChannelKind.UserName
        : ChannelSlug.TryParse(ChannelIdOrUrl) is not null ? ChannelKind.Slug
        : throw new CommandException(
            $"Cannot detect the kind of channel identifier '{ChannelIdOrUrl}'."
        );

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            (Kind ?? DetectKind()) switch
            {
                ChannelKind.Id => await youtube.Channels.GetAsync(
                    ChannelId.Parse(ChannelIdOrUrl),
                    cancellationToken
                ),
                ChannelKind.UserName => await youtube.Channels.GetByUserAsync(
                    UserName.Parse(ChannelIdOrUrl),
                    cancellationToken
                ),
                ChannelKind.Slug => await youtube.Channels.GetBySlugAsync(
                    ChannelSlug.Parse(ChannelIdOrUrl),
                    cancellationToken
                ),
                ChannelKind.Handle => await youtube.Channels.GetByHandleAsync(
                    ChannelHandle.Parse(ChannelIdOrUrl),
                    cancellationToken
                ),
                _ => throw new InvalidOperationException(),
            }
        );
}
