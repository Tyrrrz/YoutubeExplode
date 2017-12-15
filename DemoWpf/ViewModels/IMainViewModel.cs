using System.Collections.Generic;
using GalaSoft.MvvmLight.CommandWpf;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace DemoWpf.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        string Query { get; set; }

        Video Video { get; }
        Channel Channel { get; }
        MediaStreamInfoSet MediaStreamInfos { get; }
        IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTrackInfos { get; }
        bool IsDataAvailable { get; }

        double Progress { get; }
        bool IsProgressIndeterminate { get; }

        RelayCommand GetDataCommand { get; }
        RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
        RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }
    }
}
