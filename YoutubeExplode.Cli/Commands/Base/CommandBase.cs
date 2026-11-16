using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    protected static void WriteJson<T>(IConsole console, T value) =>
        console.Output.WriteLine(
            JsonSerializer.Serialize(value, Json.CreateOptions(!console.IsOutputRedirected))
        );

    protected static async ValueTask<T> ReadJsonFromStdinAsync<T>(
        CancellationToken cancellationToken
    )
    {
        if (!Console.IsInputRedirected)
            throw new CommandException("Expected JSON input on the standard input stream.");

        await using var input = Console.OpenStandardInput();

        return await JsonSerializer.DeserializeAsync<T>(
                input,
                Json.CreateOptions(false),
                cancellationToken
            ) ?? throw new CommandException("Standard input does not contain a valid JSON value.");
    }

    protected static async ValueTask<List<T>> ToListAsync<T>(
        IAsyncEnumerable<T> source,
        CancellationToken cancellationToken
    )
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
            list.Add(item);
        return list;
    }

    protected static void EnsureDirectoryExists(string filePath)
    {
        var dirPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);
    }
}
