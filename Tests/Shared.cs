using System.IO;

namespace YoutubeExplode.Tests
{
    public static class Shared
    {
        public static string TempDirectoryPath => Path.Combine(Directory.GetCurrentDirectory(), "Temp");
    }
}