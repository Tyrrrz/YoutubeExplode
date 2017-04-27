using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tyrrrz.Extensions;

namespace YoutubeExplode.Tests
{
    [TestClass]
    public class ValidationTests
    {
        [TestMethod]
        public void YoutubeClient_ValidateVideoId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidVideoIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsTrue(YoutubeClient.ValidateVideoId(id));
            }
        }

        [TestMethod]
        public void YoutubeClient_ValidateVideoId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidVideoIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsFalse(YoutubeClient.ValidateVideoId(id));
            }
        }

        [TestMethod]
        public void YoutubeClient_TryParseVideoId_Valid_Test()
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
        public void YoutubeClient_TryParseVideoId_Invalid_Test()
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
        public void YoutubeClient_ParseVideoId_Valid_Test()
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
        public void YoutubeClient_ParseVideoId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidVideoUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr;

                Assert.ThrowsException<FormatException>(() => YoutubeClient.ParseVideoId(url));
            }
        }

        [TestMethod]
        public void YoutubeClient_ValidatePlaylistId_Valid_Test()
        {
            var data = File.ReadAllLines("Data\\ValidPlaylistIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsTrue(YoutubeClient.ValidatePlaylistId(id));
            }
        }

        [TestMethod]
        public void YoutubeClient_ValidatePlaylistId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidPlaylistIds.txt");

            foreach (string datastr in data)
            {
                string id = datastr;

                Assert.IsFalse(YoutubeClient.ValidatePlaylistId(id));
            }
        }

        [TestMethod]
        public void YoutubeClient_TryParsePlaylistId_Valid_Test()
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
        public void YoutubeClient_TryParsePlaylistId_Invalid_Test()
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
        public void YoutubeClient_ParsePlaylistId_Valid_Test()
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
        public void YoutubeClient_ParsePlaylistId_Invalid_Test()
        {
            var data = File.ReadAllLines("Data\\InvalidPlaylistUrls.txt");

            foreach (string datastr in data)
            {
                string url = datastr;

                Assert.ThrowsException<FormatException>(() => YoutubeClient.ParsePlaylistId(url));
            }
        }
    }
}