using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Builders;
using YoutubeExplode.Converter.Utils.Extensions;

namespace YoutubeExplode.Converter
{
    // Ideally this should use named pipes and stream through stdout.
    // However, named pipes aren't well supported on non-Windows OS and
    // stdout streaming only works with some specific formats.
    internal partial class FFmpeg
    {
        private readonly string _cliFilePath;

        public FFmpeg(string cliFilePath) => _cliFilePath = cliFilePath;

        private string GetArguments(
            IReadOnlyList<string> inputFilePaths,
            string outputFilePath,
            string format,
            string preset,
            bool transcode)
        {
            var arguments = new ArgumentsBuilder();

            foreach (var inputFilePath in inputFilePaths)
            {
                arguments.Add("-i").Add(inputFilePath);
            }

            arguments.Add("-f").Add(format);
            arguments.Add("-preset").Add(preset);

            if (!transcode)
            {
                arguments.Add("-c").Add("copy");
            }

            arguments
                .Add("-threads").Add(Environment.ProcessorCount)
                .Add("-nostdin")
                .Add("-shortest")
                .Add("-y");

            arguments.Add(outputFilePath);

            return arguments.Build();
        }

        public async ValueTask ExecuteAsync(
            IReadOnlyList<string> inputFilePaths,
            string outputFilePath,
            string format,
            string preset,
            bool transcode,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var stdErrBuffer = new StringBuilder();

            var stdErrPipe = PipeTarget.Merge(
                PipeTarget.ToStringBuilder(stdErrBuffer), // error data collector
                progress?.Pipe(p => new FFmpegProgressRouter(p)) ?? PipeTarget.Null // progress
            );

            var arguments = GetArguments(
                inputFilePaths,
                outputFilePath,
                format,
                preset,
                transcode
            );

            var result = await Cli.Wrap(_cliFilePath)
                .WithArguments(arguments)
                .WithStandardErrorPipe(stdErrPipe)
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"FFmpeg exited with a non-zero exit code ({result.ExitCode})." + Environment.NewLine +
                    "Standard error:" + Environment.NewLine +
                    stdErrBuffer
                );
            }
        }
    }

    internal partial class FFmpeg
    {
        private class FFmpegProgressRouter : PipeTarget
        {
            private readonly StringBuilder _buffer  = new();
            private readonly IProgress<double> _output;

            private TimeSpan? _totalDuration;
            private TimeSpan? _lastOffset;

            public FFmpegProgressRouter(IProgress<double> output) => _output = output;

            private TimeSpan? TryParseTotalDuration(string data) => data
                .Pipe(s => Regex.Match(s, @"Duration:\s(\d\d:\d\d:\d\d.\d\d)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .Pipe(s => TimeSpan.ParseExact(s, "c", CultureInfo.InvariantCulture));

            private TimeSpan? TryParseCurrentOffset(string data) => data
                .Pipe(s => Regex.Matches(s, @"time=(\d\d:\d\d:\d\d.\d\d)")
                    .Cast<Match>()
                    .LastOrDefault()?
                    .Groups[1]
                    .Value)?
                .NullIfWhiteSpace()?
                .Pipe(s => TimeSpan.ParseExact(s, "c", CultureInfo.InvariantCulture));

            private void HandleBuffer()
            {
                var data = _buffer.ToString();

                _totalDuration ??= TryParseTotalDuration(data);
                if (_totalDuration is null)
                    return;

                var currentOffset = TryParseCurrentOffset(data);
                if (currentOffset is null || currentOffset == _lastOffset)
                    return;

                _lastOffset = currentOffset;

                var progress = (
                    currentOffset.Value.TotalMilliseconds / _totalDuration.Value.TotalMilliseconds
                ).Clamp(0, 1);

                _output.Report(progress);
            }

            public override async Task CopyFromAsync(Stream source, CancellationToken cancellationToken = default)
            {
                using var reader = new StreamReader(source, Console.OutputEncoding, false, 1024, true);

                var buffer = new char[1024];
                int charsRead;

                while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _buffer.Append(buffer, 0, charsRead);
                    HandleBuffer();
                }
            }
        }
    }
}