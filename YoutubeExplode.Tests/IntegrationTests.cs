using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Models;

namespace YoutubeExplode.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private readonly string _tempDirPath;

        public IntegrationTests()
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
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds))]
        public async Task YoutubeClient_CheckVideoExistsAsync_Existing_Test(string videoId)
        {
            var client = new YoutubeClient();

            var exists = await client.CheckVideoExistsAsync(videoId);

            Assert.That(exists, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds_NonExisting))]
        public async Task YoutubeClient_CheckVideoExistsAsync_NonExisting_Test(string videoId)
        {
            var client = new YoutubeClient();

            var exists = await client.CheckVideoExistsAsync(videoId);

            Assert.That(exists, Is.Not.True);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds))]
        public async Task YoutubeClient_GetVideoAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(videoId);

            Assert.That(video.Id, Is.EqualTo(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds_NonExisting))]
        public void YoutubeClient_GetVideoAsync_NonExisting_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoNotAvailableException>(() => client.GetVideoAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds_Paid))]
        public void YoutubeClient_GetVideoAsync_Paid_Test(string videoId)
        {
            var client = new YoutubeClient();

            Assert.ThrowsAsync<VideoRequiresPurchaseException>(() => client.GetVideoAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds))]
        public async Task YoutubeClient_GetMediaStreamAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(videoId);

            foreach (var streamInfo in video.GetAllMediaStreamInfos())
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
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds_WithCC))]
        public async Task YoutubeClient_GetClosedCaptionTrackAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(videoId);
            var trackInfo = video.ClosedCaptionTrackInfos.First();
            var track = await client.GetClosedCaptionTrackAsync(trackInfo);

            Assert.That(track, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds))]
        public async Task YoutubeClient_DownloadMediaStreamAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(videoId);
            var streamInfo = video.AudioStreamInfos.OrderBy(s => s.Size).First();
            var outputFilePath = Path.Combine(_tempDirPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
            await client.DownloadMediaStreamAsync(streamInfo, outputFilePath);
            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetVideoIds_WithCC))]
        public async Task YoutubeClient_DownloadClosedCaptionTrackAsync_Test(string videoId)
        {
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(videoId);
            var streamInfo = video.ClosedCaptionTrackInfos.First();
            var outputFilePath = Path.Combine(_tempDirPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
            await client.DownloadClosedCaptionTrackAsync(streamInfo, outputFilePath);
            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetPlaylistIds))]
        public async Task YoutubeClient_GetPlaylistAsync_Test(string playlistId)
        {
            // TODO: this should somehow verify video count

            var client = new YoutubeClient();

            var playlist = await client.GetPlaylistAsync(playlistId);

            Assert.That(playlist.Id, Is.EqualTo(playlistId));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetPlaylistIds))]
        public async Task YoutubeClient_GetPlaylistAsync_Truncated_Test(string playlistId)
        {
            const int pageLimit = 1;
            var client = new YoutubeClient();

            var playlist = await client.GetPlaylistAsync(playlistId, pageLimit);

            Assert.That(playlist.Id, Is.EqualTo(playlistId));
            Assert.That(playlist.Videos.Count, Is.LessThanOrEqualTo(200*pageLimit));
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetChannelIds))]
        public async Task YoutubeClient_GetChannelAsync_Test(string channelId)
        {
            var client = new YoutubeClient();

            var channel = await client.GetChannelAsync(channelId);

            Assert.That(channel, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(Data), nameof(Data.Integration_GetChannelIds))]
        public async Task YoutubeClient_GetChannelUploadsAsync_Test(string channelId)
        {
            var client = new YoutubeClient();

            var videos = await client.GetChannelUploadsAsync(channelId);

            Assert.That(videos, Is.Not.Null);
        }
    }
}