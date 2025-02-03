using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal static class VisitorData
{
    private static readonly Random Random = new();
    private static readonly char[] SessionIdAllowedChars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray();
    private static string _visitorData = string.Empty;

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

    public static async Task<string> ExtractFromYoutube(HttpClient http)
    {
        //avoid fetching visitor data for each request (?) could slow down the app
        if (!string.IsNullOrEmpty(_visitorData))
            return _visitorData;

        //use the same iOS user agent (?) idk if it's necessary
        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "com.google.ios.youtube/19.45.4 (iPhone16,2; U; CPU iOS 18_1_0 like Mac OS X; US)"
        );

        //request JSON format
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        var url = "https://www.youtube.com/sw.js_data";
        HttpResponseMessage response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string jsonString = await response.Content.ReadAsStringAsync();

        //Remove the prefix ")]}'"
        if (jsonString.StartsWith(")]}'"))
            jsonString = jsonString.Substring(4);

        using (JsonDocument doc = JsonDocument.Parse(jsonString))
        {
            JsonElement root = doc.RootElement;

            //jsonArray[0][2][0][0][13]
            var value = root[0]
                .EnumerateArray()
                .ElementAt(2)
                .EnumerateArray()
                .ElementAt(0)
                .EnumerateArray()
                .ElementAt(0)
                .EnumerateArray()
                .ElementAt(13)
                .GetString();

            if (value == null)
                throw new Exception("Failed to fetch visitor data");

            _visitorData = value;
        }
        return _visitorData;
    }
}
