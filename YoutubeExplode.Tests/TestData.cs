using System.Collections;
using NUnit.Framework;

namespace YoutubeExplode.Tests
{
    public static class TestData
    {
        // VIDEOS

        public static IEnumerable GetVideoUrls_Invalid()
        {
            yield return new TestCaseData("youtube.com/xxx?v=pI2I2zqzeKg");
            yield return new TestCaseData("youtu.be/watch?v=xxx");
            yield return new TestCaseData("youtube.com/embed/");
        }

        public static IEnumerable GetVideoUrls_Valid()
        {
            yield return new TestCaseData("youtube.com/watch?v=yIVRs6YSbOM", "yIVRs6YSbOM");
            yield return new TestCaseData("youtu.be/yIVRs6YSbOM", "yIVRs6YSbOM");
            yield return new TestCaseData("youtube.com/embed/yIVRs6YSbOM", "yIVRs6YSbOM");
        }

        public static IEnumerable GetVideoIds_Invalid()
        {
            yield return new TestCaseData("");
            yield return new TestCaseData("pI2I2zqzeK");
            yield return new TestCaseData("pI2I2z zeKg");
            yield return new TestCaseData("pI2I2zqzeKgz");
            yield return new TestCaseData("pI2I2zq=eKg");
        }

        public static IEnumerable GetVideoIds_Valid()
        {
            // Include nested
            foreach (var testCaseData in GetVideoIds_Valid_Unavailable())
                yield return testCaseData;
            foreach (var testCaseData in GetVideoIds_Valid_Available())
                yield return testCaseData;
        }

        public static IEnumerable GetVideoIds_Valid_Unavailable()
        {
            yield return new TestCaseData("qld9w0b-1ao"); // non-existing
        }

        public static IEnumerable GetVideoIds_Valid_Available()
        {
            // Include nested
            foreach (var testCaseData in GetVideoIds_Valid_Available_Unplayable())
                yield return testCaseData;
            foreach (var testCaseData in GetVideoIds_Valid_Available_Playable())
                yield return testCaseData;
        }

        public static IEnumerable GetVideoIds_Valid_Available_Unplayable()
        {
            yield return new TestCaseData("p3dDcKOFXQg"); // requires purchase
        }

        public static IEnumerable GetVideoIds_Valid_Available_Playable()
        {
            yield return new TestCaseData("9bZkp7q19f0"); // very popular
            yield return new TestCaseData("SkRSXFQerZs"); // age restricted (embed allowed)
            yield return new TestCaseData("hySoCSoH-g8"); // age restricted (embed not allowed)
            yield return new TestCaseData("_kmeFXjjGfk"); // embed not allowed (type 1)
            yield return new TestCaseData("MeJVWBSsPAY"); // embed not allowed (type 2)
            yield return new TestCaseData("5VGm0dczmHc"); // rating not allowed
            yield return new TestCaseData("ZGdLIwrGHG8"); // unlisted
            yield return new TestCaseData("H1O_-JVbl_k"); // very large video
            yield return new TestCaseData("NgTdNd5lkvY"); // controversial video
            yield return new TestCaseData("rsAAeyAr-9Y"); // recording of a live stream

            // Include nested
            foreach (var testCaseData in GetVideoIds_Valid_Available_Playable_WithClosedCaptions())
                yield return testCaseData;
        }

        public static IEnumerable GetVideoIds_Valid_Available_Playable_WithClosedCaptions()
        {
            yield return new TestCaseData("_QdPW8JrYzQ");
        }

        // PLAYLISTS

        public static IEnumerable GetPlaylistUrls_Invalid()
        {
            yield return new TestCaseData("youtube.com/playlist?lisp=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H");
            yield return new TestCaseData("youtube.com/playlist?list=asd");
            yield return new TestCaseData("youtube.com/");
        }

        public static IEnumerable GetPlaylistUrls_Valid()
        {
            yield return new TestCaseData("youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
                "PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H");
            yield return new TestCaseData("youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr",
                "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr");
            yield return new TestCaseData("youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr",
                "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr");
            yield return new TestCaseData("youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr",
                "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr");
            yield return new TestCaseData("youtube.com/watch?v=x2ZRoWQ0grU&list=RDEMNJhLy4rECJ_fG8NL-joqsg",
                "RDEMNJhLy4rECJ_fG8NL-joqsg");
        }

        public static IEnumerable GetPlaylistIds_Invalid()
        {
            yield return new TestCaseData("");
            yield return new TestCaseData("PLm_3vnTS-pvmZFuF L1Pyhqf8kTTYVKjW");
            yield return new TestCaseData("PLm_3vnTS-pvmZFuF3L1Pyhqf8kTTYVKjWzWefI32jU");
            yield return new TestCaseData("PLm_3vnTS-pvmZFuF3L=Pyhqf8kTTYVKjW");
        }

        public static IEnumerable GetPlaylistIds_Valid()
        {
            yield return new TestCaseData("PL601B2E69B03FAB9D"); // short??
            yield return new TestCaseData("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e"); // normal
            yield return new TestCaseData("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk"); // large
            yield return new TestCaseData("RD1hu8-y6fKg0"); // video mix
            yield return new TestCaseData("RDMMU-ty-2B02VY"); // my mix
            yield return new TestCaseData("RDEMNJhLy4rECJ_fG8NL-joqsg"); // music mix
            yield return new TestCaseData("ULl6WWX-BgIiE"); // channel video mix
            yield return new TestCaseData("UUTMt7iMWa7jy0fNXIktwyLA"); // user uploads
            yield return new TestCaseData("PUTMt7iMWa7jy0fNXIktwyLA"); // popular user uploads
            yield return new TestCaseData("OLAK5uy_lLeonUugocG5J0EUAEDmbskX4emejKwcM"); // music album
            yield return new TestCaseData("LLEnBXANsKmyj2r9xVyKoDiQ"); // liked
            yield return new TestCaseData("FLEnBXANsKmyj2r9xVyKoDiQ"); // favorites
        }

        // CHANNELS

        public static IEnumerable GetUserUrls_Invalid()
        {
            yield return new TestCaseData("https://www.youtube.com/user/P_roZD/"); // username cannot contain anything other than A-Z, a-z, 0-9
            yield return new TestCaseData("www.youtube.com/user/ProZD1234567890ABCDEF/"); // max allowed username is 20 character
            yield return new TestCaseData("youtube.com/user//asdaz");
            yield return new TestCaseData("https://www.example.com/user/ProZD/");
            yield return new TestCaseData("youtube.com/");
        }

        public static IEnumerable GetUserUrls_Valid()
        {
            yield return new TestCaseData("https://www.youtube.com/user/ProZD/", "ProZD");
            yield return new TestCaseData("http://www.youtube.com/user/ProZD/", "ProZD");
            yield return new TestCaseData("www.youtube.com/user/ProZD/", "ProZD");
            yield return new TestCaseData("youtube.com/user/ProZD/", "ProZD");
            yield return new TestCaseData("https://www.youtube.com/user/ProZD", "ProZD");
        }

        public static IEnumerable GetUsernames_Invalid()
        {
            yield return new TestCaseData("The_Tyrrr");
            yield return new TestCaseData("0123456789ABCDEFGHIJK"); // 21 characters
            yield return new TestCaseData("A1B2C3-");
            yield return new TestCaseData("=0123456789ABCDEF");
        }

        public static IEnumerable GetUsernames_Valid()
        {
            yield return new TestCaseData("TheTyrrr");
            yield return new TestCaseData("KannibalenRecords");
            yield return new TestCaseData("JClayton1994");
        }

        public static IEnumerable GetChannelIds_Invalid()
        {
            yield return new TestCaseData("");
            yield return new TestCaseData("UC3xnGqlcL3y-GXz5N3wiTJ");
            yield return new TestCaseData("UC3xnGqlcL y-GXz5N3wiTJQ");
            yield return new TestCaseData("UC3xnGqlcL3y-GXz5N3wiTJQz");
            yield return new TestCaseData("UC3xnG=lcL3y-GXz5N3wiTJQ");
        }

        public static IEnumerable GetChannelIds_Valid()
        {
            yield return new TestCaseData("UCEnBXANsKmyj2r9xVyKoDiQ"); // normal
        }

        public static IEnumerable GetChannelUrls_Invalid()
        {
            yield return new TestCaseData("youtube.com/?channel=UCUC3xnGqlcL3y-GXz5N3wiTJQ");
            yield return new TestCaseData("youtube.com/channel/asd");
            yield return new TestCaseData("youtube.com/");
        }

        public static IEnumerable GetChannelUrls_Valid()
        {
            yield return new TestCaseData("youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ", "UC3xnGqlcL3y-GXz5N3wiTJQ");
            yield return new TestCaseData("youtube.com/channel/UCkQO3QsgTpNTsOw6ujimT5Q", "UCkQO3QsgTpNTsOw6ujimT5Q");
            yield return new TestCaseData("youtube.com/channel/UCQtjJDOYluum87LA4sI6xcg", "UCQtjJDOYluum87LA4sI6xcg");
        }

        // SEARCH

        public static IEnumerable GetVideoSearchQueries()
        {
            yield return new TestCaseData("undead corporation megalomania");
            yield return new TestCaseData("imagine dragons natural");
        }
    }
}