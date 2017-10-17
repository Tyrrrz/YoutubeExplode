using System;
using NUnit.Framework;

namespace YoutubeExplode.Tests
{
    [TestFixture]
    public class ValidationTests
    {
        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetVideoIds_Valid))]
        public void YoutubeClient_ValidateVideoId_Valid_Test(string videoId)
        {
            var isValid = YoutubeClient.ValidateVideoId(videoId);

            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetVideoIds_Invalid))]
        public void YoutubeClient_ValidateVideoId_Invalid_Test(string videoId)
        {
            var isValid = YoutubeClient.ValidateVideoId(videoId);

            Assert.That(isValid, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetVideoUrls_Valid))]
        public void YoutubeClient_TryParseVideoId_Valid_Test(string videoUrl, string expectedVideoId)
        {
            var success = YoutubeClient.TryParseVideoId(videoUrl, out string videoId);

            Assert.That(success, Is.True);
            Assert.That(videoId, Is.EqualTo(expectedVideoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetVideoUrls_Invalid))]
        public void YoutubeClient_TryParseVideoId_Invalid_Test(string videoUrl)
        {
            var success = YoutubeClient.TryParseVideoId(videoUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetVideoUrls_Valid))]
        public void YoutubeClient_ParseVideoId_Valid_Test(string videoUrl, string expectedVideoId)
        {
            var videoId = YoutubeClient.ParseVideoId(videoUrl);

            Assert.That(videoId, Is.EqualTo(expectedVideoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetVideoUrls_Invalid))]
        public void YoutubeClient_ParseVideoId_Invalid_Test(string videoUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParseVideoId(videoUrl));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetPlaylistIds_Valid))]
        public void YoutubeClient_ValidatePlaylistId_Valid_Test(string playlistId)
        {
            var isValid = YoutubeClient.ValidatePlaylistId(playlistId);

            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetPlaylistIds_Invalid))]
        public void YoutubeClient_ValidatePlaylistId_Invalid_Test(string playlistId)
        {
            var isValid = YoutubeClient.ValidatePlaylistId(playlistId);

            Assert.That(isValid, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetPlaylistUrls_Valid))]
        public void YoutubeClient_TryParsePlaylistId_Valid_Test(string playlistUrl, string expectedPlaylistId)
        {
            var success = YoutubeClient.TryParsePlaylistId(playlistUrl, out string playlistId);

            Assert.That(success, Is.True);
            Assert.That(playlistId, Is.EqualTo(expectedPlaylistId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetPlaylistUrls_Invalid))]
        public void YoutubeClient_TryParsePlaylistId_Invalid_Test(string playlistUrl)
        {
            var success = YoutubeClient.TryParsePlaylistId(playlistUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetPlaylistUrls_Valid))]
        public void YoutubeClient_ParsePlaylistId_Valid_Test(string playlistUrl, string expectedPlaylistId)
        {
            var playlistId = YoutubeClient.ParsePlaylistId(playlistUrl);

            Assert.That(playlistId, Is.EqualTo(expectedPlaylistId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetPlaylistUrls_Invalid))]
        public void YoutubeClient_ParsePlaylistId_Invalid_Test(string playlistUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParsePlaylistId(playlistUrl));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetChannelIds_Valid))]
        public void YoutubeClient_ValidateChannelId_Valid_Test(string channelId)
        {
            var isValid = YoutubeClient.ValidateChannelId(channelId);

            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetChannelIds_Invalid))]
        public void YoutubeClient_ValidateChannelId_Invalid_Test(string channelId)
        {
            var isValid = YoutubeClient.ValidateChannelId(channelId);

            Assert.That(isValid, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetChannelUrls_Valid))]
        public void YoutubeClient_TryParseChannelId_Valid_Test(string channelUrl, string expectedChannelId)
        {
            var success = YoutubeClient.TryParseChannelId(channelUrl, out string channelId);

            Assert.That(success, Is.True);
            Assert.That(channelId, Is.EqualTo(expectedChannelId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetChannelUrls_Invalid))]
        public void YoutubeClient_TryParseChannelId_Invalid_Test(string channelUrl)
        {
            var success = YoutubeClient.TryParseChannelId(channelUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetChannelUrls_Valid))]
        public void YoutubeClient_ParseChannelId_Valid_Test(string channelUrl, string expectedChannelId)
        {
            var channelId = YoutubeClient.ParseChannelId(channelUrl);

            Assert.That(channelId, Is.EqualTo(expectedChannelId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Validation_GetChannelUrls_Invalid))]
        public void YoutubeClient_ParseChannelId_Invalid_Test(string channelUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParseChannelId(channelUrl));
        }
    }
}