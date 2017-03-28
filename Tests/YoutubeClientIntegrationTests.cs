using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models;

namespace YoutubeExplode.Tests
{
    // These integration tests validate that YoutubeClient correctly works with actual data.
    // This includes parsing, deciphering, downloading, etc.
    // These tests are the primary means of detecting structural changes in Youtube's front end.
    // Due to the nature of Youtube, some data can be very inconsistent and unpredictable.
    // Because of that, consider running tests a few times to make sure. ;)

    [TestClass]
    public class YoutubeClientIntegrationTests
    {
        [TestMethod]
        public async Task CheckVideoExistsAsync_Existing_Test()
        {
            string videoId = "Te_dGvF6CcE";

            var client = new YoutubeClient();
            bool exists = await client.CheckVideoExistsAsync(videoId);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task CheckVideoExistsAsync_NonExisting_Test()
        {
            string videoId = "qld9w0b-1ao";

            var client = new YoutubeClient();
            bool exists = await client.CheckVideoExistsAsync(videoId);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_Normal_Test()
        {
            // Most common video type

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_QdPW8JrYzQ");

            Assert.IsNotNull(videoInfo);

            // Basic metadata
            Assert.AreEqual("_QdPW8JrYzQ", videoInfo.Id);
            Assert.AreEqual("This is what happens when you reply to spam email | James Veitch", videoInfo.Title);
            Assert.IsTrue(588 <= videoInfo.Duration.TotalSeconds);
            Assert.AreEqual(1659729120, videoInfo.Description.GetStaticHashCode());
            Assert.IsTrue(10000000 <= videoInfo.ViewCount);
            Assert.IsTrue(263000 <= videoInfo.LikeCount);
            Assert.IsTrue(6000 <= videoInfo.DislikeCount);

            // Author
            Assert.IsNotNull(videoInfo.Author);
            Assert.AreEqual("UCAuUUnT6oDeKwE6v1NGQxug", videoInfo.Author.Id);
            Assert.AreEqual("TEDtalksDirector", videoInfo.Author.Name);
            Assert.AreEqual("TED", videoInfo.Author.DisplayName);
            Assert.AreEqual("TED", videoInfo.Author.ChannelTitle);
            Assert.IsFalse(videoInfo.Author.IsPaid);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            CollectionAssert.AreEqual(
                new[] {"TED Talk", "TED Talks", "James Veitch", "spam", "humor", "comedy"},
                videoInfo.Keywords.ToArray());

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Count);

            // Flags
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(20 <= videoInfo.Streams.Count);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(MediaStreamVideoQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(MediaStreamContainerType.Unknown, streamInfo.ContainerType);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.ClosedCaptionTracks);
            Assert.IsTrue(42 <= videoInfo.ClosedCaptionTracks.Count);
            foreach (var captionTrack in videoInfo.ClosedCaptionTracks)
            {
                Assert.IsNotNull(captionTrack.Url);
                Assert.IsNotNull(captionTrack.Culture);
            }
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_Signed_Test()
        {
            // Video that uses signature cipher

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("9bZkp7q19f0");

            Assert.IsNotNull(videoInfo);

            // Basic metadata
            Assert.AreEqual("9bZkp7q19f0", videoInfo.Id);
            Assert.AreEqual("PSY - GANGNAM STYLE(강남스타일) M/V", videoInfo.Title);
            Assert.IsTrue(252 <= videoInfo.Duration.TotalSeconds);
            Assert.AreEqual(2103509192, videoInfo.Description.GetStaticHashCode());
            Assert.IsTrue(2750000000 <= videoInfo.ViewCount);
            Assert.IsTrue(12000000 <= videoInfo.LikeCount);
            Assert.IsTrue(1700000 <= videoInfo.DislikeCount);

            // Author
            Assert.IsNotNull(videoInfo.Author);
            Assert.AreEqual("UCrDkAvwZum-UTjHmzDI2iIw", videoInfo.Author.Id);
            Assert.AreEqual("officialpsy", videoInfo.Author.Name);
            Assert.AreEqual("officialpsy", videoInfo.Author.DisplayName);
            Assert.AreEqual("officialpsy", videoInfo.Author.ChannelTitle);
            Assert.IsFalse(videoInfo.Author.IsPaid);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            CollectionAssert.AreEqual(
                new[]
                {
                    "PSY", "싸이", "강남스타일", "뮤직비디오", "Music Video", "Gangnam Style", "KOREAN SINGER", "KPOP",
                    "KOERAN WAVE", "PSY 6甲", "6th Studio Album", "싸이6집", "육갑", "YG Family", "YG Entertainment"
                }, videoInfo.Keywords.ToArray());

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Count);

            // Flags
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(28 <= videoInfo.Streams.Count);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(MediaStreamVideoQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(MediaStreamContainerType.Unknown, streamInfo.ContainerType);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.ClosedCaptionTracks);
            Assert.AreEqual(0, videoInfo.ClosedCaptionTracks.Count);
            foreach (var captionTrack in videoInfo.ClosedCaptionTracks)
            {
                Assert.IsNotNull(captionTrack.Url);
                Assert.IsNotNull(captionTrack.Culture);
            }
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_SignedRestricted_Test()
        {
            // Video that uses signature cipher and is also age-restricted

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("SkRSXFQerZs");

            Assert.IsNotNull(videoInfo);

            // Basic metadata
            Assert.AreEqual("SkRSXFQerZs", videoInfo.Id);
            Assert.AreEqual("HELLOVENUS 헬로비너스 - 위글위글(WiggleWiggle) M/V", videoInfo.Title);
            Assert.IsTrue(203 <= videoInfo.Duration.TotalSeconds);
            Assert.AreEqual(1475277231, videoInfo.Description.GetStaticHashCode());
            Assert.IsTrue(1200000 <= videoInfo.ViewCount);
            Assert.IsTrue(20000 <= videoInfo.LikeCount);
            Assert.IsTrue(2000 <= videoInfo.DislikeCount);

            // Author
            Assert.IsNotNull(videoInfo.Author);
            Assert.AreEqual("UC3Ea2-Ut3navgOlTfoq790g", videoInfo.Author.Id);
            Assert.AreEqual("fantagiomusic", videoInfo.Author.Name);
            Assert.AreEqual("fantagio 판타지오", videoInfo.Author.DisplayName);
            Assert.AreEqual("fantagio 판타지오", videoInfo.Author.ChannelTitle);
            Assert.IsFalse(videoInfo.Author.IsPaid);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            CollectionAssert.AreEqual(
                new[]
                {
                    "헬로비너스", "앨리스", "나라", "라임", "유영", "서영", "여름", "위글위글", "끈적끈적", "섹시", "섹시걸그룹", "섹시댄스", "위글댄스", "위글",
                    "HELLOVENUS", "ALICE", "NARA", "LIME", "YOOYOUNG", "SEOYOUNG", "YEOREUM", "WIGGLEWIGGLE", "WIGGLE",
                    "STICKYSTICKY", "STICKY", "SEXY", "SEXYDANCE", "WIGGLEDANCE"
                }, videoInfo.Keywords.ToArray());

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Count);

            // Flags
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(22 <= videoInfo.Streams.Count);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(MediaStreamVideoQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(MediaStreamContainerType.Unknown, streamInfo.ContainerType);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.ClosedCaptionTracks);
            Assert.AreEqual(0, videoInfo.ClosedCaptionTracks.Count);
            foreach (var captionTrack in videoInfo.ClosedCaptionTracks)
            {
                Assert.IsNotNull(captionTrack.Url);
                Assert.IsNotNull(captionTrack.Culture);
            }
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_CannotEmbed_Test()
        {
            // Video that cannot be embedded outside of Youtube

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_kmeFXjjGfk");

            Assert.IsNotNull(videoInfo);

            // Basic metadata
            Assert.AreEqual("_kmeFXjjGfk", videoInfo.Id);
            Assert.AreEqual("Cam'ron- Killa Kam (dirty)", videoInfo.Title);
            Assert.IsTrue(359 <= videoInfo.Duration.TotalSeconds);
            Assert.AreEqual(1462671500, videoInfo.Description.GetStaticHashCode());
            Assert.IsTrue(3600000 <= videoInfo.ViewCount);
            Assert.IsTrue(19000 <= videoInfo.LikeCount);
            Assert.IsTrue(1000 <= videoInfo.DislikeCount);

            // Author
            Assert.IsNotNull(videoInfo.Author);
            Assert.AreEqual("UCWvETAIKRu920YrnHMq0CAw", videoInfo.Author.Id);
            Assert.AreEqual("locoNsane", videoInfo.Author.Name);
            Assert.AreEqual("Ralph Arellano", videoInfo.Author.DisplayName);
            Assert.AreEqual("Ralph Arellano", videoInfo.Author.ChannelTitle);
            Assert.IsFalse(videoInfo.Author.IsPaid);

            // Keywords
            Assert.IsNotNull(videoInfo.Keywords);
            CollectionAssert.AreEqual(
                new[] {"cam'ron", "killa", "kam", "rap", "hip-hop"},
                videoInfo.Keywords.ToArray());

            // Watermarks
            Assert.IsNotNull(videoInfo.Watermarks);
            Assert.AreEqual(2, videoInfo.Watermarks.Count);

            // Flags
            Assert.IsTrue(videoInfo.IsEmbeddingAllowed);
            Assert.IsTrue(videoInfo.IsListed);
            Assert.IsTrue(videoInfo.IsRatingAllowed);
            Assert.IsFalse(videoInfo.IsMuted);

            // Streams
            Assert.IsNotNull(videoInfo.Streams);
            Assert.IsTrue(17 <= videoInfo.Streams.Count);
            foreach (var streamInfo in videoInfo.Streams)
            {
                Assert.IsNotNull(streamInfo.Url);
                Assert.AreNotEqual(MediaStreamVideoQuality.Unknown, streamInfo.Quality);
                Assert.AreNotEqual(MediaStreamContainerType.Unknown, streamInfo.ContainerType);
                Assert.IsNotNull(streamInfo.QualityLabel);
                Assert.IsNotNull(streamInfo.FileExtension);
                Assert.IsTrue(0 < streamInfo.FileSize);
            }

            // Captions
            Assert.IsNotNull(videoInfo.ClosedCaptionTracks);
            Assert.AreEqual(0, videoInfo.ClosedCaptionTracks.Count);
            foreach (var captionTrack in videoInfo.ClosedCaptionTracks)
            {
                Assert.IsNotNull(captionTrack.Url);
                Assert.IsNotNull(captionTrack.Culture);
            }
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Normal_Test()
        {
            // Playlist created by a user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e");

            Assert.IsNotNull(playlistInfo);

            // Metadata
            Assert.AreEqual("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.UserMade, playlistInfo.Type);
            Assert.AreEqual("Analytics Academy - Digital Analytics Fundamentals", playlistInfo.Title);
            Assert.AreEqual("Google Analytics", playlistInfo.Author);
            Assert.AreEqual(-1627795933, playlistInfo.Description.GetStaticHashCode());
            Assert.IsTrue(339000 <= playlistInfo.ViewCount);
            // Likes/dislikes not checked because I don't know where they come from

            // Video ids
            Assert.IsNotNull(playlistInfo.VideoIds);
            CollectionAssert.AreEqual(new[]
            {
                "uPZSSdkGQhM", "JbXNS3NjIfM", "fi0w57kr_jY", "xLJt5A-NeQI", "EpDA3XaELqs", "eyltEFyZ678", "TW3gx4t4944",
                "w9H_P2wAwSE", "OyixJ7A9phg", "dzwRzUEc_tA", "vEpq3nYeZBc", "4gYioQkIqKk", "xyh8iG5mRIs", "ORrYEEH_KPc",
                "ii0T5JUO2BY", "hgycbw6Beuc", "Dz-zgq6OqTI", "I1b4GT-GuEs", "dN3gkBBffhs", "8Kg-8ZjgLAQ", "E9zfpKsw6f8",
                "eBCw9sC5D40"
            }, playlistInfo.VideoIds.ToArray());
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Large_Test()
        {
            // Playlist created by a user with a lot of videos in it

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk");

            Assert.IsNotNull(playlistInfo);

            // Metadata
            Assert.AreEqual("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.UserMade, playlistInfo.Type);
            Assert.AreEqual("I Just Love This Music Rock, Pop, Soul Playlist 1", playlistInfo.Title);
            Assert.AreEqual("Tomas Nilsson", playlistInfo.Author);
            Assert.AreEqual(-1351967013, playlistInfo.Description.GetStaticHashCode());
            Assert.IsTrue(339000 <= playlistInfo.ViewCount);
            // Likes/dislikes not checked because I don't know where they come from

            // Video ids
            Assert.IsNotNull(playlistInfo.VideoIds);
            Assert.AreEqual(1338, playlistInfo.VideoIds.Count);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Mix_Test()
        {
            // Playlist generated by Youtube to group similar videos

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("RD1hu8-y6fKg0");

            Assert.IsNotNull(playlistInfo);

            // Metadata
            Assert.AreEqual("RD1hu8-y6fKg0", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.Mix, playlistInfo.Type);
            Assert.IsTrue(string.IsNullOrEmpty(playlistInfo.Author)); // mixes have no author
            Assert.IsTrue(string.IsNullOrEmpty(playlistInfo.Description)); // and no description
            Assert.IsTrue(29000 <= playlistInfo.ViewCount);
            // Likes/dislikes not checked because I don't know where they come from

            // Video ids (not predictable because it's a mix)
            Assert.IsNotNull(playlistInfo.VideoIds);
            Assert.IsTrue(20 <= playlistInfo.VideoIds.Count);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Liked_Test()
        {
            // System playlist for videos liked by a user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("LLEnBXANsKmyj2r9xVyKoDiQ");

            Assert.IsNotNull(playlistInfo);

            // Metadata
            Assert.AreEqual("LLEnBXANsKmyj2r9xVyKoDiQ", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.Liked, playlistInfo.Type);
            Assert.AreEqual("Tyrrrz", playlistInfo.Author);
            Assert.IsTrue(string.IsNullOrEmpty(playlistInfo.Description));
            Assert.IsTrue(10 <= playlistInfo.ViewCount);
            // Likes/dislikes not checked because I don't know where they come from

            // Video ids
            Assert.IsNotNull(playlistInfo.VideoIds);
            Assert.IsTrue(100 <= playlistInfo.VideoIds.Count);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Favorites_Test()
        {
            // System playlist for videos favorited by a user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("FLEnBXANsKmyj2r9xVyKoDiQ");

            Assert.IsNotNull(playlistInfo);

            // Metadata
            Assert.AreEqual("FLEnBXANsKmyj2r9xVyKoDiQ", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.Favorites, playlistInfo.Type);
            Assert.AreEqual("Tyrrrz", playlistInfo.Author);
            Assert.IsTrue(string.IsNullOrEmpty(playlistInfo.Description));
            Assert.IsTrue(40 <= playlistInfo.ViewCount);
            // Likes/dislikes not checked because I don't know where they come from

            // Video ids
            Assert.IsNotNull(playlistInfo.VideoIds);
            Assert.IsTrue(20 <= playlistInfo.VideoIds.Count);
        }

        [TestMethod]
        public async Task GetMediaStreamAsync_Normal_Test()
        {
            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_QdPW8JrYzQ");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await client.GetMediaStreamAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task GetMediaStreamAsync_Signed_Test()
        {
            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("9bZkp7q19f0");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await client.GetMediaStreamAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task GetMediaStreamAsync_SignedRestricted_Test()
        {
            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("SkRSXFQerZs");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await client.GetMediaStreamAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task GetMediaStreamAsync_CannotEmbed_Test()
        {
            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_kmeFXjjGfk");

            foreach (var streamInfo in videoInfo.Streams)
            {
                using (var stream = await client.GetMediaStreamAsync(streamInfo))
                {
                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task GetClosedCaptionTrackAsync_Test()
        {
            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_QdPW8JrYzQ");
            var trackInfo = videoInfo.ClosedCaptionTracks.FirstOrDefault(c => c.Culture.Name == "en");
            var track = await client.GetClosedCaptionTrackAsync(trackInfo);
            var caption = track.GetByTime(TimeSpan.FromSeconds(40));

            Assert.IsNotNull(track);
            Assert.IsNotNull(track.Captions);
            Assert.AreEqual("I was looking at my phone.\nI thought, I could just delete this.", caption.Text);
        }
    }
}