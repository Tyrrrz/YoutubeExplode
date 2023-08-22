using System.IO;

namespace YoutubeExplode.Converter.Tests.Utils;

internal static class FileEx
{
    public static bool ContainsBytes(string filePath, byte[] data)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        var referenceIndex = 0;

        while (stream.Position < stream.Length)
        {
            if (reader.ReadByte() == data[referenceIndex])
            {
                referenceIndex++;
            }
            else
            {
                referenceIndex = 0;
            }

            if (referenceIndex >= data.Length)
                return true;
        }

        return false;
    }
}
