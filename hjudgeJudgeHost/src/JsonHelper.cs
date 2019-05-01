using SpanJson;
using SpanJson.Resolvers;
using System;
using System.Text;

namespace hjudgeJudgeHost
{
    public static class JsonHelper
    {
        public class ConfigDefaultResolver<TSymbol> : ResolverBase<TSymbol, ConfigDefaultResolver<TSymbol>> where TSymbol : struct
        {
            public ConfigDefaultResolver() : base(new SpanJsonOptions
            {
                NullOption = NullOptions.IncludeNulls,
                NamingConvention = NamingConventions.OriginalCase,
                EnumOption = EnumOptions.Integer
            })
            {
            }
        }
        public static byte[] SerializeJson<T>(this T obj)
        {
            return JsonSerializer.Generic.Utf8.Serialize<T, ConfigDefaultResolver<byte>>(obj);
        }
        public static string SerializeJsonAsString<T>(this T obj)
        {
            return Encoding.UTF8.GetString(JsonSerializer.Generic.Utf8.Serialize<T, ConfigDefaultResolver<byte>>(obj));
        }
        public static T DeserializeJson<T>(this ReadOnlySpan<byte> data)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T, ConfigDefaultResolver<byte>>(data);
        }
        public static T DeserializeJson<T>(this byte[] data)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T, ConfigDefaultResolver<byte>>(data);
        }
        public static T DeserializeJson<T>(this string str)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T, ConfigDefaultResolver<byte>>(Encoding.UTF8.GetBytes(str));
        }
    }
}
