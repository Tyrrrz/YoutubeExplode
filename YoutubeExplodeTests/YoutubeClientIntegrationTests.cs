using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models;

namespace YoutubeExplode.Tests
{
    // These integration tests validate that YoutubeClient correctly works with actual data.
    // This includes parsing, deciphering, downloading, etc.
    // These tests are the primary means of detecting structural changes in youtube's front end.
    // Due to the nature of youtube, some data can be very inconsistent and unpredictable.
    // Because of that, consider running tests a few times to make sure. ;)

    [TestClass]
    public class YoutubeClientIntegrationTests
    {
        private YoutubeClient _client;

        [TestInitialize]
        public void Setup()
        {
            _client = new YoutubeClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
        }

        [TestMethod]
        public async Task CheckVideoExistsAsync_Existing_Test()
        {
            string videoId = "Te_dGvF6CcE";

            bool exists = await _client.CheckVideoExistsAsync(videoId);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task CheckVideoExistsAsync_NonExisting_Test()
        {
            string videoId = "qld9w0b-1ao";

            bool exists = await _client.CheckVideoExistsAsync(videoId);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_UnsignedUnrestricted_Test()
        {
            var videoInfo = await _client.GetVideoInfoAsync("LsNPjFXIPT8");

            Assert.IsNotNull(videoInfo);

            // Basic meta data
            Assert.AreEqual("LsNPjFXIPT8", videoInfo.Id);
            Assert.AreEqual("kyoumei no true force iyasine", videoInfo.Title);
            Assert.AreEqual("Tyrrrz", videoInfo.Author);
            Assert.IsTrue(103 <= videoInfo.Length.TotalSeconds);
            Assert.IsTrue(1 <= videoInfo.ViewCount);
            Assert.IsTrue(0 <= videoInfo.AverageRating);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            Assert.AreEqual(0, videoInfo.Keywords.Length);

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Length);

            // Flags
            Assert.IsFalse(videoInfo.HasClosedCaptions);
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(9 <= videoInfo.Streams.Length);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(VideoStreamQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(VideoStreamType.Unknown, streamInfo.Type);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.CaptionTracks);
            Assert.AreEqual(0, videoInfo.CaptionTracks.Length);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_SignedUnrestricted_Test()
        {
            var videoInfo = await _client.GetVideoInfoAsync("TZRvO0S-TLU");

            Assert.IsNotNull(videoInfo);

            // Basic meta data
            Assert.AreEqual("TZRvO0S-TLU", videoInfo.Id);
            Assert.AreEqual("BABYMETAL - THE ONE (OFFICIAL)", videoInfo.Title);
            Assert.AreEqual("BABYMETALofficial", videoInfo.Author);
            Assert.IsTrue(428 <= videoInfo.Length.TotalSeconds);
            Assert.IsTrue(4 <= videoInfo.AverageRating);
            Assert.IsTrue(6000000 <= videoInfo.ViewCount);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            Assert.AreEqual(30, videoInfo.Keywords.Length);

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Length);

            // Flags
            Assert.IsTrue(videoInfo.HasClosedCaptions);
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(22 <= videoInfo.Streams.Length);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(VideoStreamQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(VideoStreamType.Unknown, streamInfo.Type);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.CaptionTracks);
            Assert.AreEqual(1, videoInfo.CaptionTracks.Length);
            Assert.AreEqual("en", videoInfo.CaptionTracks[0].Language);
            Assert.IsFalse(videoInfo.CaptionTracks[0].IsAutoGenerated);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_SignedRestricted_Test()
        {
            var videoInfo = await _client.GetVideoInfoAsync("SkRSXFQerZs");

            Assert.IsNotNull(videoInfo);

            // Basic meta data
            Assert.AreEqual("SkRSXFQerZs", videoInfo.Id);
            Assert.AreEqual("HELLOVENUS 헬로비너스 - 위글위글(WiggleWiggle) M/V", videoInfo.Title);
            Assert.AreEqual("fantagio 판타지오", videoInfo.Author);
            Assert.IsTrue(203 <= videoInfo.Length.TotalSeconds);
            Assert.IsTrue(4 <= videoInfo.AverageRating);
            Assert.IsTrue(1200000 <= videoInfo.ViewCount);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            Assert.AreEqual(28, videoInfo.Keywords.Length);

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Length);

            // Flags
            Assert.IsFalse(videoInfo.HasClosedCaptions);
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(22 <= videoInfo.Streams.Length);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(VideoStreamQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(VideoStreamType.Unknown, streamInfo.Type);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.CaptionTracks);
            Assert.AreEqual(0, videoInfo.CaptionTracks.Length);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Test()
        {
            var playlistInfo = await _client.GetPlaylistInfoAsync("PLOU2XLYxmsII8UKqP84oaAxpwyryxbM-o");

            var expectedVideoIds = new[]
            {
                "zDAYZU4A3w0", "HgWHeT_OwHc", "axhdIa_co2o", "ZRvWqF2JmUg", "M_G_1SWVHgw",
                "LxwQeQCUplg", "WaKZ5pCKP6Y", "g_iig8sxsYc", "_xNFt7FsWaA", "H4vMcD7zKM0",
                "jgsKOc4skmY", "twC2viX7u6s", "MPhQ9VT6Rq4", "mBs8MQG-pp0", "HGdKHqMTAko",
                "WVc8iZyhezw", "h1Q5X-Uv0dw", "9nWyWwY2Onc", "jsznS0QxtYI", "LaGpoOgGip0",
                "AUW4ZEhhk_w", "qamtiWa-Cy4", "RK8K9nuRQPQ", "xT6tQAIywFQ", "6xV6aelL6fQ",
                "Ja2hxBAwG_0", "mJ5lNaLX5Bg", "8Lo3KZ1rZWw", "6Nv18xmJirs", "LTVFg6YOjWo",
                "8NbP07OEGsQ", "fqOpaCS117Q"
            };

            CollectionAssert.AreEqual(expectedVideoIds, playlistInfo.VideoIds);
        }

        [TestMethod]
        public async Task DownloadVideoAsync_UnsignedUnrestricted_Test()
        {
            var videoInfo = await _client.GetVideoInfoAsync("LsNPjFXIPT8");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await _client.DownloadVideoAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task DownloadVideoAsync_SignedUnrestricted_Test()
        {
            var videoInfo = await _client.GetVideoInfoAsync("9bZkp7q19f0");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await _client.DownloadVideoAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task DownloadVideoAsync_SignedRestricted_Test()
        {
            var videoInfo = await _client.GetVideoInfoAsync("SkRSXFQerZs");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await _client.DownloadVideoAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }
    }
}