using System.Security.Cryptography;

namespace YoutubeExplode.Utils;

internal static class Hash
{
    public static byte[] Compute(HashAlgorithm algorithm, byte[] data)
    {
        using (algorithm)
            return algorithm.ComputeHash(data);
    }
}