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
    private YoutubeClient? _youtube;

    protected YoutubeClient Youtube => _youtube ??= new YoutubeClient();

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        try
        {
            await ExecuteAsync(console, cancellationToken);
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
                        or JsonException
                        or HttpRequestException
            )
        {
            throw new CommandException(ex.Message, innerException: ex);
        }
        finally
        {
            _youtube?.Dispose();
        }
    }

    protected abstract ValueTask ExecuteAsync(
        IConsole console,
        CancellationToken cancellationToken
    );

    // Data goes to stdout as JSON; compact when piped, indented when shown to the user
    protected static async ValueTask WriteJsonAsync<T>(IConsole console, T value) =>
        await console.Output.WriteLineAsync(
            JsonSerializer.Serialize(
                value,
                typeof(T),
                Json.CreateContext(!console.IsOutputRedirected)
            )
        );

    protected static async ValueTask<T> ReadJsonAsync<T>(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        if (!console.IsInputRedirected)
            throw new CommandException("Expected JSON input on the standard input stream.");

        return
            await JsonSerializer.DeserializeAsync(
                console.Input.BaseStream,
                typeof(T),
                Json.CreateContext(false),
                cancellationToken
            )
                is T value
            ? value
            : throw new CommandException("Standard input does not contain a valid JSON value.");
    }
}
