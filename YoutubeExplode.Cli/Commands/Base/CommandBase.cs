using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Utils;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Cli.Commands.Base;

public abstract partial class CommandBase : ICommand
{
    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        try
        {
            using var youtube = new YoutubeClient();
            await ExecuteAsync(console, youtube, cancellationToken);
        }
        catch (YoutubeExplodeException ex)
        {
            throw new CommandException(ex.Message, innerException: ex);
        }
        catch (Exception ex)
            when (ex
                    is ArgumentException
                        or FormatException
                        or KeyNotFoundException
                        or InvalidOperationException
                        or JsonException
                        or HttpRequestException
            )
        {
            throw new CommandException(ex.Message, innerException: ex);
        }
    }

    protected abstract ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    );

    protected static async ValueTask WriteJsonAsync<T>(IConsole console, T value) =>
        await console.Output.WriteLineAsync(
            JsonSerializer.Serialize(value, typeof(T), JsonSerializerContext.Default)
        );

    protected static async ValueTask<JsonElement> ReadJsonAsync(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        if (!console.IsInputRedirected)
            throw new CommandException("Expected JSON input on the standard input stream.");

        using var document = await JsonDocument.ParseAsync(
            console.Input.BaseStream,
            cancellationToken: cancellationToken
        );

        return document.RootElement.Clone();
    }
}
