using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

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

        public static void IsSet(this Assert assert, VideoInfoSnippet videoInfoSnippet)
        {
            Assert.IsNotNull(videoInfoSnippet);

            Assert.That.IsNotBlank(videoInfoSnippet.Id);
            Assert.That.IsNotBlank(videoInfoSnippet.Title);
            Assert.IsNotNull(videoInfoSnippet.Description);
            Assert.IsNotNull(videoInfoSnippet.Keywords);
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

            Assert.IsNotNull(videoInfo.MixedStreams);
            Assert.IsNotNull(videoInfo.AudioStreams);
            Assert.IsNotNull(videoInfo.VideoStreams);

            foreach (var streamInfo in videoInfo.MixedStreams)
            {
                Assert.IsNotNull(streamInfo);
                Assert.That.IsNotBlank(streamInfo.Url);
                Assert.IsTrue(0 < streamInfo.ContentLength);
                Assert.That.IsNotBlank(streamInfo.VideoQualityLabel);
            }

            foreach (var streamInfo in videoInfo.AudioStreams)
            {
                Assert.IsNotNull(streamInfo);
                Assert.That.IsNotBlank(streamInfo.Url);
                Assert.IsTrue(0 < streamInfo.ContentLength);
                Assert.IsTrue(0 < streamInfo.Bitrate);
            }

            foreach (var streamInfo in videoInfo.VideoStreams)
            {
                Assert.IsNotNull(streamInfo);
                Assert.That.IsNotBlank(streamInfo.Url);
                Assert.IsTrue(0 < streamInfo.ContentLength);
                Assert.IsTrue(0 < streamInfo.Bitrate);
                Assert.IsTrue(0 < streamInfo.VideoFramerate);
                Assert.That.IsNotBlank(streamInfo.VideoQualityLabel);
            }

            Assert.IsNotNull(videoInfo.ClosedCaptionTracks);
            foreach (var captionTrack in videoInfo.ClosedCaptionTracks)
            {
                Assert.IsNotNull(captionTrack);
                Assert.That.IsNotBlank(captionTrack.Url);
                Assert.IsNotNull(captionTrack.Culture);
            }
        }

        public static void IsSet(this Assert assert, PlaylistInfo playlistInfo)
        {
            Assert.IsNotNull(playlistInfo);

            Assert.That.IsNotBlank(playlistInfo.Id);
            Assert.That.IsNotBlank(playlistInfo.Title);
            Assert.IsNotNull(playlistInfo.Author);
            Assert.IsNotNull(playlistInfo.Description);
            Assert.IsNotNull(playlistInfo.Videos);

            foreach (var video in playlistInfo.Videos)
                Assert.That.IsSet(video);
        }

        public static void IsSet(this Assert assert, MediaStream mediaStream)
        {
            Assert.IsNotNull(mediaStream);
            Assert.IsNotNull(mediaStream.Info);
            Assert.IsTrue(mediaStream.CanRead);
            Assert.AreEqual(mediaStream.Info.ContentLength, mediaStream.Length);
        }

        public static void IsSet(this Assert assert, ClosedCaptionTrack closedCaption)
        {
            Assert.IsNotNull(closedCaption);
            Assert.IsNotNull(closedCaption.Info);
            Assert.IsNotNull(closedCaption.Captions);

            foreach (var caption in closedCaption.Captions)
            {
                Assert.IsNotNull(caption);
                Assert.IsNotNull(caption.Text);
            }
        }
    }
}