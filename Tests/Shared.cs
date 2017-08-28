using System.IO;

namespace Tests
{
    public static class Shared
    {
        public static string TempDirectoryPath => Path.Combine(Directory.GetCurrentDirectory(), "Temp");
    }
}