using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace YoutubeExplode.Cli.Commands.Videos.ClosedCaptions;

[Command("video cc", Description = "Commands for working with video closed captions.")]
public partial class ClosedCaptionCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Please specify a subcommand.", showHelp: true);
}
