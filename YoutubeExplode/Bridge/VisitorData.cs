using System;
using System.Linq;
using System.Text;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal static class VisitorData
{
    private static readonly Random Random = new();

    private static string GenerateRandomString(int length)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        var buffer = new StringBuilder();
        for (var i = 0; i < length; i++)
            buffer.Append(alphabet.ElementAt(Random.Next(alphabet.Length)));

        return buffer.ToString();
    }

    public static string Generate() =>
        new ProtoBuilder()
            .AddString(1, GenerateRandomString(11))
            .AddNumber(
                5,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - Random.Next(600000)
            )
            .AddBytes(
                6,
                new ProtoBuilder()
                    .AddString(1, "US")
                    .AddBytes(
                        2,
                        new ProtoBuilder()
                            .AddString(2, "")
                            .AddNumber(4, Random.Next(255) + 1)
                            .ToBytes()
                    )
                    .ToBytes()
            )
            .ToUrlEncodedBase64();
}
