using System.IO;

namespace YoutubeExplode.Converter.Tests.Utils;

internal static class MediaFormat
{
    public static bool IsMp4File(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Skip 4 bytes
        stream.Seek(4, SeekOrigin.Current);

        // Expect: 66 74 79 70

        if (reader.ReadByte() != 0x66)
            return false;

        if (reader.ReadByte() != 0x74)
            return false;

        if (reader.ReadByte() != 0x79)
            return false;

        if (reader.ReadByte() != 0x70)
            return false;

        return true;
    }

    public static bool IsWebMFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Expect: 1A 45 DF A3

        if (reader.ReadByte() != 0x1A)
            return false;

        if (reader.ReadByte() != 0x45)
            return false;

        if (reader.ReadByte() != 0xDF)
            return false;

        if (reader.ReadByte() != 0xA3)
            return false;

        return true;
    }

    public static bool IsMp3File(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Assume ID3 container is present
        // Expect: 49 44 33

        if (reader.ReadByte() != 0x49)
            return false;

        if (reader.ReadByte() != 0x44)
            return false;

        if (reader.ReadByte() != 0x33)
            return false;

        return true;
    }

    public static bool IsOggFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Expect: 4F 67 67 53

        if (reader.ReadByte() != 0x4F)
            return false;

        if (reader.ReadByte() != 0x67)
            return false;

        if (reader.ReadByte() != 0x67)
            return false;

        if (reader.ReadByte() != 0x53)
            return false;

        return true;
    }
}