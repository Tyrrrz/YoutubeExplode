using System;
using System.Linq;
using System.Text;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal static class VisitorData
{
    private static readonly Random Random = new();
    private static readonly char[] SessionIdAllowedChars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray();

    public static string Generate() =>
        new ProtoBuilder()
            // Session ID
            .AddString(1, Random.NextString(SessionIdAllowedChars, 11))
            // Timestamp
            .AddNumber(
                5,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - Random.Next(600000)
            )
            // Country info
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
