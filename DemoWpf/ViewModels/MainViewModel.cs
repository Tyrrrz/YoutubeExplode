using System;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;

namespace YoutubeExplode.DemoWpf.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly YoutubeClient _client;

        private bool _isBusy;
        private string _videoId;
        private VideoInfo _videoInfo;
        private double _progress;
        private bool _isProgressIndeterminate;

        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                Set(ref _isBusy, value);
                GetVideoInfoCommand.RaiseCanExecuteChanged();
                DownloadMediaStreamCommand.RaiseCanExecuteChanged();
            }
        }

        public string VideoId
        {
            get { return _videoId; }
            set
            {
                Set(ref _videoId, value);
                GetVideoInfoCommand.RaiseCanExecuteChanged();
            }
        }

        public VideoInfo VideoInfo
        {
            get { return _videoInfo; }
            private set
            {
                Set(ref _videoInfo, value);
                RaisePropertyChanged(() => IsVideoInfoAvailable);
            }
        }

        public bool IsVideoInfoAvailable => VideoInfo != null;

        public double Progress
        {
            get { return _progress; }
            private set { Set(ref _progress, value); }
        }

        public bool IsProgressIndeterminate
        {
            get { return _isProgressIndeterminate; }
            private set { Set(ref _isProgressIndeterminate, value); }
        }

        // Commands
        public RelayCommand GetVideoInfoCommand { get; }
        public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

        public MainViewModel(YoutubeClient client)
        {
            _client = client;

            // Commands
            GetVideoInfoCommand = new RelayCommand(GetVideoInfoAsync, () => !IsBusy && VideoId.IsNotBlank());
            DownloadMediaStreamCommand = new RelayCommand<MediaStreamInfo>(DownloadMediaStreamAsync, vse => !IsBusy);
            DownloadClosedCaptionTrackCommand = new RelayCommand<ClosedCaptionTrackInfo>(
                DownloadClosedCaptionTrackAsync, vse => !IsBusy);
        }

        private async void GetVideoInfoAsync()
        {
            // Check params
            if (VideoId.IsBlank())
                return;

            IsBusy = true;
            IsProgressIndeterminate = true;

            // Reset data
            VideoInfo = null;

            // Parse URL if necessary
            if (!YoutubeClient.TryParseVideoId(VideoId, out string id))
                id = VideoId;

            // Perform the request
            VideoInfo = await _client.GetVideoInfoAsync(id);

            IsBusy = false;
            IsProgressIndeterminate = false;
        }

        private async void DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo)
        {
            // Create dialog
            string defaultFileName = $"{VideoInfo.Title}.{mediaStreamInfo.QualityLabel}.{mediaStreamInfo.FileExtension}";
            defaultFileName = defaultFileName.Except(Path.GetInvalidFileNameChars());
            string fileFilter =
                $"{mediaStreamInfo.ContainerType} Files|" +
                $"*.{mediaStreamInfo.FileExtension}|All files|*.*";
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = mediaStreamInfo.FileExtension,
                FileName = defaultFileName,
                Filter = fileFilter
            };

            // Select destination
            if (sfd.ShowDialog() != true) return;
            string filePath = sfd.FileName;

            // Download and save to file
            IsBusy = true;
            Progress = 0;

            var progressHandler = new Progress<double>(p => Progress = p);
            await _client.DownloadMediaStreamAsync(mediaStreamInfo, filePath, progressHandler);

            IsBusy = false;
            Progress = 0;
        }

        private async void DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo)
        {
            // Create dialog
            string defaultFileName = $"{VideoInfo.Title}.{closedCaptionTrackInfo.Culture.EnglishName}.srt";
            defaultFileName = defaultFileName.Except(Path.GetInvalidFileNameChars());
            string fileFilter =
                "SRT Files|*.srt|" +
                "All files|*.*";
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "srt",
                FileName = defaultFileName,
                Filter = fileFilter
            };
            // Select destination
            if (sfd.ShowDialog() != true) return;
            string filePath = sfd.FileName;

            // Download
            IsBusy = true;
            Progress = 0;

            var progressHandler = new Progress<double>(p => Progress = p);
            await _client.DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progressHandler);

            IsBusy = false;
            Progress = 0;
        }
    }
}