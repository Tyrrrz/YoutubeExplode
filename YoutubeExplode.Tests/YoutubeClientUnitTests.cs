using System;
using NUnit.Framework;

namespace YoutubeExplode.Tests
{
    [TestFixture]
    public class YoutubeClientUnitTests
    {
        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Invalid))]
        public void YoutubeClient_ValidateVideoId_Invalid_Test(string videoId)
        {
            var isValid = YoutubeClient.ValidateVideoId(videoId);

            Assert.That(isValid, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid))]
        public void YoutubeClient_ValidateVideoId_Test(string videoId)
        {
            var isValid = YoutubeClient.ValidateVideoId(videoId);

            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoUrls_Invalid))]
        public void YoutubeClient_TryParseVideoId_Invalid_Test(string videoUrl)
        {
            var success = YoutubeClient.TryParseVideoId(videoUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoUrls_Valid))]
        public void YoutubeClient_TryParseVideoId_Test(string videoUrl, string expectedVideoId)
        {
            var success = YoutubeClient.TryParseVideoId(videoUrl, out var videoId);

            Assert.That(success, Is.True);
            Assert.That(videoId, Is.EqualTo(expectedVideoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoUrls_Invalid))]
        public void YoutubeClient_ParseVideoId_Invalid_Test(string videoUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParseVideoId(videoUrl));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoUrls_Valid))]
        public void YoutubeClient_ParseVideoId_Test(string videoUrl, string expectedVideoId)
        {
            var videoId = YoutubeClient.ParseVideoId(videoUrl);

            Assert.That(videoId, Is.EqualTo(expectedVideoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistIds_Invalid))]
        public void YoutubeClient_ValidatePlaylistId_Invalid_Test(string playlistId)
        {
            var isValid = YoutubeClient.ValidatePlaylistId(playlistId);

            Assert.That(isValid, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistIds_Valid))]
        public void YoutubeClient_ValidatePlaylistId_Test(string playlistId)
        {
            var isValid = YoutubeClient.ValidatePlaylistId(playlistId);

            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistUrls_Invalid))]
        public void YoutubeClient_TryParsePlaylistId_Invalid_Test(string playlistUrl)
        {
            var success = YoutubeClient.TryParsePlaylistId(playlistUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistUrls_Valid))]
        public void YoutubeClient_TryParsePlaylistId_Test(string playlistUrl, string expectedPlaylistId)
        {
            var success = YoutubeClient.TryParsePlaylistId(playlistUrl, out var playlistId);

            Assert.That(success, Is.True);
            Assert.That(playlistId, Is.EqualTo(expectedPlaylistId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistUrls_Invalid))]
        public void YoutubeClient_ParsePlaylistId_Invalid_Test(string playlistUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParsePlaylistId(playlistUrl));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistUrls_Valid))]
        public void YoutubeClient_ParsePlaylistId_Test(string playlistUrl, string expectedPlaylistId)
        {
            var playlistId = YoutubeClient.ParsePlaylistId(playlistUrl);

            Assert.That(playlistId, Is.EqualTo(expectedPlaylistId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUsernames_Invalid))]
        public void YoutubeClient_ValidateUsername_Invalid_Test(string username)
        {
            var success = YoutubeClient.ValidateUsername(username);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUsernames_Valid))]
        public void YoutubeClient_ValidateUsername_Test(string username)
        {
            var success = YoutubeClient.ValidateUsername(username);

            Assert.That(success, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUserUrls_Invalid))]
        public void YoutubeClient_TryParseUsername_Invalid_Test(string userUrl)
        {
            var success = YoutubeClient.TryParseUsername(userUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUserUrls_Valid))]
        public void YoutubeClient_TryParseUsername_Test(string userUrl, string expectedUsername)
        {
            var success = YoutubeClient.TryParseUsername(userUrl, out var username);

            Assert.That(success, Is.True);
            Assert.That(username, Is.EqualTo(expectedUsername));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUserUrls_Invalid))]
        public void YoutubeClient_ParseUsername_Invalid_Test(string userUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParseUsername(userUrl));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUserUrls_Valid))]
        public void YoutubeClient_ParseUsername_Test(string userUrl, string expectedUsername)
        {
            var username = YoutubeClient.ParseUsername(userUrl);

            Assert.That(username, Is.EqualTo(expectedUsername));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelIds_Invalid))]
        public void YoutubeClient_ValidateChannelId_Invalid_Test(string channelId)
        {
            var isValid = YoutubeClient.ValidateChannelId(channelId);

            Assert.That(isValid, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelIds_Valid))]
        public void YoutubeClient_ValidateChannelId_Test(string channelId)
        {
            var isValid = YoutubeClient.ValidateChannelId(channelId);

            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelUrls_Invalid))]
        public void YoutubeClient_TryParseChannelId_Invalid_Test(string channelUrl)
        {
            var success = YoutubeClient.TryParseChannelId(channelUrl, out _);

            Assert.That(success, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelUrls_Valid))]
        public void YoutubeClient_TryParseChannelId_Test(string channelUrl, string expectedChannelId)
        {
            var success = YoutubeClient.TryParseChannelId(channelUrl, out var channelId);

            Assert.That(success, Is.True);
            Assert.That(channelId, Is.EqualTo(expectedChannelId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelUrls_Invalid))]
        public void YoutubeClient_ParseChannelId_Invalid_Test(string channelUrl)
        {
            Assert.Throws<FormatException>(() => YoutubeClient.ParseChannelId(channelUrl));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelUrls_Valid))]
        public void YoutubeClient_ParseChannelId_Test(string channelUrl, string expectedChannelId)
        {
            var channelId = YoutubeClient.ParseChannelId(channelUrl);

            Assert.That(channelId, Is.EqualTo(expectedChannelId));
        }
    }
}