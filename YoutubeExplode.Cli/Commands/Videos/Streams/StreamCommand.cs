using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace YoutubeExplode.Cli.Commands.Videos.Streams;

[Command("video stream", Description = "Commands for working with video streams.")]
public partial class StreamCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Please specify a subcommand.", showHelp: true);
}
