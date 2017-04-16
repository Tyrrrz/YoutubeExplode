using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models;

namespace YoutubeExplode.Tests
{
    [TestClass]
    public partial class YoutubeClientTests
    {
        [TestMethod]
        public async Task CheckVideoExistsAsync_Guard_Test()
        {
            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.CheckVideoExistsAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => client.CheckVideoExistsAsync("invalid_id"));
        }

        [TestMethod]
        public async Task CheckVideoExistsAsync_Existing_Test()
        {
            var client = new YoutubeClient();
            bool exists = await client.CheckVideoExistsAsync("Te_dGvF6CcE");

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task CheckVideoExistsAsync_NonExisting_Test()
        {
            var client = new YoutubeClient();
            bool exists = await client.CheckVideoExistsAsync("qld9w0b-1ao");

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_Guard_Test()
        {
            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.GetVideoInfoAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => client.GetVideoInfoAsync("invalid_id"));
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_Normal_Test()
        {
            // Most common video type

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_QdPW8JrYzQ");

            Assert.That.IsSet(videoInfo);
            Assert.AreEqual("_QdPW8JrYzQ", videoInfo.Id);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_Signed_Test()
        {
            // Video that uses signature cipher

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("9bZkp7q19f0");

            Assert.That.IsSet(videoInfo);
            Assert.AreEqual("9bZkp7q19f0", videoInfo.Id);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_SignedRestricted_Test()
        {
            // Video that uses signature cipher and is also age-restricted

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("SkRSXFQerZs");

            Assert.That.IsSet(videoInfo);
            Assert.AreEqual("SkRSXFQerZs", videoInfo.Id);
        }

        [TestMethod]
        public async Task GetVideoInfoAsync_CannotEmbed_Test()
        {
            // Video that cannot be embedded outside of Youtube

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_kmeFXjjGfk");

            Assert.That.IsSet(videoInfo);
            Assert.AreEqual("_kmeFXjjGfk", videoInfo.Id);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Guard_Test()
        {
            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.GetPlaylistInfoAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => client.GetPlaylistInfoAsync("invalid_id"));
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => client.GetPlaylistInfoAsync("WL", 0));
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Normal_Test()
        {
            // Playlist created by a user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e");

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.UserMade, playlistInfo.Type);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Large_Test()
        {
            // Playlist created by a user with a lot of videos in it

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk");

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.UserMade, playlistInfo.Type);
            Assert.IsTrue(200 < playlistInfo.VideoIds.Count);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_LargeTruncated_Test()
        {
            // Playlist created by a user with a lot of videos in it

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk", 2);

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.UserMade, playlistInfo.Type);
            Assert.IsTrue(400 >= playlistInfo.VideoIds.Count);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_VideoMix_Test()
        {
            // Playlist generated by Youtube to group similar videos

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("RD1hu8-y6fKg0");

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("RD1hu8-y6fKg0", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.VideoMix, playlistInfo.Type);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_ChannelMix_Test()
        {
            // Playlist generated by Youtube to group uploads by same user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("ULl6WWX-BgIiE");

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("ULl6WWX-BgIiE", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.ChannelMix, playlistInfo.Type);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Liked_Test()
        {
            // System playlist for videos liked by a user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("LLEnBXANsKmyj2r9xVyKoDiQ");

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("LLEnBXANsKmyj2r9xVyKoDiQ", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.Liked, playlistInfo.Type);
        }

        [TestMethod]
        public async Task GetPlaylistInfoAsync_Favorites_Test()
        {
            // System playlist for videos favorited by a user

            var client = new YoutubeClient();
            var playlistInfo = await client.GetPlaylistInfoAsync("FLEnBXANsKmyj2r9xVyKoDiQ");

            Assert.That.IsSet(playlistInfo);
            Assert.AreEqual("FLEnBXANsKmyj2r9xVyKoDiQ", playlistInfo.Id);
            Assert.AreEqual(PlaylistType.Favorites, playlistInfo.Type);
        }

        [TestMethod]
        public async Task GetMediaStreamAsync_Guard_Test()
        {
            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.GetMediaStreamAsync(null));
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
                    Assert.That.IsSet(stream);

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
                    Assert.That.IsSet(stream);

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
                    Assert.That.IsSet(stream);

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
                    Assert.That.IsSet(stream);

                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        public async Task GetClosedCaptionTrackAsync_Guard_Test()
        {
            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.GetClosedCaptionTrackAsync(null));
        }

        [TestMethod]
        public async Task GetClosedCaptionTrackAsync_Test()
        {
            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoInfoAsync("_QdPW8JrYzQ");

            var trackInfo = videoInfo.ClosedCaptionTracks.FirstOrDefault(c => c.Culture.Name == "en");
            var track = await client.GetClosedCaptionTrackAsync(trackInfo);

            Assert.That.IsSet(track);

            var caption = track.GetByTime(TimeSpan.FromSeconds(40));

            Assert.IsNotNull(caption);
            Assert.AreEqual("I was looking at my phone.\nI thought, I could just delete this.", caption.Text);
        }
    }
}