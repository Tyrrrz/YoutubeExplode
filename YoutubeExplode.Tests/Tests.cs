using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Tests
{
    // TODO: rework

    [TestFixture]
    public class Tests
    {
        private readonly string _tempDirPath;

        public Tests()
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
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available))]
        public async Task YoutubeClient_GetVideoAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(videoId);

            Assert.That(video, Is.Not.Null);
            Assert.That(video.Id.ToString(), Is.EqualTo(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Unavailable))]
        public void YoutubeClient_GetVideoAsync_Unavailable_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            Assert.CatchAsync<VideoUnavailableException>(() => youtube.Videos.GetAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available))]
        public async Task YoutubeClient_GetVideoAuthorChannelAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var channel = await youtube.Channels.GetByVideoAsync(videoId);

            Assert.That(channel, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Unavailable))]
        public void YoutubeClient_GetVideoAuthorChannelAsync_Unavailable_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            Assert.CatchAsync<VideoUnavailableException>(() => youtube.Channels.GetByVideoAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Playable))]
        public async Task YoutubeClient_GetVideoMediaStreamInfosAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            Assert.That(manifest, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Unavailable))]
        public void YoutubeClient_GetVideoMediaStreamInfosAsync_Unavailable_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            Assert.CatchAsync<VideoUnavailableException>(() => youtube.Videos.Streams.GetManifestAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Unplayable))]
        public void YoutubeClient_GetVideoMediaStreamInfosAsync_Unplayable_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            Assert.CatchAsync<VideoUnplayableException>(() => youtube.Videos.Streams.GetManifestAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Unplayable_RequiresPurchase))]
        public void YoutubeClient_GetVideoMediaStreamInfosAsync_Unplayable_RequiresPurchase_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            Assert.CatchAsync<VideoRequiresPurchaseException>(() => youtube.Videos.Streams.GetManifestAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Playable))]
        public async Task YoutubeClient_GetVideoClosedCaptionTrackInfosAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            Assert.That(trackManifest, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Unavailable))]
        public void YoutubeClient_GetVideoClosedCaptionTrackInfosAsync_Unavailable_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            Assert.CatchAsync<VideoUnavailableException>(() => youtube.Videos.ClosedCaptions.GetManifestAsync(videoId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Playable))]
        public async Task YoutubeClient_GetMediaStreamAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            Assert.That(manifest.Streams, Is.Not.Empty);

            foreach (var streamInfo in manifest.Streams)
            {
                await using var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                Assert.That(stream, Is.Not.Null);

                var buffer = new byte[100];
                await stream.ReadAsync(buffer, 0, buffer.Length);
            }
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Playable))]
        public async Task YoutubeClient_DownloadMediaStreamAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            var streamInfo = manifest.GetAudio().OrderBy(s => s.Size).First();
            var outputFilePath = Path.Combine(_tempDirPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
            await youtube.Videos.Streams.DownloadAsync(streamInfo, outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.EqualTo(streamInfo.Size.TotalBytes));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Playable_WithClosedCaptions))]
        public async Task YoutubeClient_GetClosedCaptionTrackAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            Assert.That(manifest.Tracks, Is.Not.Empty);

            foreach (var trackInfo in manifest.Tracks)
            {
                var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

                Assert.That(track, Is.Not.Null);
            }
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoIds_Valid_Available_Playable_WithClosedCaptions))]
        public async Task YoutubeClient_DownloadClosedCaptionTrackAsync_Test(string videoId)
        {
            var youtube = new YoutubeClient();

            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            var outputFilePath = Path.Combine(_tempDirPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
            await youtube.Videos.ClosedCaptions.DownloadAsync(manifest.Tracks.First(), outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistIds_Valid))]
        public async Task YoutubeClient_GetPlaylistAsync_Test(string playlistId)
        {
            var youtube = new YoutubeClient();

            var playlist = await youtube.Playlists.GetAsync(playlistId);

            Assert.That(playlist, Is.Not.Null);
            Assert.That(playlist.Id.ToString(), Is.EqualTo(playlistId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetPlaylistIds_Valid))]
        public async Task YoutubeClient_GetPlaylistAsync_Truncated_Test(string playlistId)
        {
            var youtube = new YoutubeClient();

            var videos = await youtube.Playlists.GetVideosAsync(playlistId).BufferAsync(100);

            Assert.That(videos, Is.Not.Null);
            Assert.That(videos.Count, Is.Not.Zero.And.LessThanOrEqualTo(100));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetUsernames_Valid))]
        public async Task YoutubeClient_GetChannelByUserAsync_Test(string username)
        {
            var youtube = new YoutubeClient();

            var channel = await youtube.Channels.GetByUserAsync(username);

            Assert.That(channel, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelIds_Valid))]
        public async Task YoutubeClient_GetChannelAsync_Test(string channelId)
        {
            var youtube = new YoutubeClient();

            var channel = await youtube.Channels.GetAsync(channelId);

            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.Id.ToString(), Is.EqualTo(channelId));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetChannelIds_Valid))]
        public async Task YoutubeClient_GetChannelUploadsAsync_Test(string channelId)
        {
            var youtube = new YoutubeClient();

            var videos = await youtube.Channels.GetUploadsAsync(channelId).BufferAsync(100);

            Assert.That(videos, Is.Not.Null);
            Assert.That(videos.Count, Is.Not.Zero.And.LessThanOrEqualTo(100));
        }

        [Test]
        [TestCaseSource(typeof(TestData), nameof(TestData.GetVideoSearchQueries))]
        public async Task YoutubeClient_SearchVideosAsync_Test(string query)
        {
            var youtube = new YoutubeClient();

            var videos = await youtube.Search.GetVideosAsync(query).BufferAsync(100);

            Assert.That(videos, Is.Not.Null);
            Assert.That(videos.Count, Is.Not.Zero.And.LessThanOrEqualTo(100));
        }
    }
}