using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode;

namespace Tests
{
    [TestClass]
    public class ValidationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_VideoIds_Valid.csv",
            "Validation_VideoIds_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ValidateVideoId_Valid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            Assert.IsTrue(YoutubeClient.ValidateVideoId(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_VideoIds_Invalid.csv",
            "Validation_VideoIds_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ValidateVideoId_Invalid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            Assert.IsFalse(YoutubeClient.ValidateVideoId(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_VideoUrls_Valid.csv",
            "Validation_VideoUrls_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_TryParseVideoId_Valid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            var id = (string) TestContext.DataRow["Id"];

            var success = YoutubeClient.TryParseVideoId(url, out string actualId);
            Assert.IsTrue(success);
            Assert.AreEqual(id, actualId);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_VideoUrls_Invalid.csv",
            "Validation_VideoUrls_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_TryParseVideoId_Invalid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];

            var success = YoutubeClient.TryParseVideoId(url, out _);
            Assert.IsFalse(success);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_VideoUrls_Valid.csv",
            "Validation_VideoUrls_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ParseVideoId_Valid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            var id = (string) TestContext.DataRow["Id"];

            var actualId = YoutubeClient.ParseVideoId(url);
            Assert.AreEqual(id, actualId);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_VideoUrls_Invalid.csv",
            "Validation_VideoUrls_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ParseVideoId_Invalid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            Assert.ThrowsException<FormatException>(() => YoutubeClient.ParseVideoId(url));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_PlaylistIds_Valid.csv",
            "Validation_PlaylistIds_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ValidatePlaylistId_Valid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            Assert.IsTrue(YoutubeClient.ValidatePlaylistId(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_PlaylistIds_Invalid.csv",
            "Validation_PlaylistIds_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ValidatePlaylistId_Invalid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            Assert.IsFalse(YoutubeClient.ValidatePlaylistId(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_PlaylistUrls_Valid.csv",
            "Validation_PlaylistUrls_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_TryParsePlaylistId_Valid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            var id = (string) TestContext.DataRow["Id"];

            var success = YoutubeClient.TryParsePlaylistId(url, out string actualId);
            Assert.IsTrue(success);
            Assert.AreEqual(id, actualId);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_PlaylistUrls_Invalid.csv",
            "Validation_PlaylistUrls_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_TryParsePlaylistId_Invalid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];

            var success = YoutubeClient.TryParsePlaylistId(url, out _);
            Assert.IsFalse(success);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_PlaylistUrls_Valid.csv",
            "Validation_PlaylistUrls_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ParsePlaylistId_Valid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            var id = (string) TestContext.DataRow["Id"];

            var actualId = YoutubeClient.ParsePlaylistId(url);
            Assert.AreEqual(id, actualId);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_PlaylistUrls_Invalid.csv",
            "Validation_PlaylistUrls_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ParsePlaylistId_Invalid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            Assert.ThrowsException<FormatException>(() => YoutubeClient.ParsePlaylistId(url));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_ChannelIds_Valid.csv",
            "Validation_ChannelIds_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ValidateChannelId_Valid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            Assert.IsTrue(YoutubeClient.ValidateChannelId(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_ChannelIds_Invalid.csv",
            "Validation_ChannelIds_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ValidateChannelId_Invalid_Test()
        {
            var id = (string) TestContext.DataRow["Id"];
            Assert.IsFalse(YoutubeClient.ValidateChannelId(id));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_ChannelUrls_Valid.csv",
            "Validation_ChannelUrls_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_TryParseChannelId_Valid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            var id = (string) TestContext.DataRow["Id"];

            var success = YoutubeClient.TryParseChannelId(url, out string actualId);
            Assert.IsTrue(success);
            Assert.AreEqual(id, actualId);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_ChannelUrls_Invalid.csv",
            "Validation_ChannelUrls_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_TryParseChannelId_Invalid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];

            var success = YoutubeClient.TryParseChannelId(url, out _);
            Assert.IsFalse(success);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_ChannelUrls_Valid.csv",
            "Validation_ChannelUrls_Valid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ParseChannelId_Valid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            var id = (string) TestContext.DataRow["Id"];

            var actualId = YoutubeClient.ParseChannelId(url);
            Assert.AreEqual(id, actualId);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "Data\\Validation_ChannelUrls_Invalid.csv",
            "Validation_ChannelUrls_Invalid#csv", DataAccessMethod.Sequential)]
        public void YoutubeClient_ParseChannelId_Invalid_Test()
        {
            var url = (string) TestContext.DataRow["Url"];
            Assert.ThrowsException<FormatException>(() => YoutubeClient.ParseChannelId(url));
        }
    }
}