using System;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal class TooltipConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Tooltip) || t == typeof(Tooltip?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "Verified")
            {
                return Tooltip.Verified;
            }
            throw new Exception("Cannot unmarshal type Tooltip");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Tooltip)untypedValue;
            if (value == Tooltip.Verified)
            {
                serializer.Serialize(writer, "Verified");
                return;
            }
            throw new Exception("Cannot marshal type Tooltip");
        }

        public static readonly TooltipConverter Singleton = new TooltipConverter();
    }
}