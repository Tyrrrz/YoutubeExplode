using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.Builders;
using YoutubeExplode.Converter.Utils;
using YoutubeExplode.Converter.Utils.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

internal partial class Converter
{
    private readonly VideoClient _videoClient;
    private readonly FFmpeg _ffmpeg;
    private readonly ConversionPreset _preset;

    public Converter(VideoClient videoClient, FFmpeg ffmpeg, ConversionPreset preset)
    {
        _videoClient = videoClient;
        _ffmpeg = ffmpeg;
        _preset = preset;
    }

    private async ValueTask ProcessAsync(
        string filePath,
        Container container,
        IReadOnlyList<StreamInput> streamInputs,
        IReadOnlyList<SubtitleInput> subtitleInputs,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = new ArgumentsBuilder();

        // Stream inputs
        foreach (var streamInput in streamInputs)
            arguments.Add("-i").Add(streamInput.FilePath);

        // Subtitle inputs
        foreach (var subtitleInput in subtitleInputs)
            arguments.Add("-i").Add(subtitleInput.FilePath);

        // Format
        arguments.Add("-f").Add(container.Name);

        // Preset
        arguments.Add("-preset").Add(_preset);

        // Mapping
        for (var i = 0; i < streamInputs.Count + subtitleInputs.Count; i++)
            arguments.Add("-map").Add(i);

        // Avoid transcoding if possible
        if (streamInputs.All(s => s.Info.Container == container))
        {
            arguments
                .Add("-c:a").Add("copy")
                .Add("-c:v").Add("copy");
        }

        // MP4: specify subtitle codec manually, otherwise they're not injected
        if (container == Container.Mp4 && subtitleInputs.Any())
            arguments.Add("-c:s").Add("mov_text");

        // MP3: specify bitrate manually, otherwise the metadata will contain wrong duration
        // https://superuser.com/questions/892996/ffmpeg-is-doubling-audio-length-when-extracting-from-video
        if (container == Container.Mp3)
            arguments.Add("-b:a").Add("165k");

        // Inject language metadata for subtitles
        for (var i = 0; i < subtitleInputs.Count; i++)
        {
            arguments
                .Add($"-metadata:s:s:{i}")
                .Add($"language={subtitleInputs[i].Info.Language.Code}")
                .Add($"-metadata:s:s:{i}")
                .Add($"title={subtitleInputs[i].Info.Language.Name}");
        }

        // Misc settings
        arguments
            .Add("-threads").Add(Environment.ProcessorCount)
            .Add("-nostdin")
            .Add("-y");

        // Output
        arguments.Add(filePath);

        // Run FFmpeg
        await _ffmpeg.ExecuteAsync(arguments.Build(), progress, cancellationToken);
    }

    private async ValueTask PopulateStreamInputsAsync(
        string baseFilePath,
        IReadOnlyList<IStreamInfo> streamInfos,
        ICollection<StreamInput> streamInputs,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var progressMuxer = progress?.Pipe(p => new ProgressMuxer(p));
        var progresses = streamInfos.Select(s => progressMuxer?.CreateInput(s.Size.MegaBytes)).ToArray();

        var lastIndex = 0;

        foreach (var (streamInfo, streamProgress) in streamInfos.Zip(progresses))
        {
            var streamInput = new StreamInput(
                streamInfo,
                $"{baseFilePath}.stream-{lastIndex++}.tmp"
            );

            streamInputs.Add(streamInput);

            await _videoClient.Streams.DownloadAsync(
                streamInfo,
                streamInput.FilePath,
                streamProgress,
                cancellationToken
            );
        }

        progress?.Report(1);
    }

    private async ValueTask PopulateSubtitleInputsAsync(
        string baseFilePath,
        IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos,
        ICollection<SubtitleInput> subtitleInputs,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var progressMuxer = progress?.Pipe(p => new ProgressMuxer(p));
        var progresses = closedCaptionTrackInfos.Select(_ => progressMuxer?.CreateInput()).ToArray();

        var lastIndex = 0;

        foreach (var (trackInfo, trackProgress) in closedCaptionTrackInfos.Zip(progresses))
        {
            var subtitleInput = new SubtitleInput(
                trackInfo,
                $"{baseFilePath}.subtitles-{lastIndex++}.tmp"
            );

            subtitleInputs.Add(subtitleInput);

            await _videoClient.ClosedCaptions.DownloadAsync(
                trackInfo,
                subtitleInput.FilePath,
                trackProgress,
                cancellationToken
            );
        }

        progress?.Report(1);
    }

    public async ValueTask ProcessAsync(
        string filePath,
        Container container,
        IReadOnlyList<IStreamInfo> streamInfos,
        IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!streamInfos.Any())
            throw new InvalidOperationException("No streams provided.");

        if (streamInfos.Count > 2)
            throw new InvalidOperationException("Too many streams provided.");

        var progressMuxer = progress?.Pipe(p => new ProgressMuxer(p));
        var streamDownloadProgress = progressMuxer?.CreateInput();
        var subtitleDownloadProgress = progressMuxer?.CreateInput(0.01);
        var conversionProgress = progressMuxer?.CreateInput(
            streamInfos.All(s => s.Container == container)
                ? 0.05 // transcoding is not required
                : 10 // transcoding is required
        );

        var streamInputs = new List<StreamInput>(streamInfos.Count);
        var subtitleInputs = new List<SubtitleInput>(closedCaptionTrackInfos.Count);

        try
        {
            await PopulateStreamInputsAsync(
                filePath,
                streamInfos,
                streamInputs,
                streamDownloadProgress,
                cancellationToken
            );

            await PopulateSubtitleInputsAsync(
                filePath,
                closedCaptionTrackInfos,
                subtitleInputs,
                subtitleDownloadProgress,
                cancellationToken
            );

            await ProcessAsync(
                filePath,
                container,
                streamInputs,
                subtitleInputs,
                conversionProgress,
                cancellationToken
            );
        }
        finally
        {
            foreach (var inputStream in streamInputs)
                inputStream.Dispose();

            foreach (var inputClosedCaptionTrack in subtitleInputs)
                inputClosedCaptionTrack.Dispose();
        }
    }
}

internal partial class Converter
{
    private class StreamInput : IDisposable
    {
        public IStreamInfo Info { get; }

        public string FilePath { get; }

        public StreamInput(IStreamInfo info, string filePath)
        {
            Info = info;
            FilePath = filePath;
        }

        public void Dispose()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    private class SubtitleInput : IDisposable
    {
        public ClosedCaptionTrackInfo Info { get; }

        public string FilePath { get; }

        public SubtitleInput(ClosedCaptionTrackInfo info, string filePath)
        {
            Info = info;
            FilePath = filePath;
        }

        public void Dispose()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch
            {
                // Ignore
            }
        }
    }
}