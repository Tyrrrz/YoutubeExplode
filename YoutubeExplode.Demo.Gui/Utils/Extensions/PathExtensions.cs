using System.IO;

namespace YoutubeExplode.Demo.Gui.Utils.Extensions;

internal static class PathExtensions
{
    extension(Path)
    {
        public static string SanitizeFileName(string fileName, char replacement = '_')
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidChar, replacement);

            return fileName;
        }
    }
}
