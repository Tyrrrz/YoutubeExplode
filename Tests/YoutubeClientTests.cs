using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Tests
{
    [TestClass]
    public class YoutubeClientTests
    {
        [TestMethod]
        public void ValidateVideoId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidVideoIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsTrue(YoutubeClient.ValidateVideoId(id));
            }
        }

        [TestMethod]
        public void ValidateVideoId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidVideoIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsFalse(YoutubeClient.ValidateVideoId(id));
            }
        }

        [TestMethod]
        public void TryParseVideoId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidVideoUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr.SubstringUntil(";");
                string id = datastr.SubstringAfter(";");

                bool success = YoutubeClient.TryParseVideoId(url, out string actualId);
                Assert.IsTrue(success);
                Assert.AreEqual(id, actualId);
            }
        }

        [TestMethod]
        public void TryParseVideoId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidVideoUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr;

                bool success = YoutubeClient.TryParseVideoId(url, out _);
                Assert.IsFalse(success);
            }
        }

        [TestMethod]
        public void ParseVideoId_Guard_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => YoutubeClient.ParseVideoId(null));
        }

        [TestMethod]
        public void ParseVideoId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidVideoUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr.SubstringUntil(";");
                string id = datastr.SubstringAfter(";");

                string actualId = YoutubeClient.ParseVideoId(url);
                Assert.AreEqual(id, actualId);
            }
        }

        [TestMethod]
        public void ParseVideoId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidVideoUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr;

                Assert.ThrowsException<FormatException>(() => YoutubeClient.ParseVideoId(url));
            }
        }

        [TestMethod]
        public void ValidatePlaylistId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidPlaylistIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsTrue(YoutubeClient.ValidatePlaylistId(id));
            }
        }

        [TestMethod]
        public void ValidatePlaylistId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidPlaylistIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsFalse(YoutubeClient.ValidatePlaylistId(id));
            }
        }

        [TestMethod]
        public void TryParsePlaylistId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidPlaylistUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr.SubstringUntil(";");
                string id = datastr.SubstringAfter(";");

                bool success = YoutubeClient.TryParsePlaylistId(url, out string actualId);
                Assert.IsTrue(success);
                Assert.AreEqual(id, actualId);
            }
        }

        [TestMethod]
        public void TryParsePlaylistId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidPlaylistUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr;

                bool success = YoutubeClient.TryParsePlaylistId(url, out _);
                Assert.IsFalse(success);
            }
        }

        [TestMethod]
        public void ParsePlaylistId_Guard_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => YoutubeClient.ParsePlaylistId(null));
        }

        [TestMethod]
        public void ParsePlaylistId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidPlaylistUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr.SubstringUntil(";");
                string id = datastr.SubstringAfter(";");

                string actualId = YoutubeClient.ParsePlaylistId(url);
                Assert.AreEqual(id, actualId);
            }
        }

        [TestMethod]
        public void ParsePlaylistId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidPlaylistUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr;

                Assert.ThrowsException<FormatException>(() => YoutubeClient.ParsePlaylistId(url));
            }
        }

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

            var streams = new List<MediaStreamInfo>();
            streams.AddRange(videoInfo.MixedStreams);
            streams.AddRange(videoInfo.AudioStreams);
            streams.AddRange(videoInfo.VideoStreams);

            foreach (var streamInfo in streams)
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

            var streams = new List<MediaStreamInfo>();
            streams.AddRange(videoInfo.MixedStreams);
            streams.AddRange(videoInfo.AudioStreams);
            streams.AddRange(videoInfo.VideoStreams);

            foreach (var streamInfo in streams)
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

            var streams = new List<MediaStreamInfo>();
            streams.AddRange(videoInfo.MixedStreams);
            streams.AddRange(videoInfo.AudioStreams);
            streams.AddRange(videoInfo.VideoStreams);

            foreach (var streamInfo in streams)
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

            var streams = new List<MediaStreamInfo>();
            streams.AddRange(videoInfo.MixedStreams);
            streams.AddRange(videoInfo.AudioStreams);
            streams.AddRange(videoInfo.VideoStreams);

            foreach (var streamInfo in streams)
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

            var trackInfo = videoInfo.ClosedCaptionTracks.First();
            var track = await client.GetClosedCaptionTrackAsync(trackInfo);

            Assert.That.IsSet(track);
        }
    }
}