using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using YoutubeExplode.DemoWpf.ViewModels.Framework;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.DemoWpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly YoutubeClient _client;

        private bool _isBusy;
        private string? _query;
        private Video? _video;
        private Channel? _channel;
        private MediaStreamInfoSet? _mediaStreamInfos;
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
                DownloadMediaStreamCommand.RaiseCanExecuteChanged();
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

        public MediaStreamInfoSet? MediaStreamInfos
        {
            get => _mediaStreamInfos;
            private set
            {
                Set(ref _mediaStreamInfos, value);
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

        public bool IsDataAvailable => Video != null && Channel != null
                                       && MediaStreamInfos != null && ClosedCaptionTrackInfos != null;

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
        public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

        public MainViewModel()
        {
            _client = new YoutubeClient();

            // Commands
            PullDataCommand = new RelayCommand(PullData,
                () => !IsBusy && !string.IsNullOrWhiteSpace(Query));
            DownloadMediaStreamCommand = new RelayCommand<MediaStreamInfo>(DownloadMediaStream,
                _ => !IsBusy);
            DownloadClosedCaptionTrackCommand = new RelayCommand<ClosedCaptionTrackInfo>(
                DownloadClosedCaptionTrack, _ => !IsBusy);
        }

        private static string NormalizeVideoId(string input) =>
            YoutubeClient.TryParseVideoId(input, out var videoId)
                ? videoId!
                : input;

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
                MediaStreamInfos = null;
                ClosedCaptionTrackInfos = null;

                // Normalize video id
                var videoId = NormalizeVideoId(Query!);

                // Get data
                Video = await _client.GetVideoAsync(videoId);
                Channel = await _client.GetVideoAuthorChannelAsync(videoId);
                MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(videoId);
                ClosedCaptionTrackInfos = await _client.GetVideoClosedCaptionTrackInfosAsync(videoId);
            }
            finally
            {
                // Exit busy state
                IsBusy = false;
                IsProgressIndeterminate = false;
            }
        }

        private async void DownloadMediaStream(MediaStreamInfo info)
        {
            try
            {
                // Enter busy state
                IsBusy = true;
                Progress = 0;

                // Generate default file name
                var fileExt = info.Container.GetFileExtension();
                var defaultFileName = SanitizeFileName($"{Video!.Title}.{fileExt}");

                // Prompt file path
                var filePath = PromptSaveFilePath(defaultFileName, $"{fileExt} files|*.{fileExt}|All Files|*.*");
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                // Set up progress handler
                var progressHandler = new Progress<double>(p => Progress = p);

                // Download to file
                await _client.DownloadMediaStreamAsync(info, filePath, progressHandler);
            }
            finally
            {
                // Exit busy state
                IsBusy = false;
                Progress = 0;
            }
        }

        private async void DownloadClosedCaptionTrack(ClosedCaptionTrackInfo info)
        {
            try
            {
                // Enter busy state
                IsBusy = true;
                Progress = 0;

                // Generate default file name
                var defaultFileName = SanitizeFileName($"{Video!.Title}.{info.Language.Name}.srt");

                // Prompt file path
                var filePath = PromptSaveFilePath(defaultFileName, "SRT Files|*.srt|All Files|*.*");
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                // Set up progress handler
                var progressHandler = new Progress<double>(p => Progress = p);

                // Download to file
                await _client.DownloadClosedCaptionTrackAsync(info, filePath, progressHandler);
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