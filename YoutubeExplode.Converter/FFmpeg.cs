using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using YoutubeExplode.Converter.Utils.Extensions;

namespace YoutubeExplode.Converter;

// Ideally this should use named pipes and stream through stdout.
// However, named pipes aren't well supported on non-Windows OS and
// stdout streaming only works with some specific formats.
internal partial class FFmpeg
{
    private readonly string _filePath;

    public FFmpeg(string filePath) => _filePath = filePath;

    public async ValueTask ExecuteAsync(
        string arguments,
        IProgress<double>? progress,
        CancellationToken cancellationToken = default)
    {
        var stdErrBuffer = new StringBuilder();

        var stdErrPipe = PipeTarget.Merge(
            // Collect error output in case of failure
            PipeTarget.ToStringBuilder(stdErrBuffer),
            // Collect progress output if requested
            progress?.Pipe(CreateProgressRouter) ?? PipeTarget.Null
        );

        var result = await Cli.Wrap(_filePath)
            .WithArguments(arguments)
            .WithStandardErrorPipe(stdErrPipe)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(cancellationToken);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"""
                FFmpeg exited with a non-zero exit code ({result.ExitCode}).

                Arguments:
                {arguments}

                Standard error:
                {stdErrBuffer}
                """
            );
        }
    }
}

internal partial class FFmpeg
{
    public static string GetFilePath() =>
        // Try to find FFmpeg in the probe directory
        Directory
            .EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory())
            .FirstOrDefault(f =>
                string.Equals(
                    Path.GetFileNameWithoutExtension(f),
                    "ffmpeg",
                    StringComparison.OrdinalIgnoreCase
                )
            ) ??

        // Otherwise fallback to just "ffmpeg" and hope it's on the PATH
        "ffmpeg";

    private static PipeTarget CreateProgressRouter(IProgress<double> progress)
    {
        var totalDuration = default(TimeSpan?);

        return PipeTarget.ToDelegate(l =>
        {
            totalDuration ??= Regex
                .Match(l, @"Duration:\s(\d\d:\d\d:\d\d.\d\d)")
                .Groups[1]
                .Value
                .NullIfWhiteSpace()?
                .Pipe(s => TimeSpan.ParseExact(s, "c", CultureInfo.InvariantCulture));

            if (totalDuration is null || totalDuration == TimeSpan.Zero)
                return;

            var processedDuration = Regex
                .Match(l, @"time=(\d\d:\d\d:\d\d.\d\d)")
                .Groups[1]
                .Value
                .NullIfWhiteSpace()?
                .Pipe(s => TimeSpan.ParseExact(s, "c", CultureInfo.InvariantCulture));

            if (processedDuration is null)
                return;

            progress.Report((
                processedDuration.Value.TotalMilliseconds /
                totalDuration.Value.TotalMilliseconds
            ).Clamp(0, 1));
        });
    }
}