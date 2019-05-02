using SpanJson;
using SpanJson.Resolvers;
using System;
using System.Text;

namespace hjudgeShared.Utils
{
    public static class JsonHelper
    {
        public class AspNetCoreDefaultResolver<TSymbol> : ResolverBase<TSymbol, AspNetCoreDefaultResolver<TSymbol>> where TSymbol : struct
        {
            public AspNetCoreDefaultResolver() : base(new SpanJsonOptions
            {
                NullOption = NullOptions.IncludeNulls,
                NamingConvention = NamingConventions.CamelCase,
                EnumOption = EnumOptions.Integer
            })
            {
            }
        }
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
        public static byte[] SerializeJson<T>(this T obj, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Generic.Utf8.Serialize<T, AspNetCoreDefaultResolver<byte>>(obj),
                false => JsonSerializer.Generic.Utf8.Serialize<T, ConfigDefaultResolver<byte>>(obj)
            };
        }
        public static string SerializeJsonAsString<T>(this T obj, bool camel = true)
        {
            return Encoding.UTF8.GetString(camel switch
            {
                true => JsonSerializer.Generic.Utf8.Serialize<T, AspNetCoreDefaultResolver<byte>>(obj),
                false => JsonSerializer.Generic.Utf8.Serialize<T, ConfigDefaultResolver<byte>>(obj)
            });
        }
        public static T DeserializeJson<T>(this ReadOnlySpan<byte> data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Generic.Utf8.Deserialize<T, AspNetCoreDefaultResolver<byte>>(data),
                false => JsonSerializer.Generic.Utf8.Deserialize<T, ConfigDefaultResolver<byte>>(data)
            };
        }
        public static T DeserializeJson<T>(this byte[] data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Generic.Utf8.Deserialize<T, AspNetCoreDefaultResolver<byte>>(data),
                false => JsonSerializer.Generic.Utf8.Deserialize<T, ConfigDefaultResolver<byte>>(data)
            };
        }
        public static T DeserializeJson<T>(this string str, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Generic.Utf8.Deserialize<T, AspNetCoreDefaultResolver<byte>>(Encoding.UTF8.GetBytes(str)),
                false => JsonSerializer.Generic.Utf8.Deserialize<T, ConfigDefaultResolver<byte>>(Encoding.UTF8.GetBytes(str))
            };
        }

        public static object DeserializeJson(this ReadOnlySpan<byte> data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.NonGeneric.Utf8.Deserialize<AspNetCoreDefaultResolver<byte>>(data, type),
                false => JsonSerializer.NonGeneric.Utf8.Deserialize<ConfigDefaultResolver<byte>>(data, type)
            };
        }
        public static object DeserializeJson(this byte[] data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.NonGeneric.Utf8.Deserialize<AspNetCoreDefaultResolver<byte>>(data, type),
                false => JsonSerializer.NonGeneric.Utf8.Deserialize<ConfigDefaultResolver<byte>>(data, type)
            };
        }
        public static object DeserializeJson(this string str, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.NonGeneric.Utf8.Deserialize<AspNetCoreDefaultResolver<byte>>(Encoding.UTF8.GetBytes(str), type),
                false => JsonSerializer.NonGeneric.Utf8.Deserialize<ConfigDefaultResolver<byte>>(Encoding.UTF8.GetBytes(str), type)
            };
        }
    }
}
