using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                WebPageTypeConverter.Singleton,
                StyleConverter.Singleton,
                TooltipConverter.Singleton,
                TextUnionConverter.Singleton,
                TextTextConverter.Singleton,
                RunTextEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}