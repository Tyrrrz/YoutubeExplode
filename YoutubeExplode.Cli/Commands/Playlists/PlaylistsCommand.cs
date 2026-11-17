using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace YoutubeExplode.Cli.Commands.Playlists;

[Command("playlists", Description = "Commands for working with playlists.")]
public partial class PlaylistsCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException(
            "Please specify a subcommand. Use --help to see the list of available subcommands.",
            showHelp: true
        );
}
