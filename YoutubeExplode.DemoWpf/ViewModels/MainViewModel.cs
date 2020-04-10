using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using YoutubeExplode.Channels;
using YoutubeExplode.DemoWpf.ViewModels.Framework;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.DemoWpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly YoutubeClient _youtube;

        private bool _isBusy;
        private string? _query;
        private Video? _video;
        private Channel? _channel;
        private IReadOnlyList<MuxedStreamInfo>? _muxedStreamInfos;
        private IReadOnlyList<AudioOnlyStreamInfo>? _audioOnlyStreamInfos;
        private IReadOnlyList<VideoOnlyStreamInfo>? _videoOnlyStreamInfos;
        private IReadOnlyList<ClosedCaptionTrackInfo>? _closedCaptionTrackInfos;
        private double _progress;
        private bool _isProgressIndeterminate;

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

        public string? Query
        {
            get => _query;
            set
            {
                Set(ref _query, value);
                PullDataCommand.RaiseCanExecuteChanged();
            }
        }

        public Video? Video
        {
            get => _video;
            private set
            {
                Set(ref _video, value);
                RaisePropertyChanged(nameof(IsDataAvailable));
            }
        }

        public Channel? Channel
        {
            get => _channel;
            private set
            {
                Set(ref _channel, value);
                RaisePropertyChanged(nameof(IsDataAvailable));
            }
        }

        public IReadOnlyList<MuxedStreamInfo>? MuxedStreamInfos
        {
            get => _muxedStreamInfos;
            private set
            {
                Set(ref _muxedStreamInfos, value);
                RaisePropertyChanged(nameof(IsDataAvailable));
            }
        }

        public IReadOnlyList<AudioOnlyStreamInfo>? AudioOnlyStreamInfos
        {
            get => _audioOnlyStreamInfos;
            private set
            {
                Set(ref _audioOnlyStreamInfos, value);
                RaisePropertyChanged(nameof(IsDataAvailable));
            }
        }

        public IReadOnlyList<VideoOnlyStreamInfo>? VideoOnlyStreamInfos
        {
            get => _videoOnlyStreamInfos;
            private set
            {
                Set(ref _videoOnlyStreamInfos, value);
                RaisePropertyChanged(nameof(IsDataAvailable));
            }
        }

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
            Video != null && Channel != null &&
            MuxedStreamInfos != null &&
            AudioOnlyStreamInfos != null &&
            VideoOnlyStreamInfos != null &&
            ClosedCaptionTrackInfos != null;

        public double Progress
        {
            get => _progress;
            private set => Set(ref _progress, value);
        }

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            private set => Set(ref _isProgressIndeterminate, value);
        }

        // Commands
        public RelayCommand PullDataCommand { get; }
        public RelayCommand<IStreamInfo> DownloadStreamCommand { get; }
        public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

        public MainViewModel()
        {
            _youtube = new YoutubeClient();

            // Commands
            PullDataCommand = new RelayCommand(PullData,
                () => !IsBusy && !string.IsNullOrWhiteSpace(Query));
            DownloadStreamCommand = new RelayCommand<IStreamInfo>(DownloadStream,
                _ => !IsBusy);
            DownloadClosedCaptionTrackCommand = new RelayCommand<ClosedCaptionTrackInfo>(
                DownloadClosedCaptionTrack, _ => !IsBusy);
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
                DefaultExt = Path.GetExtension(defaultFileName) ?? ""
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private async void PullData()
        {
            try
            {
                // Enter busy state
                IsBusy = true;
                IsProgressIndeterminate = true;

                // Reset data
                Video = null;
                Channel = null;
                MuxedStreamInfos = null;
                AudioOnlyStreamInfos = null;
                VideoOnlyStreamInfos = null;
                ClosedCaptionTrackInfos = null;

                // Normalize video id
                var videoId = new VideoId(Query!);

                // Get data
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
                var trackManifest = await _youtube.Videos.ClosedCaptionTracks.GetManifestAsync(videoId);

                Video = await _youtube.Videos.GetAsync(videoId);
                Channel = await _youtube.Channels.GetByVideoAsync(videoId);
                MuxedStreamInfos = streamManifest.GetMuxed().ToArray();
                AudioOnlyStreamInfos = streamManifest.GetAudioOnly().ToArray();
                VideoOnlyStreamInfos = streamManifest.GetVideoOnly().ToArray();
                ClosedCaptionTrackInfos = trackManifest.Tracks;
            }
            finally
            {
                // Exit busy state
                IsBusy = false;
                IsProgressIndeterminate = false;
            }
        }

        private async void DownloadStream(IStreamInfo streamInfo)
        {
            try
            {
                // Enter busy state
                IsBusy = true;
                Progress = 0;

                // Generate default file name
                var fileExt = streamInfo.Container.GetFileExtension();
                var defaultFileName = SanitizeFileName($"{Video!.Title}.{fileExt}");

                // Prompt file path
                var filePath = PromptSaveFilePath(defaultFileName, $"{fileExt} files|*.{fileExt}|All Files|*.*");
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                // Set up progress handler
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

        private async void DownloadClosedCaptionTrack(ClosedCaptionTrackInfo trackInfo)
        {
            try
            {
                // Enter busy state
                IsBusy = true;
                Progress = 0;

                // Generate default file name
                var defaultFileName = SanitizeFileName($"{Video!.Title}.{trackInfo.Language.Name}.srt");

                // Prompt file path
                var filePath = PromptSaveFilePath(defaultFileName, "SRT Files|*.srt|All Files|*.*");
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                // Set up progress handler
                var progressHandler = new Progress<double>(p => Progress = p);

                // Download to file
                await _youtube.Videos.ClosedCaptionTracks.DownloadAsync(trackInfo, filePath, progressHandler);
            }
            finally
            {
                // Exit busy state
                IsBusy = false;
                Progress = 0;
            }
        }
    }
}