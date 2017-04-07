using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YoutubeExplode.Tests
{
    // These unit tests validate YoutubeClient's workflow in predictable manner.

    [TestClass]
    public class YoutubeClientUnitTests
    {
        [TestMethod]
        public void ValidateVideoId_Valid_Test()
        {
            string[] ids =
            {
                "cpm00Hv1Umg",
                "aI5pUqiVJdw",
                "9onx5sgnkPQ",
                "lg0s242Hg-8",
                "fIDyDVzlqN4",
                "JE1Gvzxfm1E",
                "OpV62-86Fv4",
                "UnUkNfX8v1E",
                "aGTz8o_fey8",
                "10V6xet5ODk"
            };

            foreach (string id in ids)
                Assert.IsTrue(YoutubeClient.ValidateVideoId(id));
        }

        [TestMethod]
        public void ValidateVideoId_Invalid_Test()
        {
            string[] ids =
            {
                null,
                "",
                "@pm!!Hv#Lmg",
                "lg0s242Hg#8",
                "f`DyDVzlqN`",
                "JE1Gv[]fm1E",
                "***62-86Fv4",
                "U  kNfX8v1E",
                "aGяк8o_fey8",
                "10Vあxet5ODk"
            };

            foreach (string id in ids)
                Assert.IsFalse(YoutubeClient.ValidateVideoId(id));
        }

        [TestMethod]
        public void ParseVideoId_Valid_Test()
        {
            string[] urls =
            {
                "https://www.youtube.com/watch?v=cpm00Hv1Umg",
                "https://youtu.be/yIVRs6YSbOM",
                "https://www.youtube.com.ua/watch?v=10V6xet5ODk",
                "https://www.youtube.co.il/watch?v=OpV62-86Fv4",
                "https://www.youtube.be/watch?v=Nsib94LHi9I&gl=BE",
                "https://www.youtube.co.kr/embed/Y_t4kg1z-sA"
            };
            string[] ids =
            {
                "cpm00Hv1Umg",
                "yIVRs6YSbOM",
                "10V6xet5ODk",
                "OpV62-86Fv4",
                "Nsib94LHi9I",
                "Y_t4kg1z-sA"
            };

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                string id = ids[i];
                string parsed = YoutubeClient.ParseVideoId(url);
                Assert.AreEqual(id, parsed);
            }
        }

        [TestMethod]
        public void TryParseVideoId_Valid_Test()
        {
            string[] urls =
            {
                "https://www.youtube.com/watch?v=cpm00Hv1Umg",
                "https://youtu.be/yIVRs6YSbOM",
                "https://www.youtube.com.ua/watch?v=10V6xet5ODk",
                "https://www.youtube.co.il/watch?v=OpV62-86Fv4",
                "https://www.youtube.be/watch?v=Nsib94LHi9I&gl=BE",
                "https://www.youtube.co.kr/embed/Y_t4kg1z-sA"
            };
            string[] ids =
            {
                "cpm00Hv1Umg",
                "yIVRs6YSbOM",
                "10V6xet5ODk",
                "OpV62-86Fv4",
                "Nsib94LHi9I",
                "Y_t4kg1z-sA"
            };

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                string id = ids[i];
                bool success = YoutubeClient.TryParseVideoId(url, out string parsed);
                Assert.IsTrue(success);
                Assert.AreEqual(id, parsed);
            }
        }

        [TestMethod]
        public void TryParseVideoId_Invalid_Test()
        {
            string[] urls =
            {
                null,
                "",
                "https://www.youtube.com",
                "https://www.youtube.com/watch?v=@pm!!Hv#Lmg",
                "https://www.youtube.com/qweasd?v=Nsib94LHi9I"
            };

            foreach (string url in urls)
            {
                bool success = YoutubeClient.TryParseVideoId(url, out string parsed);
                Assert.IsFalse(success);
            }
        }

        [TestMethod]
        public void ValidatePlaylistId_Valid_Test()
        {
            string[] ids =
            {
                "PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
                "WL",
                "RDE2NZB6E5-do",
                "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
            };

            foreach (string id in ids)
                Assert.IsTrue(YoutubeClient.ValidatePlaylistId(id));
        }

        [TestMethod]
        public void ValidatePlaylistId_Invalid_Test()
        {
            string[] ids =
            {
                null,
                "",
                "@pm!!Hv#Lmgeghdfhdh",
                "adadasd*asdadasd aa"
            };

            foreach (string id in ids)
                Assert.IsFalse(YoutubeClient.ValidatePlaylistId(id));
        }

        [TestMethod]
        public void ParsePlaylistId_Valid_Test()
        {
            string[] urls =
            {
                "https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
                "https://www.youtube.com/playlist?list=WL",
                "https://youtu.be/E2NZB6E5-do/?list=RDE2NZB6E5-do",
                "https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
            };
            string[] ids =
            {
                "PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
                "WL",
                "RDE2NZB6E5-do",
                "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
            };

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                string id = ids[i];
                string parsed = YoutubeClient.ParsePlaylistId(url);
                Assert.AreEqual(id, parsed);
            }
        }

        [TestMethod]
        public void TryParsePlaylistId_Valid_Test()
        {
            string[] urls =
            {
                "https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
                "https://www.youtube.com/playlist?list=WL",
                "https://youtu.be/E2NZB6E5-do/?list=RDE2NZB6E5-do",
                "https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
            };
            string[] ids =
            {
                "PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
                "WL",
                "RDE2NZB6E5-do",
                "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
            };

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                string id = ids[i];
                bool success = YoutubeClient.TryParsePlaylistId(url, out string parsed);
                Assert.IsTrue(success);
                Assert.AreEqual(id, parsed);
            }
        }

        [TestMethod]
        public void TryParsePlaylistId_Invalid_Test()
        {
            string[] urls =
            {
                null,
                "",
                "https://www.youtube.com",
                "https://www.youtube.com/playlist?list=@pm!!Hv#Lmg",
                "https://www.youtube.co.il/watch?v=OpV62-86Fv4",
                "https://www.youtube.com/qweasd?list=Nsib94LHi9I"
            };

            foreach (string url in urls)
            {
                bool success = YoutubeClient.TryParsePlaylistId(url, out string parsed);
                Assert.IsFalse(success);
            }
        }
    }
}