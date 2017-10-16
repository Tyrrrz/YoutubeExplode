using System;
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
        private string _videoId;
        private Video _video;
        private double _progress;
        private bool _isProgressIndeterminate;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                Set(ref _isBusy, value);
                GetVideoCommand.RaiseCanExecuteChanged();
                DownloadMediaStreamCommand.RaiseCanExecuteChanged();
            }
        }

        public string VideoId
        {
            get => _videoId;
            set
            {
                Set(ref _videoId, value);
                GetVideoCommand.RaiseCanExecuteChanged();
            }
        }

        public Video Video
        {
            get => _video;
            private set
            {
                Set(ref _video, value);
                RaisePropertyChanged(() => IsVideoAvailable);
            }
        }

        public bool IsVideoAvailable => Video != null;

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
        public RelayCommand GetVideoCommand { get; }
        public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

        public MainViewModel(YoutubeClient client)
        {
            _client = client;

            // Commands
            GetVideoCommand = new RelayCommand(GetVideo, () => !IsBusy && VideoId.IsNotBlank());
            DownloadMediaStreamCommand = new RelayCommand<MediaStreamInfo>(DownloadMediaStream, vse => !IsBusy);
            DownloadClosedCaptionTrackCommand = new RelayCommand<ClosedCaptionTrackInfo>(
                DownloadClosedCaptionTrack, vse => !IsBusy);
        }

        private async void GetVideo()
        {
            // Check params
            if (VideoId.IsBlank())
                return;

            IsBusy = true;
            IsProgressIndeterminate = true;

            // Reset data
            Video = null;

            // Parse URL if necessary
            if (!YoutubeClient.TryParseVideoId(VideoId, out string id))
                id = VideoId;

            // Perform the request
            Video = await _client.GetVideoAsync(id);

            IsBusy = false;
            IsProgressIndeterminate = false;
        }

        private async void DownloadMediaStream(MediaStreamInfo info)
        {
            // Create dialog
            var fileExtension = info.Container.GetFileExtension();
            var defaultFileName = $"{Video.Title}.{fileExtension}";
            defaultFileName = defaultFileName.Except(Path.GetInvalidFileNameChars());
            var fileFilter =
                $"{info.Container} Files|*.{fileExtension}|" +
                "All Files|*.*";
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = fileExtension,
                FileName = defaultFileName,
                Filter = fileFilter
            };

            // Select destination
            if (sfd.ShowDialog() != true) return;
            var filePath = sfd.FileName;

            // Download and save to file
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
            var defaultFileName = $"{Video.Title}.{info.Language.Name}.srt";
            defaultFileName = defaultFileName.Except(Path.GetInvalidFileNameChars());
            var fileFilter =
                "SRT Files|*.srt|" +
                "All Files|*.*";
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "srt",
                FileName = defaultFileName,
                Filter = fileFilter
            };
            // Select destination
            if (sfd.ShowDialog() != true) return;
            var filePath = sfd.FileName;

            // Download
            IsBusy = true;
            Progress = 0;

            var progressHandler = new Progress<double>(p => Progress = p);
            await _client.DownloadClosedCaptionTrackAsync(info, filePath, progressHandler);

            IsBusy = false;
            Progress = 0;
        }
    }
}