using System;
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Tyrrrz.Extensions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace DemoWpf.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly YoutubeClient _client;

        private bool _isBusy;
        private string _query;
        private Video _video;
        private Channel _channel;
        private MediaStreamInfoSet _mediaStreamInfos;
        private IReadOnlyList<ClosedCaptionTrackInfo> _closedCaptionTrackInfos;
        private double _progress;
        private bool _isProgressIndeterminate;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                Set(ref _isBusy, value);
                GetDataCommand.RaiseCanExecuteChanged();
                DownloadMediaStreamCommand.RaiseCanExecuteChanged();
            }
        }

        public string Query
        {
            get => _query;
            set
            {
                Set(ref _query, value);
                GetDataCommand.RaiseCanExecuteChanged();
            }
        }

        public Video Video
        {
            get => _video;
            private set
            {
                Set(ref _video, value);
                RaisePropertyChanged(() => IsDataAvailable);
            }
        }

        public Channel Channel
        {
            get => _channel;
            private set
            {
                Set(ref _channel, value);
                RaisePropertyChanged(() => IsDataAvailable);
            }
        }

        public MediaStreamInfoSet MediaStreamInfos
        {
            get => _mediaStreamInfos;
            private set
            {
                Set(ref _mediaStreamInfos, value);
                RaisePropertyChanged(() => IsDataAvailable);
            }
        }

        public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTrackInfos
        {
            get => _closedCaptionTrackInfos;
            private set
            {
                Set(ref _closedCaptionTrackInfos, value);
                RaisePropertyChanged(() => IsDataAvailable);
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
        public RelayCommand GetDataCommand { get; }
        public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

        public MainViewModel()
        {
            _client = new YoutubeClient();

            // Commands
            GetDataCommand = new RelayCommand(GetData,
                () => !IsBusy && Query.IsNotBlank());
            DownloadMediaStreamCommand = new RelayCommand<MediaStreamInfo>(DownloadMediaStream,
                _ => !IsBusy);
            DownloadClosedCaptionTrackCommand = new RelayCommand<ClosedCaptionTrackInfo>(
                DownloadClosedCaptionTrack, _ => !IsBusy);
        }

        private async void GetData()
        {
            IsBusy = true;
            IsProgressIndeterminate = true;

            // Reset data
            Video = null;
            Channel = null;
            MediaStreamInfos = null;
            ClosedCaptionTrackInfos = null;

            // Parse URL if necessary
            if (!YoutubeClient.TryParseVideoId(Query, out var videoId))
                videoId = Query;

            // Get data
            Video = await _client.GetVideoAsync(videoId);
            Channel = await _client.GetVideoAuthorChannelAsync(videoId);
            MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(videoId);
            ClosedCaptionTrackInfos = await _client.GetVideoClosedCaptionTrackInfosAsync(videoId);

            IsBusy = false;
            IsProgressIndeterminate = false;
        }

        private async void DownloadMediaStream(MediaStreamInfo info)
        {
            // Create dialog
            var fileExt = info.Container.GetFileExtension();
            var defaultFileName = $"{Video.Title}.{fileExt}"
                .Replace(Path.GetInvalidFileNameChars(), '_');

            var sfd = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = $"{fileExt} files|*.{fileExt}|All Files|*.*",
                AddExtension = true,
                DefaultExt = fileExt
            };

            // Select file path
            if (sfd.ShowDialog() != true)
                return;

            var filePath = sfd.FileName;

            // Download to file
            IsBusy = true;
            Progress = 0;

            var progressHandler = new Progress<double>(p => Progress = p);
            await _client.DownloadMediaStreamAsync(info, filePath, progressHandler);

            IsBusy = false;
            Progress = 0;
        }

        private async void DownloadClosedCaptionTrack(ClosedCaptionTrackInfo info)
        {
            // Create dialog
            var defaultFileName = $"{Video.Title}.{info.Language.Name}.srt"
                .Replace(Path.GetInvalidFileNameChars(), '_');

            var sfd = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "SRT Files|*.srt|All Files|*.*",
                AddExtension = true,
                DefaultExt = "srt"
            };

            // Select file path
            if (sfd.ShowDialog() != true)
                return;

            var filePath = sfd.FileName;

            // Download to file
            IsBusy = true;
            Progress = 0;

            var progressHandler = new Progress<double>(p => Progress = p);
            await _client.DownloadClosedCaptionTrackAsync(info, filePath, progressHandler);

            IsBusy = false;
            Progress = 0;
        }
    }
}