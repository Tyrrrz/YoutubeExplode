using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;
using YoutubeExplode.NetFx;

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
        public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }

        public MainViewModel(YoutubeClient client)
        {
            _client = client;

            // Commands
            GetVideoInfoCommand = new RelayCommand(GetVideoInfoAsync, () => !IsBusy);
            DownloadMediaStreamCommand = new RelayCommand<MediaStreamInfo>(DownloadMediaStreamAsync, vse => !IsBusy);
            DownloadClosedCaptionTrackCommand = new RelayCommand<ClosedCaptionTrackInfo>(DownloadClosedCaptionTrackAsync, vse => !IsBusy);
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

        private async void DownloadMediaStreamAsync(MediaStreamInfo mediaStreamInfo)
        {
            // Select destination
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = mediaStreamInfo.FileExtension,
                FileName = $"{VideoInfo.Title}.{mediaStreamInfo.FileExtension}".Except(Path.GetInvalidFileNameChars()),
                Filter = $"{mediaStreamInfo.FileExtension.ToUpperInvariant()} Files|*.{mediaStreamInfo.FileExtension}|All files|*.*"
            };
            if (sfd.ShowDialog() == false) return;
            string filePath = sfd.FileName;

            // Download and save to file
            IsBusy = true;
            Progress = 0;
            using (var input = await _client.GetMediaStreamAsync(mediaStreamInfo))
            using (var output = File.Create(filePath))
            {
                // Read the response and copy it to output stream
                var buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = await input.ReadAsync(buffer, 0, buffer.Length);
                    await output.WriteAsync(buffer, 0, bytesRead);

                    if (mediaStreamInfo.FileSize > 0)
                        Progress += 1.0*bytesRead/mediaStreamInfo.FileSize;
                } while (bytesRead > 0);
            }

            Progress = 0;
            IsBusy = false;
        }

        private async void DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo)
        {
            // Select destination
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "srt",
                FileName = $"{VideoInfo.Title}.{closedCaptionTrackInfo.Culture.EnglishName}.srt".Except(Path.GetInvalidFileNameChars()),
                Filter = "SRT Files|*.srt|All files|*.*"
            };
            if (sfd.ShowDialog() == false) return;
            string filePath = sfd.FileName;

            // Download
            IsBusy = true;
            IsProgressIndeterminate = true;
            await _client.DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath);
            IsProgressIndeterminate = false;
            IsBusy = false;
        }
    }
}