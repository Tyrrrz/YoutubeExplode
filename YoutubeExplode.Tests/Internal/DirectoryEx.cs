using System.IO;

namespace YoutubeExplode.Tests.Internal
{
    internal static class DirectoryEx
    {
        public static void DeleteIfExists(string dirPath, bool recursive = true)
        {
            try
            {
                Directory.Delete(dirPath, recursive);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        public static void Reset(string dirPath)
        {
            DeleteIfExists(dirPath);
            Directory.CreateDirectory(dirPath);
        }
    }
}