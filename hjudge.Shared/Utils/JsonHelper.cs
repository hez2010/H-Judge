using System;
using System.Text;
using System.Text.Json;

namespace hjudge.Shared.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions camelOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            IgnoreNullValues = true
        };

        private static readonly JsonSerializerOptions pascalOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            AllowTrailingCommas = true,
            IgnoreNullValues = true
        };

        public static byte[] SerializeJson<T>(this T obj, bool camel = true)
        {
            return Encoding.UTF8.GetBytes(camel switch
            {
                true => JsonSerializer.Serialize<T>(obj, camelOptions),
                false => JsonSerializer.Serialize<T>(obj, pascalOptions)
            });
        }
        public static string SerializeJsonAsString<T>(this T obj, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Serialize<T>(obj, camelOptions),
                false => JsonSerializer.Serialize<T>(obj, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this ReadOnlySpan<byte> data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize<T>(data, camelOptions),
                false => JsonSerializer.Deserialize<T>(data, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this byte[] data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize<T>(data, camelOptions),
                false => JsonSerializer.Deserialize<T>(data, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this string str, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize<T>(str, camelOptions),
                false => JsonSerializer.Deserialize<T>(str, pascalOptions)
            };
        }

        public static object DeserializeJson(this ReadOnlySpan<byte> data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize(data, type, camelOptions),
                false => JsonSerializer.Deserialize(data, type, pascalOptions)
            };
        }
        public static object DeserializeJson(this byte[] data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize(data, type, camelOptions),
                false => JsonSerializer.Deserialize(data, type, pascalOptions)
            };
        }
        public static object DeserializeJson(this string str, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize(str, type, camelOptions),
                false => JsonSerializer.Deserialize(str, type, pascalOptions)
            };
        }
    }
}
