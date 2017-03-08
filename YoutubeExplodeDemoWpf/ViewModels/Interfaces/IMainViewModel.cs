using GalaSoft.MvvmLight.CommandWpf;
using YoutubeExplode.Models;

namespace YoutubeExplode.DemoWpf.ViewModels.Interfaces
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
        RelayCommand<VideoStreamInfo> OpenVideoCommand { get; }
        RelayCommand<VideoStreamInfo> DownloadVideoCommand { get; }
    }
}
