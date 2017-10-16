using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Models;

namespace YoutubeExplode.Tests
{
    [TestClass]
    public partial class IntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds.csv",
            "Integration_VideoIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_CheckVideoExistsAsync_Existing_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var exists = await client.CheckVideoExistsAsync(id);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds_NonExisting.csv",
            "Integration_VideoIds_NonExisting#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_CheckVideoExistsAsync_NonExisting_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var exists = await client.CheckVideoExistsAsync(id);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds.csv",
            "Integration_VideoIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetVideoAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var video = await client.GetVideoAsync(id);

            Assert.AreEqual(id, video.Id);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds_NonExisting.csv",
            "Integration_VideoIds_NonExisting#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetVideoAsync_NonExisting_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<VideoNotAvailableException>(() => client.GetVideoAsync(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds_Paid.csv",
            "Integration_VideoIds_Paid#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetVideoAsync_Paid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            await Assert.ThrowsExceptionAsync<VideoRequiresPurchaseException>(() => client.GetVideoAsync(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_PlaylistIds.csv",
            "Integration_PlaylistIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetPlaylistAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            var minVideoCount = (int) TestContext.DataRow["MinVideoCount"];

            var client = new YoutubeClient();
            var playlist = await client.GetPlaylistAsync(id);

            Assert.AreEqual(id, playlist.Id);
            Assert.IsTrue(minVideoCount <= playlist.Videos.Count);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_PlaylistIds.csv",
            "Integration_PlaylistIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetPlaylistAsync_Truncated_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            var pageLimit = 1;

            var client = new YoutubeClient();
            var playlist = await client.GetPlaylistAsync(id, pageLimit);

            Assert.AreEqual(id, playlist.Id);
            Assert.IsTrue(200 * pageLimit >= playlist.Videos.Count);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_ChannelIds.csv",
            "Integration_ChannelIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetChannelAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var channel = await client.GetChannelAsync(id);

            Assert.IsNotNull(channel);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_ChannelIds.csv",
            "Integration_ChannelIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetChannelUploadsAsync_Test()
        {
            var id = (string)TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var videos = await client.GetChannelUploadsAsync(id);

            Assert.IsNotNull(videos);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds.csv",
            "Integration_VideoIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetMediaStreamAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var video = await client.GetVideoAsync(id);

            foreach (var streamInfo in video.GetAllMediaStreamInfos())
            {
                using (var stream = await client.GetMediaStreamAsync(streamInfo))
                {
                    Assert.IsNotNull(stream);

                    var buffer = new byte[100];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds_HasCC.csv",
            "Integration_VideoIds_HasCC#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_GetClosedCaptionTrackAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var video = await client.GetVideoAsync(id);

            var trackInfo = video.ClosedCaptionTrackInfos.First();
            var track = await client.GetClosedCaptionTrackAsync(trackInfo);

            Assert.IsNotNull(track);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds.csv",
            "Integration_VideoIds#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_DownloadMediaStreamAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var video = await client.GetVideoAsync(id);

            var streamInfo = video.AudioStreamInfos.OrderBy(s => s.Size).First();
            var outputFilePath = Path.Combine(Shared.TempDirectoryPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(Shared.TempDirectoryPath);
            await client.DownloadMediaStreamAsync(streamInfo, outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);
            Assert.IsTrue(fileInfo.Exists);
            Assert.IsTrue(0 < fileInfo.Length);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Integration_VideoIds_HasCC.csv",
            "Integration_VideoIds_HasCC#csv", DataAccessMethod.Sequential)]
        public async Task YoutubeClient_DownloadClosedCaptionTrackAsync_Test()
        {
            var id = (string) TestContext.DataRow["Id"];

            var client = new YoutubeClient();
            var video = await client.GetVideoAsync(id);

            var streamInfo = video.ClosedCaptionTrackInfos.First();
            var outputFilePath = Path.Combine(Shared.TempDirectoryPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(Shared.TempDirectoryPath);
            await client.DownloadClosedCaptionTrackAsync(streamInfo, outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);
            Assert.IsTrue(fileInfo.Exists);
            Assert.IsTrue(0 < fileInfo.Length);
        }
    }

    public partial class IntegrationTests
    {
        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (Directory.Exists(Shared.TempDirectoryPath))
                Directory.Delete(Shared.TempDirectoryPath, true);
        }
    }
}