using System.Diagnostics;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Tyrrrz.Extensions;
using YoutubeExplode.DemoWpf.ViewModels.Interfaces;
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
                DownloadVideoCommand.RaiseCanExecuteChanged();
            }
        }

        public string VideoId
        {
            get { return _videoId; }
            set { Set(ref _videoId, value); }
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
        public RelayCommand<VideoStreamInfo> OpenVideoCommand { get; }
        public RelayCommand<VideoStreamInfo> DownloadVideoCommand { get; }

        public MainViewModel(YoutubeClient client)
        {
            _client = client;

            // Commands
            GetVideoInfoCommand = new RelayCommand(GetVideoInfoAsync, () => !IsBusy);
            OpenVideoCommand = new RelayCommand<VideoStreamInfo>(vse => Process.Start(vse.Url));
            DownloadVideoCommand = new RelayCommand<VideoStreamInfo>(DownloadVideoAsync, vse => !IsBusy);
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
            string id;
            if (!YoutubeClient.TryParseVideoId(VideoId, out id))
                id = VideoId;

            // Perform the request
            VideoInfo = await _client.GetVideoInfoAsync(id);

            IsProgressIndeterminate = false;
            IsBusy = false;
        }

        private async void DownloadVideoAsync(VideoStreamInfo videoStreamInfo)
        {
            // Check params
            if (videoStreamInfo == null) return;
            if (VideoInfo == null) return;

            // Copy values
            string title = VideoInfo.Title;
            string ext = videoStreamInfo.FileExtension;

            // Select destination
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ext,
                FileName = $"{title}.{ext}".Without(Path.GetInvalidFileNameChars()),
                Filter = $"{ext.ToUpperInvariant()} Video Files|*.{ext}|All files|*.*"
            };
            if (sfd.ShowDialog() == false) return;
            string filePath = sfd.FileName;

            // Try download
            IsBusy = true;
            Progress = 0;
            using (var output = File.Create(filePath))
            using (var input = await _client.DownloadVideoAsync(videoStreamInfo))
            {
                // Read the response and copy it to output stream
                var buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = await input.ReadAsync(buffer, 0, buffer.Length);
                    await output.WriteAsync(buffer, 0, bytesRead);

                    if (videoStreamInfo.FileSize > 0)
                        Progress += 1.0*bytesRead/videoStreamInfo.FileSize;
                } while (bytesRead > 0);
            }

            Progress = 0;
            IsBusy = false;
        }
    }
}