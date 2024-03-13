using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Demo.Gui.Utils;
using YoutubeExplode.Demo.Gui.Utils.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Demo.Gui.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly YoutubeClient _youtube = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            SetProperty(ref _isBusy, value);
            PullMetadataCommand.NotifyCanExecuteChanged();
            DownloadStreamCommand.NotifyCanExecuteChanged();
            DownloadClosedCaptionTrackCommand.NotifyCanExecuteChanged();
        }
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    private bool _isProgressIndeterminate;
    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        private set => SetProperty(ref _isProgressIndeterminate, value);
    }

    private string? _query;
    public string? Query
    {
        get => _query;
        set
        {
            SetProperty(ref _query, value);
            PullMetadataCommand.NotifyCanExecuteChanged();
        }
    }

    private Video? _video;
    public Video? Video
    {
        get => _video;
        private set
        {
            SetProperty(ref _video, value);
            OnPropertyChanged(nameof(IsDataAvailable));
            DownloadStreamCommand.NotifyCanExecuteChanged();
            DownloadClosedCaptionTrackCommand.NotifyCanExecuteChanged();
        }
    }

    private Thumbnail? _videoThumbnail;
    public Thumbnail? VideoThumbnail
    {
        get => _videoThumbnail;
        private set
        {
            SetProperty(ref _videoThumbnail, value);
            OnPropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Channel? _channel;
    public Channel? Channel
    {
        get => _channel;
        private set
        {
            SetProperty(ref _channel, value);
            OnPropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Thumbnail? _channelThumbnail;
    public Thumbnail? ChannelThumbnail
    {
        get => _channelThumbnail;
        private set
        {
            SetProperty(ref _channelThumbnail, value);
            OnPropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<MuxedStreamInfo>? _muxedStreamInfos;
    public IReadOnlyList<MuxedStreamInfo>? MuxedStreamInfos
    {
        get => _muxedStreamInfos;
        private set
        {
            SetProperty(ref _muxedStreamInfos, value);
            OnPropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<AudioOnlyStreamInfo>? _audioOnlyStreamInfos;
    public IReadOnlyList<AudioOnlyStreamInfo>? AudioOnlyStreamInfos
    {
        get => _audioOnlyStreamInfos;
        private set
        {
            SetProperty(ref _audioOnlyStreamInfos, value);
            OnPropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<VideoOnlyStreamInfo>? _videoOnlyStreamInfos;
    public IReadOnlyList<VideoOnlyStreamInfo>? VideoOnlyStreamInfos
    {
        get => _videoOnlyStreamInfos;
        private set
        {
            SetProperty(ref _videoOnlyStreamInfos, value);
            OnPropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<ClosedCaptionTrackInfo>? _closedCaptionTrackInfos;
    public IReadOnlyList<ClosedCaptionTrackInfo>? ClosedCaptionTrackInfos
    {
        get => _closedCaptionTrackInfos;
        private set
        {
            SetProperty(ref _closedCaptionTrackInfos, value);
            OnPropertyChanged(nameof(IsDataAvailable));
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

    public AsyncRelayCommand PullMetadataCommand { get; }

    public AsyncRelayCommand<IStreamInfo> DownloadStreamCommand { get; }

    public AsyncRelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

    public MainViewModel()
    {
        PullMetadataCommand = new AsyncRelayCommand(
            PullMetadataAsync,
            () => !IsBusy && !string.IsNullOrWhiteSpace(Query)
        );

        DownloadStreamCommand = new AsyncRelayCommand<IStreamInfo>(
            DownloadStreamAsync,
            s => s is not null && !IsBusy && Video is not null
        );

        DownloadClosedCaptionTrackCommand = new AsyncRelayCommand<ClosedCaptionTrackInfo>(
            DownloadClosedCaptionTrackAsync,
            c => c is not null && !IsBusy && Video is not null
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

    private async Task DownloadStreamAsync(IStreamInfo? streamInfo)
    {
        if (streamInfo is null || IsBusy || Video is null)
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

    private async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo? trackInfo)
    {
        if (trackInfo is null || IsBusy || Video is null)
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
