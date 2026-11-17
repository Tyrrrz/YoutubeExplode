using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace YoutubeExplode.Cli.Commands.Channels;

[Command("channels", Description = "Commands for working with channels.")]
public partial class ChannelsCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Please specify a subcommand.", showHelp: true);
}
