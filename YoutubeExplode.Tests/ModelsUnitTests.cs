using System;
using System.Linq;
using NUnit.Framework;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Tests
{
    [TestFixture]
    public class ModelsUnitTests
    {
        private Video GetVideo()
        {
            var thumbnails = new ThumbnailSet("-TEST-TEST-");
            var statistics = new Statistics(1337, 13, 37);
            return new Video("-TEST-TEST-", "Test Author", DateTimeOffset.Now, "Test Video", "test", thumbnails,
                TimeSpan.FromMinutes(2), new string[0], statistics);
        }

        private Playlist GetPlaylist()
        {
            var videos = new[] {GetVideo()};
            var statistics = new Statistics(1337, 13, 37);
            return new Playlist("PLTESTTESTTESTTESTTESTTESTTESTTEST", "Test Author", "Test Playlist", "test",
                statistics, videos);
        }

        private Channel GetChannel()
        {
            return new Channel("UCTESTTESTTESTTESTTESTTE", "Test Channel", "test");
        }

        private ClosedCaptionTrack GetClosedCaptionTrack()
        {
            var info = new ClosedCaptionTrackInfo("test", new Language("en", "English (auto-generated)"), true);
            return new ClosedCaptionTrack(info,
                new[]
                {
                    new ClosedCaption("Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                    new ClosedCaption("world", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))
                });
        }

        [Test]
        public void ClosedCaptionTrack_GetByTime_Test()
        {
            var captionTrack = GetClosedCaptionTrack();

            var caption = captionTrack.GetByTime(TimeSpan.FromSeconds(0.5));

            Assert.That(caption, Is.Not.Null);
            Assert.That(caption.Text, Is.EqualTo("Hello"));
        }

        [Test]
        public void ClosedCaptionTrack_GetByTime_NonExisting_Test()
        {
            var captionTrack = GetClosedCaptionTrack();

            var caption = captionTrack.GetByTime(TimeSpan.FromSeconds(5));

            Assert.That(caption, Is.Null);
        }

        [Test]
        public void VideoResolution_Equality_Test()
        {
            var vr1 = new VideoResolution(800, 600);
            var vr2 = new VideoResolution(800, 600);
            var vr3 = new VideoResolution(640, 480);

            Assert.That(vr1, Is.EqualTo(vr2));
            Assert.That(vr2, Is.Not.EqualTo(vr3));
            Assert.That(vr1.GetHashCode(), Is.EqualTo(vr2.GetHashCode()));
            Assert.That(vr2.GetHashCode(), Is.Not.EqualTo(vr3.GetHashCode()));
        }

        [Theory]
        public void Extensions_Container_GetFileExtension_Test(Container container)
        {
            var ext = container.GetFileExtension();

            Assert.That(ext, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void Extensions_GetUrl_Video_Test()
        {
            var video = GetVideo();
            var url = video.GetUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParseVideoId(url), Is.EqualTo(video.Id));
        }

        [Test]
        public void Extensions_GetEmbedUrl_Video_Test()
        {
            var video = GetVideo();
            var url = video.GetEmbedUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParseVideoId(url), Is.EqualTo(video.Id));
        }

        [Test]
        public void Extensions_GetShortUrl_Video_Test()
        {
            var video = GetVideo();
            var url = video.GetShortUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParseVideoId(url), Is.EqualTo(video.Id));
        }

        [Test]
        public void Extensions_GetUrl_Playlist_Test()
        {
            var playlist = GetPlaylist();
            var url = playlist.GetUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParsePlaylistId(url), Is.EqualTo(playlist.Id));
        }

        [Test]
        public void Extensions_GetWatchUrl_Playlist_Test()
        {
            var playlist = GetPlaylist();
            var firstVideo = playlist.Videos.First();
            var url = playlist.GetWatchUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParsePlaylistId(url), Is.EqualTo(playlist.Id));
            Assert.That(YoutubeClient.ParseVideoId(url), Is.EqualTo(firstVideo.Id));
        }

        [Test]
        public void Extensions_GetEmbedUrl_Playlist_Test()
        {
            var playlist = GetPlaylist();
            var firstVideo = playlist.Videos.First();
            var url = playlist.GetEmbedUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParsePlaylistId(url), Is.EqualTo(playlist.Id));
            Assert.That(YoutubeClient.ParseVideoId(url), Is.EqualTo(firstVideo.Id));
        }

        [Test]
        public void Extensions_GetShortUrl_Playlist_Test()
        {
            var playlist = GetPlaylist();
            var firstVideo = playlist.Videos.First();
            var url = playlist.GetShortUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParsePlaylistId(url), Is.EqualTo(playlist.Id));
            Assert.That(YoutubeClient.ParseVideoId(url), Is.EqualTo(firstVideo.Id));
        }

        [Test]
        public void Extensions_GetUrl_Channel_Test()
        {
            var channel = GetChannel();
            var url = channel.GetUrl();

            Assert.That(url, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ParseChannelId(url), Is.EqualTo(channel.Id));
        }

        [Test]
        public void Extensions_GetVideoMixPlaylistId_Test()
        {
            var video = GetVideo();
            var playlistId = video.GetVideoMixPlaylistId();

            Assert.That(playlistId, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ValidatePlaylistId(playlistId), Is.True);
        }

        [Test]
        public void Extensions_GetChannelVideoMixPlaylistId_Test()
        {
            var video = GetVideo();
            var playlistId = video.GetChannelVideoMixPlaylistId();

            Assert.That(playlistId, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ValidatePlaylistId(playlistId), Is.True);
        }

        [Test]
        public void Extensions_GetChannelVideosPlaylistId_Test()
        {
            var channel = GetChannel();
            var playlistId = channel.GetChannelVideosPlaylistId();

            Assert.That(playlistId, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ValidatePlaylistId(playlistId), Is.True);
        }

        [Test]
        public void Extensions_GetPopularChannelVideosPlaylistId_Test()
        {
            var channel = GetChannel();
            var playlistId = channel.GetPopularChannelVideosPlaylistId();

            Assert.That(playlistId, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ValidatePlaylistId(playlistId), Is.True);
        }

        [Test]
        public void Extensions_GetLikedVideosPlaylistId_Test()
        {
            var channel = GetChannel();
            var playlistId = channel.GetLikedVideosPlaylistId();

            Assert.That(playlistId, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ValidatePlaylistId(playlistId), Is.True);
        }

        [Test]
        public void Extensions_GetFavoritesPlaylistId_Test()
        {
            var channel = GetChannel();
            var playlistId = channel.GetFavoritesPlaylistId();

            Assert.That(playlistId, Is.Not.Null.Or.Empty);
            Assert.That(YoutubeClient.ValidatePlaylistId(playlistId), Is.True);
        }
    }
}