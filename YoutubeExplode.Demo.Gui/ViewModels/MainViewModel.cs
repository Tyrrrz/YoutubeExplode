using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Demo.Gui.ViewModels.Framework;
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
        private set
        {
            Set(ref _isBusy, value);
            PullDataCommand.RaiseCanExecuteChanged();
            DownloadStreamCommand.RaiseCanExecuteChanged();
        }
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        private set => Set(ref _progress, value);
    }

    private bool _isProgressIndeterminate;
    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        private set => Set(ref _isProgressIndeterminate, value);
    }

    private string? _query;
    public string? Query
    {
        get => _query;
        set
        {
            Set(ref _query, value);
            PullDataCommand.RaiseCanExecuteChanged();
        }
    }

    private Video? _video;
    public Video? Video
    {
        get => _video;
        private set
        {
            Set(ref _video, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Thumbnail? _videoThumbnail;
    public Thumbnail? VideoThumbnail
    {
        get => _videoThumbnail;
        private set
        {
            Set(ref _videoThumbnail, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Channel? _channel;
    public Channel? Channel
    {
        get => _channel;
        private set
        {
            Set(ref _channel, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private Thumbnail? _channelThumbnail;
    public Thumbnail? ChannelThumbnail
    {
        get => _channelThumbnail;
        private set
        {
            Set(ref _channelThumbnail, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<MuxedStreamInfo>? _muxedStreamInfos;
    public IReadOnlyList<MuxedStreamInfo>? MuxedStreamInfos
    {
        get => _muxedStreamInfos;
        private set
        {
            Set(ref _muxedStreamInfos, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<AudioOnlyStreamInfo>? _audioOnlyStreamInfos;
    public IReadOnlyList<AudioOnlyStreamInfo>? AudioOnlyStreamInfos
    {
        get => _audioOnlyStreamInfos;
        private set
        {
            Set(ref _audioOnlyStreamInfos, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<VideoOnlyStreamInfo>? _videoOnlyStreamInfos;
    public IReadOnlyList<VideoOnlyStreamInfo>? VideoOnlyStreamInfos
    {
        get => _videoOnlyStreamInfos;
        private set
        {
            Set(ref _videoOnlyStreamInfos, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
        }
    }

    private IReadOnlyList<ClosedCaptionTrackInfo>? _closedCaptionTrackInfos;
    public IReadOnlyList<ClosedCaptionTrackInfo>? ClosedCaptionTrackInfos
    {
        get => _closedCaptionTrackInfos;
        private set
        {
            Set(ref _closedCaptionTrackInfos, value);
            RaisePropertyChanged(nameof(IsDataAvailable));
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

    public RelayCommand PullDataCommand { get; }

    public RelayCommand<IStreamInfo> DownloadStreamCommand { get; }

    public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

    public MainViewModel()
    {
        PullDataCommand = new AsyncRelayCommand(
            PullDataAsync,
            () => !IsBusy && !string.IsNullOrWhiteSpace(Query)
        );

        DownloadStreamCommand = new AsyncRelayCommand<IStreamInfo>(
            DownloadStreamAsync,
            _ => !IsBusy
        );

        DownloadClosedCaptionTrackCommand = new AsyncRelayCommand<ClosedCaptionTrackInfo>(
            DownloadClosedCaptionTrackAsync,
            _ => !IsBusy
        );
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalidChar, '_');

        return fileName;
    }

    private static string? PromptSaveFilePath(string defaultFileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            Filter = filter,
            AddExtension = true,
            DefaultExt = Path.GetExtension(defaultFileName)
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private async Task PullDataAsync()
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
            var videoIdOrUrl = Query;

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

    private async Task DownloadStreamAsync(IStreamInfo streamInfo)
    {
        if (IsBusy || Video is null)
            return;

        try
        {
            // Enter busy state
            IsBusy = true;
            Progress = 0;

            // Generate default file name
            var defaultFileName = SanitizeFileName($"{Video.Title}.{streamInfo.Container.Name}");

            // Prompt file path
            var filePath = PromptSaveFilePath(
                defaultFileName,
                $"{streamInfo.Container.Name} files|*.{streamInfo.Container.Name}|All Files|*.*"
            );

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            // Set up progress reporting
            var progressHandler = new Progress<double>(p => Progress = p);

            // Download to file
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

            // Generate default file name
            var defaultFileName = SanitizeFileName($"{Video.Title}.{trackInfo.Language.Name}.srt");

            // Prompt file path
            var filePath = PromptSaveFilePath(defaultFileName, "SRT Files|*.srt|All Files|*.*");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            // Set up progress reporting
            var progressHandler = new Progress<double>(p => Progress = p);

            // Download to file
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
