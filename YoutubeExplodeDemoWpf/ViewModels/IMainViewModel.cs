using GalaSoft.MvvmLight.CommandWpf;
using YoutubeExplode.Models;

namespace YoutubeExplode.DemoWpf.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        string VideoId { get; set; }
        VideoInfo VideoInfo { get; }
        bool IsVideoInfoAvailable { get; }
        double Progress { get; }
        bool IsProgressIndeterminate { get; }

        RelayCommand GetVideoInfoCommand { get; }
        RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }
    }
}
