using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single stream, identified either by URL or by JSON from stdin
public abstract partial class StreamCommandBase : CommandBase
{
    [CommandParameter(
        0,
        Name = "url",
        Description = "Stream URL. If omitted, stream info JSON is read from the standard input."
    )]
    public string? StreamUrl { get; set; }

    protected async ValueTask<IStreamInfo> ResolveStreamInfoAsync(
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(StreamUrl))
            return await ReadJsonFromStdinAsync<IStreamInfo>(cancellationToken);

        if (!Uri.TryCreate(StreamUrl, UriKind.Absolute, out var uri))
            throw new CommandException($"'{StreamUrl}' is not a valid URL.");

        // Direct stream URLs carry the content length in the 'clen' query parameter
        foreach (var pair in uri.Query.TrimStart('?').Split('&'))
        {
            var parts = pair.Split('=', 2);
            if (parts is ["clen", var value] && long.TryParse(value, out var size))
            {
                return new AudioOnlyStreamInfo(
                    StreamUrl,
                    new Container("unknown"),
                    new FileSize(size),
                    new Bitrate(0),
                    "unknown",
                    null,
                    null
                );
            }
        }

        throw new CommandException(
            "Cannot determine the stream size from the URL. "
                + "Provide the stream info JSON through the standard input instead."
        );
    }
}
