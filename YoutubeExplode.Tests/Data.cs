using System.Collections;
using NUnit.Framework;

namespace YoutubeExplode.Tests
{
    public static class Data
    {
        public static IEnumerable GetVideoIds()
        {
            yield return new TestCaseData("_QdPW8JrYzQ"); // normal
            yield return new TestCaseData("9bZkp7q19f0"); // very popular
            yield return new TestCaseData("SkRSXFQerZs"); // age restricted (embed allowed)
            yield return new TestCaseData("hySoCSoH-g8"); // age restricted (embed not allowed)
            yield return new TestCaseData("_kmeFXjjGfk"); // embed not allowed (type 1)
            yield return new TestCaseData("MeJVWBSsPAY"); // embed not allowed (type 2)
            yield return new TestCaseData("5VGm0dczmHc"); // rating not allowed
            yield return new TestCaseData("ZGdLIwrGHG8"); // unlisted
            yield return new TestCaseData("H1O_-JVbl_k"); // very large video
        }

        public static IEnumerable GetVideoIds_Invalid()
        {
            yield return new TestCaseData("");
            yield return new TestCaseData("pI2I2zqzeK");
            yield return new TestCaseData("pI2I2z zeKg");
            yield return new TestCaseData("pI2I2zqzeKgz");
            yield return new TestCaseData("pI2I2zq=eKg");
        }

        public static IEnumerable GetVideoIds_Unavailable()
        {
            yield return new TestCaseData("qld9w0b-1ao"); // non-existing
        }

        public static IEnumerable GetVideoIds_RequiresPurchase()
        {
            yield return new TestCaseData("p3dDcKOFXQg");
        }

        public static IEnumerable GetVideoIds_HasClosedCaptions()
        {
            yield return new TestCaseData("_QdPW8JrYzQ");
        }

        public static IEnumerable GetVideoUrls()
        {
            yield return new TestCaseData("youtube.com/watch?v=yIVRs6YSbOM", "yIVRs6YSbOM");
            yield return new TestCaseData("youtu.be/yIVRs6YSbOM", "yIVRs6YSbOM");
            yield return new TestCaseData("youtube.com/embed/yIVRs6YSbOM", "yIVRs6YSbOM");
        }

        public static IEnumerable GetVideoUrls_Invalid()
        {
            yield return new TestCaseData("youtube.com/xxx?v=pI2I2zqzeKg");
            yield return new TestCaseData("youtu.be/watch?v=xxx");
            yield return new TestCaseData("youtube.com/embed/");
        }

        public static IEnumerable GetPlaylistIds()
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
            yield return new TestCaseData("LLEnBXANsKmyj2r9xVyKoDiQ"); // liked
            yield return new TestCaseData("FLEnBXANsKmyj2r9xVyKoDiQ"); // favorites
        }

        public static IEnumerable GetPlaylistIds_Invalid()
        {
            yield return new TestCaseData("");
            yield return new TestCaseData("PLm_3vnTS-pvmZFuF L1Pyhqf8kTTYVKjW");
            yield return new TestCaseData("PLm_3vnTS-pvmZFuF3L1Pyhqf8kTTYVKjWz");
            yield return new TestCaseData("PLm_3vnTS-pvmZFuF3L=Pyhqf8kTTYVKjW");
        }

        public static IEnumerable GetPlaylistUrls()
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

        public static IEnumerable GetPlaylistUrls_Invalid()
        {
            yield return new TestCaseData("youtube.com/playlist?lisp=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H");
            yield return new TestCaseData("youtube.com/playlist?list=asd");
            yield return new TestCaseData("youtube.com/");
        }

        public static IEnumerable GetChannelIds()
        {
            yield return new TestCaseData("UCEnBXANsKmyj2r9xVyKoDiQ"); // normal
        }

        public static IEnumerable GetChannelIds_Invalid()
        {
            yield return new TestCaseData("");
            yield return new TestCaseData("UC3xnGqlcL3y-GXz5N3wiTJ");
            yield return new TestCaseData("UC3xnGqlcL y-GXz5N3wiTJQ");
            yield return new TestCaseData("UC3xnGqlcL3y-GXz5N3wiTJQz");
            yield return new TestCaseData("UC3xnG=lcL3y-GXz5N3wiTJQ");
        }

        public static IEnumerable GetChannelUrls()
        {
            yield return new TestCaseData("youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ", "UC3xnGqlcL3y-GXz5N3wiTJQ");
            yield return new TestCaseData("youtube.com/channel/UCkQO3QsgTpNTsOw6ujimT5Q", "UCkQO3QsgTpNTsOw6ujimT5Q");
            yield return new TestCaseData("youtube.com/channel/UCQtjJDOYluum87LA4sI6xcg", "UCQtjJDOYluum87LA4sI6xcg");
        }

        public static IEnumerable GetChannelUrls_Invalid()
        {
            yield return new TestCaseData("youtube.com/?channel=UCUC3xnGqlcL3y-GXz5N3wiTJQ");
            yield return new TestCaseData("youtube.com/channel/asd");
            yield return new TestCaseData("youtube.com/");
        }

        public static IEnumerable GetVideoSearchQueries()
        {
            yield return new TestCaseData("undead corporation megalomania");
            yield return new TestCaseData("white siberian fox");
        }
    }
}