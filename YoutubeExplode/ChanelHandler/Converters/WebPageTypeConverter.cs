using System;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal class WebPageTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(WebPageType) || t == typeof(WebPageType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "WEB_PAGE_TYPE_BROWSE":
                    return WebPageType.WebPageTypeBrowse;
                case "WEB_PAGE_TYPE_SEARCH":
                    return WebPageType.WebPageTypeSearch;
                case "WEB_PAGE_TYPE_WATCH":
                    return WebPageType.WebPageTypeWatch;
            }
            throw new Exception("Cannot unmarshal type WebPageType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (WebPageType)untypedValue;
            switch (value)
            {
                case WebPageType.WebPageTypeBrowse:
                    serializer.Serialize(writer, "WEB_PAGE_TYPE_BROWSE");
                    return;
                case WebPageType.WebPageTypeSearch:
                    serializer.Serialize(writer, "WEB_PAGE_TYPE_SEARCH");
                    return;
                case WebPageType.WebPageTypeWatch:
                    serializer.Serialize(writer, "WEB_PAGE_TYPE_WATCH");
                    return;
            }
            throw new Exception("Cannot marshal type WebPageType");
        }

        public static readonly WebPageTypeConverter Singleton = new WebPageTypeConverter();
    }
}