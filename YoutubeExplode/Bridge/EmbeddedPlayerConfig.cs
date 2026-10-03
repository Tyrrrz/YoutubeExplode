using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonExtensions.Reading;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal sealed class EmbeddedPlayerConfig(JsonElement content)
{
    public JsonElement Context => content.GetProperty("INNERTUBE_CONTEXT");

    public string? UserAgent =>
        Context.GetPropertyOrNull("client")?.GetPropertyOrNull("userAgent")?.GetStringOrNull();

    public string ClientVersion =>
        Context.GetProperty("client").GetProperty("clientVersion").GetString()
        ?? throw new YoutubeExplodeException("Missing embedded player client version.");

    public string? VisitorData =>
        content.GetPropertyOrNull("VISITOR_DATA")?.GetStringOrNull()
        ?? Context.GetPropertyOrNull("client")?.GetPropertyOrNull("visitorData")?.GetStringOrNull();

    public string? SessionIndex => content.GetPropertyOrNull("SESSION_INDEX")?.ToString();

    public string? UserSessionId =>
        content.GetPropertyOrNull("USER_SESSION_ID")?.GetStringOrNull() ?? ParseDataSyncId().User;

    public string? DelegatedSessionId =>
        content.GetPropertyOrNull("DELEGATED_SESSION_ID")?.GetStringOrNull()
        ?? ParseDataSyncId().Delegated;

    public bool IsLoggedIn => content.GetPropertyOrNull("LOGGED_IN")?.GetBooleanOrNull() == true;

    public int? SignatureTimestamp => content.GetPropertyOrNull("STS")?.GetInt32OrNull();

    public string? PlayerSourceUrl =>
        content.GetPropertyOrNull("PLAYER_JS_URL")?.GetStringOrNull()
        ?? content
            .GetPropertyOrNull("WEB_PLAYER_CONTEXT_CONFIGS")
            ?.GetPropertyOrNull("WEB_PLAYER_CONTEXT_CONFIG_ID_EMBEDDED_PLAYER")
            ?.GetPropertyOrNull("jsUrl")
            ?.GetStringOrNull();

    public string? EncryptedHostFlags =>
        content
            .GetPropertyOrNull("WEB_PLAYER_CONTEXT_CONFIGS")
            ?.GetPropertyOrNull("WEB_PLAYER_CONTEXT_CONFIG_ID_EMBEDDED_PLAYER")
            ?.GetPropertyOrNull("encryptedHostFlags")
            ?.GetStringOrNull();

    private (string? Delegated, string? User) ParseDataSyncId()
    {
        var data = content.GetPropertyOrNull("DATASYNC_ID")?.GetStringOrNull();
        if (string.IsNullOrWhiteSpace(data))
            return (null, null);
        var parts = data.Split(new[] { "||" }, StringSplitOptions.None);
        return parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1])
            ? (parts[0], parts[1])
            : (null, parts[0]);
    }

    public static EmbeddedPlayerConfig Parse(string html)
    {
        foreach (Match match in Regex.Matches(html, @"ytcfg\.set\s*\(\s*(\{)"))
        {
            var config = Json.TryParse(Json.Extract(html[match.Groups[1].Index..]));
            if (config?.GetPropertyOrNull("INNERTUBE_CONTEXT") is not null)
                return new EmbeddedPlayerConfig(config.Value);
        }
        throw new YoutubeExplodeException("Failed to extract embedded player configuration.");
    }
}
