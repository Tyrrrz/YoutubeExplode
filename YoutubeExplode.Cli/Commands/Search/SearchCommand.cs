using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace YoutubeExplode.Cli.Commands.Search;

[Command("search", Description = "Commands for searching YouTube.")]
public partial class SearchCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Please specify a subcommand.", showHelp: true);
}
