using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;

namespace YoutubeExplode.Tests
{
    public static class Extensions
    {
        public static void IsNotBlank(this Assert assert, string str)
        {
            Assert.IsTrue(str.IsNotBlank());
        }

        public static void IsSet(this Assert assert, UserInfo userInfo)
        {
            Assert.IsNotNull(userInfo);
            Assert.That.IsNotBlank(userInfo.Id);
            Assert.That.IsNotBlank(userInfo.Name);
            Assert.That.IsNotBlank(userInfo.DisplayName);
            Assert.That.IsNotBlank(userInfo.ChannelTitle);
            Assert.That.IsNotBlank(userInfo.ChannelUrl);
            Assert.That.IsNotBlank(userInfo.ChannelBannerUrl);
            Assert.That.IsNotBlank(userInfo.ChannelLogoUrl);
        }

        public static void IsSet(this Assert assert, VideoInfo videoInfo)
        {
            Assert.IsNotNull(videoInfo);

            Assert.That.IsNotBlank(videoInfo.Id);
            Assert.That.IsNotBlank(videoInfo.Title);
            Assert.AreNotEqual(0, videoInfo.Duration.TotalSeconds);
            Assert.IsNotNull(videoInfo.Description);

            Assert.That.IsSet(videoInfo.Author);

            Assert.IsNotNull(videoInfo.Keywords);
            Assert.IsNotNull(videoInfo.Watermarks);

            Assert.IsNotNull(videoInfo.Streams);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.That.IsNotBlank(streamInfo.Url);
                Assert.AreNotEqual(MediaStreamVideoQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(MediaStreamContainerType.Unknown, streamInfo.ContainerType);
                Assert.That.IsNotBlank(streamInfo.QualityLabel);
                Assert.That.IsNotBlank(streamInfo.FileExtension);
                Assert.AreNotEqual(0, streamInfo.FileSize);
            }

            Assert.IsNotNull(videoInfo.ClosedCaptionTracks);
            foreach (var captionTrack in videoInfo.ClosedCaptionTracks)
            {
                Assert.That.IsNotBlank(captionTrack.Url);
                Assert.IsNotNull(captionTrack.Culture);
            }
        }

        public static void IsSet(this Assert assert, PlaylistInfo playlistInfo)
        {
            Assert.IsNotNull(playlistInfo);

            Assert.That.IsNotBlank(playlistInfo.Id);
            Assert.AreNotEqual(PlaylistType.Unknown, playlistInfo.Type);
            Assert.That.IsNotBlank(playlistInfo.Title);
            if (playlistInfo.Type != PlaylistType.VideoMix)
                Assert.IsNotNull(playlistInfo.Author);
            Assert.IsNotNull(playlistInfo.Description);
            Assert.IsNotNull(playlistInfo.VideoIds);
        }

        public static void IsSet(this Assert assert, MediaStream mediaStream)
        {
            Assert.IsNotNull(mediaStream);
            Assert.IsNotNull(mediaStream.Info);
            Assert.IsTrue(mediaStream.CanRead);
            Assert.AreNotEqual(0, mediaStream.Length);
        }

        public static void IsSet(this Assert assert, ClosedCaptionTrack closedCaption)
        {
            Assert.IsNotNull(closedCaption);
            Assert.IsNotNull(closedCaption.Info);
            Assert.IsNotNull(closedCaption.Captions);
        }
    }
}