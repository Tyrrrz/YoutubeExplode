using System.IO;

namespace YoutubeExplode.Demo.Gui.Utils;

internal static class PathEx
{
    public static string SanitizeFileName(string fileName, char replacement = '_')
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalidChar, replacement);

        return fileName;
    }
}
