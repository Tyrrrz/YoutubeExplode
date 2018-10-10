using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Tests
{
    [TestFixture]
    public class YoutubeClientIntegrationTests
    {
        private readonly string _tempDirPath;

        public YoutubeClientIntegrationTests()
        {
            var testDirPath = TestContext.CurrentContext.TestDirectory;
            _tempDirPath = Path.Combine(testDirPath, "Temp");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDirPath))
                Directory.Delete(_tempDirPath, true);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds))]
        public async Task YoutubeClient_GetVideoAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(videoId);

            Assert.That(video, Is.Not.Null);
            Assert.That(video.Id, Is.EqualTo(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_Unavailable))]
        public void YoutubeClient_GetVideoAsync_Unavailable_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoUnavailableException>(() => client.GetVideoAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds))]
        public async Task YoutubeClient_GetVideoAuthorChannelAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var channel = await client.GetVideoAuthorChannelAsync(videoId);

            Assert.That(channel, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_Unavailable))]
        public void YoutubeClient_GetVideoAuthorChannelAsync_Unavailable_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoUnavailableException>(() => client.GetVideoAuthorChannelAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds))]
        public async Task YoutubeClient_GetVideoMediaStreamInfosAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            Assert.That(streamInfoSet, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_Unavailable))]
        public void YoutubeClient_GetVideoMediaStreamInfosAsync_Unavailable_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoUnavailableException>(() => client.GetVideoMediaStreamInfosAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_RequiresPurchase))]
        public void YoutubeClient_GetVideoMediaStreamInfosAsync_RequiresPurchase_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoRequiresPurchaseException>(() => client.GetVideoMediaStreamInfosAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds))]
        public async Task YoutubeClient_GetVideoClosedCaptionTrackInfosAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var trackInfos = await client.GetVideoClosedCaptionTrackInfosAsync(videoId);

            Assert.That(trackInfos, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_Unavailable))]
        public void YoutubeClient_GetVideoClosedCaptionTrackInfosAsync_Unavailable_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoUnavailableException>(() => client.GetVideoClosedCaptionTrackInfosAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds))]
        public async Task YoutubeClient_GetMediaStreamAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            foreach (var streamInfo in mediaStreamInfoSet.GetAll())
            {
                using (var stream = await client.GetMediaStreamAsync(streamInfo))
                {
                    Assert.That(stream, Is.Not.Null);

                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds))]
        public async Task YoutubeClient_DownloadMediaStreamAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            var streamInfo = mediaStreamInfoSet.Audio.OrderBy(s => s.Size).First();
            var outputFilePath = Path.Combine(_tempDirPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
            await client.DownloadMediaStreamAsync(streamInfo, outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.EqualTo(streamInfo.Size));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_HasClosedCaptions))]
        public async Task YoutubeClient_GetClosedCaptionTrackAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var closedCaptionTrackInfos = await client.GetVideoClosedCaptionTrackInfosAsync(videoId);

            var trackInfo = closedCaptionTrackInfos.First();
            var track = await client.GetClosedCaptionTrackAsync(trackInfo);

            Assert.That(track, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoIds_HasClosedCaptions))]
        public async Task YoutubeClient_DownloadClosedCaptionTrackAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var closedCaptionTrackInfos = await client.GetVideoClosedCaptionTrackInfosAsync(videoId);

            var trackInfo = closedCaptionTrackInfos.First();
            var outputFilePath = Path.Combine(_tempDirPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
            await client.DownloadClosedCaptionTrackAsync(trackInfo, outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetPlaylistIds))]
        public async Task YoutubeClient_GetPlaylistAsync_Test(string playlistId)
        {
            // TODO: this should somehow verify video count

            var client = new YoutubeClient();

            var playlist = await client.GetPlaylistAsync(playlistId);

            Assert.That(playlist, Is.Not.Null);
            Assert.That(playlist.Id, Is.EqualTo(playlistId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetPlaylistIds))]
        public async Task YoutubeClient_GetPlaylistAsync_Truncated_Test(string playlistId)
        {
            const int pageLimit = 1;
            var client = new YoutubeClient();

            var playlist = await client.GetPlaylistAsync(playlistId, pageLimit);

            Assert.That(playlist, Is.Not.Null);
            Assert.That(playlist.Id, Is.EqualTo(playlistId));
            Assert.That(playlist.Videos, Is.Not.Null);
            Assert.That(playlist.Videos.Count, Is.LessThanOrEqualTo(200*pageLimit));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetUsernames))]
        public async Task YoutubeClient_GetChannelIdAsync_Test(string username)
        {
            var client = new YoutubeClient();

            var channelId = await client.GetChannelIdAsync(username);

            Assert.That(channelId, Is.Not.Null.Or.Empty);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetChannelIds))]
        public async Task YoutubeClient_GetChannelAsync_Test(string channelId)
        {
            var client = new YoutubeClient();

            var channel = await client.GetChannelAsync(channelId);

            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.Id, Is.EqualTo(channelId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetChannelIds))]
        public async Task YoutubeClient_GetChannelUploadsAsync_Test(string channelId)
        {
            var client = new YoutubeClient();

            var videos = await client.GetChannelUploadsAsync(channelId);

            Assert.That(videos, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.GetVideoSearchQueries))]
        public async Task YoutubeClient_SearchVideosAsync_Test(string query)
        {
            var client = new YoutubeClient();

            var videos = await client.SearchVideosAsync(query);

            Assert.That(videos, Is.Not.Null);
        }
    }
}