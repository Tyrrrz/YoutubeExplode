using CliFx.Binding;

namespace YoutubeExplode.Cli.Commands.Base;

public abstract partial class SearchCommandBase : CommandBase
{
    [CommandParameter(0, Name = "query", Description = "Search query.")]
    public required string Query { get; set; }
}
