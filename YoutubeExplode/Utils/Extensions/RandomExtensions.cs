using System;

namespace YoutubeExplode.Utils.Extensions;

internal static class RandomExtensions
{
    public static string NextString(this Random random, char[] allowedChars, int length)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
            buffer[i] = allowedChars[random.Next(allowedChars.Length)];

        return new string(buffer);
    }
}
