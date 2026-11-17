using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace YoutubeExplode.Cli.Commands.Videos;

[Command("videos", Description = "Commands for working with videos.")]
public partial class VideosCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Please specify a subcommand.", showHelp: true);
}
