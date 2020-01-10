using System;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal class StyleConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Style) || t == typeof(Style?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "BADGE_STYLE_TYPE_VERIFIED")
            {
                return Style.BadgeStyleTypeVerified;
            }
            throw new Exception("Cannot unmarshal type Style");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Style)untypedValue;
            if (value == Style.BadgeStyleTypeVerified)
            {
                serializer.Serialize(writer, "BADGE_STYLE_TYPE_VERIFIED");
                return;
            }
            throw new Exception("Cannot marshal type Style");
        }

        public static readonly StyleConverter Singleton = new StyleConverter();
    }
}