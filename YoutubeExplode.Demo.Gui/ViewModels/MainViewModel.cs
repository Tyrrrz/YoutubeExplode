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

public partial class MainViewModel : ObservableObject
{
    private readonly YoutubeClient _youtube = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PullMetadataCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadStreamCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadClosedCaptionTrackCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isProgressIndeterminate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PullMetadataCommand))]
    private string? _query;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    [NotifyCanExecuteChangedFor(nameof(DownloadStreamCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadClosedCaptionTrackCommand))]
    private Video? _video;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private Thumbnail? _videoThumbnail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private Channel? _channel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private Thumbnail? _channelThumbnail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private IReadOnlyList<MuxedStreamInfo>? _muxedStreamInfos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private IReadOnlyList<AudioOnlyStreamInfo>? _audioOnlyStreamInfos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private IReadOnlyList<VideoOnlyStreamInfo>? _videoOnlyStreamInfos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDataAvailable))]
    private IReadOnlyList<ClosedCaptionTrackInfo>? _closedCaptionTrackInfos;

    public bool IsDataAvailable =>
        Video is not null
        && VideoThumbnail is not null
        && Channel is not null
        && ChannelThumbnail is not null
        && MuxedStreamInfos is not null
        && AudioOnlyStreamInfos is not null
        && VideoOnlyStreamInfos is not null
        && ClosedCaptionTrackInfos is not null;

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

    private bool CanPullMetadata() => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    [RelayCommand(CanExecute = nameof(CanPullMetadata))]
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

    private bool CanDownloadStream(IStreamInfo? streamInfo) =>
        !IsBusy && Video is not null && streamInfo is not null;

    [RelayCommand(CanExecute = nameof(CanDownloadStream))]
    private async Task DownloadStreamAsync(IStreamInfo? streamInfo)
    {
        if (IsBusy || Video is null || streamInfo is null)
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

    private bool CanDownloadClosedCaptionTrack(ClosedCaptionTrackInfo? trackInfo) =>
        !IsBusy && Video is not null && trackInfo is not null;

    [RelayCommand(CanExecute = nameof(CanDownloadClosedCaptionTrack))]
    private async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo? trackInfo)
    {
        if (IsBusy || Video is null || trackInfo is null)
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
