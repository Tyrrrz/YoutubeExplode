using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using ReactiveUI;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Demo.Gui.Utils;
using YoutubeExplode.Demo.Gui.Utils.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Demo.Gui.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly YoutubeClient _youtube = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        private set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    private bool _isProgressIndeterminate;
    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        private set => this.RaiseAndSetIfChanged(ref _isProgressIndeterminate, value);
    }

    private string? _query;
    public string? Query
    {
        get => _query;
        set => this.RaiseAndSetIfChanged(ref _query, value);
    }

    private Video? _video;
    public Video? Video
    {
        get => _video;
        private set
        {
            this.RaiseAndSetIfChanged(ref _video, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Thumbnail? _videoThumbnail;
    public Thumbnail? VideoThumbnail
    {
        get => _videoThumbnail;
        private set
        {
            this.RaiseAndSetIfChanged(ref _videoThumbnail, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Channel? _channel;
    public Channel? Channel
    {
        get => _channel;
        private set
        {
            this.RaiseAndSetIfChanged(ref _channel, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Thumbnail? _channelThumbnail;
    public Thumbnail? ChannelThumbnail
    {
        get => _channelThumbnail;
        private set
        {
            this.RaiseAndSetIfChanged(ref _channelThumbnail, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<MuxedStreamInfo>? _muxedStreamInfos;
    public IReadOnlyList<MuxedStreamInfo>? MuxedStreamInfos
    {
        get => _muxedStreamInfos;
        private set
        {
            this.RaiseAndSetIfChanged(ref _muxedStreamInfos, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<AudioOnlyStreamInfo>? _audioOnlyStreamInfos;
    public IReadOnlyList<AudioOnlyStreamInfo>? AudioOnlyStreamInfos
    {
        get => _audioOnlyStreamInfos;
        private set
        {
            this.RaiseAndSetIfChanged(ref _audioOnlyStreamInfos, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<VideoOnlyStreamInfo>? _videoOnlyStreamInfos;
    public IReadOnlyList<VideoOnlyStreamInfo>? VideoOnlyStreamInfos
    {
        get => _videoOnlyStreamInfos;
        private set
        {
            this.RaiseAndSetIfChanged(ref _videoOnlyStreamInfos, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<ClosedCaptionTrackInfo>? _closedCaptionTrackInfos;
    public IReadOnlyList<ClosedCaptionTrackInfo>? ClosedCaptionTrackInfos
    {
        get => _closedCaptionTrackInfos;
        private set
        {
            this.RaiseAndSetIfChanged(ref _closedCaptionTrackInfos, value);
            this.RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    public bool IsDataAvailable =>
        Video is not null
        && VideoThumbnail is not null
        && Channel is not null
        && ChannelThumbnail is not null
        && MuxedStreamInfos is not null
        && AudioOnlyStreamInfos is not null
        && VideoOnlyStreamInfos is not null
        && ClosedCaptionTrackInfos is not null;

    public ReactiveCommand<Unit, Unit> PullMetadataCommand { get; }

    public ReactiveCommand<IStreamInfo, Unit> DownloadStreamCommand { get; }

    public ReactiveCommand<ClosedCaptionTrackInfo, Unit> DownloadClosedCaptionTrackCommand { get; }

    public MainViewModel()
    {
        PullMetadataCommand = ReactiveCommand.CreateFromTask(
            PullMetadataAsync,
            this.WhenAnyValue(
                x => x.IsBusy,
                x => x.Query,
                (busy, query) => !busy && !string.IsNullOrWhiteSpace(query)
            )
        );

        DownloadStreamCommand = ReactiveCommand.CreateFromTask<IStreamInfo>(
            DownloadStreamAsync,
            this.WhenAnyValue(x => x.IsBusy, busy => !busy)
        );

        DownloadClosedCaptionTrackCommand = ReactiveCommand.CreateFromTask<ClosedCaptionTrackInfo>(
            DownloadClosedCaptionTrackAsync,
            this.WhenAnyValue(x => x.IsBusy, busy => !busy)
        );
    }

    private async Task PullMetadataAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Query))
            return;

        try
        {
            // Enter busy state
            IsBusy = true;
            IsProgressIndeterminate = true;

            // Reset data
            Video = null;
            VideoThumbnail = null;
            Channel = null;
            ChannelThumbnail = null;
            MuxedStreamInfos = null;
            AudioOnlyStreamInfos = null;
            VideoOnlyStreamInfos = null;
            ClosedCaptionTrackInfos = null;

            // Get data
            var videoIdOrUrl = Query.Trim();

            Video = await _youtube.Videos.GetAsync(videoIdOrUrl);
            VideoThumbnail = Video.Thumbnails.GetWithHighestResolution();

            Channel = await _youtube.Channels.GetAsync(Video.Author.ChannelId);
            ChannelThumbnail = Channel.Thumbnails.GetWithHighestResolution();

            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoIdOrUrl);

            MuxedStreamInfos = streamManifest
                .GetMuxedStreams()
                .OrderByDescending(s => s.VideoQuality)
                .ToArray();

            AudioOnlyStreamInfos = streamManifest
                .GetAudioOnlyStreams()
                .OrderByDescending(s => s.Bitrate)
                .ToArray();

            VideoOnlyStreamInfos = streamManifest
                .GetVideoOnlyStreams()
                .OrderByDescending(s => s.VideoQuality)
                .ToArray();

            var trackManifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoIdOrUrl);

            ClosedCaptionTrackInfos = trackManifest.Tracks.OrderBy(t => t.Language.Name).ToArray();
        }
        finally
        {
            // Exit busy state
            IsBusy = false;
            IsProgressIndeterminate = false;
        }
    }

    private async Task<string?> PromptSaveFilePathAsync(
        string defaultFileName,
        IReadOnlyList<string>? fileTypes
    )
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                SuggestedFileName = defaultFileName,
                FileTypeChoices = fileTypes
                    ?.Select(t => new FilePickerFileType($"{t} file") { Patterns = [$"*.{t}"] })
                    .ToArray(),
                DefaultExtension = Path.GetExtension(defaultFileName)
            }
        );

        return file?.Path.LocalPath;
    }

    private async Task DownloadStreamAsync(IStreamInfo streamInfo)
    {
        if (IsBusy || Video is null)
            return;

        try
        {
            // Enter busy state
            IsBusy = true;
            Progress = 0;

            // Generate a default file name
            var defaultFileName = PathEx.SanitizeFileName(
                $"{Video.Title}.{streamInfo.Container.Name}"
            );

            // Prompt for file path
            var filePath = await PromptSaveFilePathAsync(
                defaultFileName,
                [streamInfo.Container.Name]
            );

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            // Set up progress reporting
            var progressHandler = new DelegateProgress<double>(p => Progress = p);

            // Download to the file
            await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressHandler);
        }
        finally
        {
            // Exit busy state
            IsBusy = false;
            Progress = 0;
        }
    }

    private async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo trackInfo)
    {
        if (IsBusy || Video is null)
            return;

        try
        {
            // Enter busy state
            IsBusy = true;
            Progress = 0;

            // Generate a default file name
            var defaultFileName = PathEx.SanitizeFileName(
                $"{Video.Title}.{trackInfo.Language.Name}.srt"
            );

            // Prompt for file path
            var filePath = await PromptSaveFilePathAsync(defaultFileName, ["srt"]);
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            // Set up progress reporting
            var progressHandler = new DelegateProgress<double>(p => Progress = p);

            // Download to the file
            await _youtube.Videos.ClosedCaptions.DownloadAsync(
                trackInfo,
                filePath,
                progressHandler
            );
        }
        finally
        {
            // Exit busy state
            IsBusy = false;
            Progress = 0;
        }
    }
}
