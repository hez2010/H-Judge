using System;
using System.Text;
using System.Text.Json.Serialization;

namespace hjudge.Shared.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions camelOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        private static readonly JsonSerializerOptions pascalOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            AllowTrailingCommas = true
        };

        public static byte[] SerializeJson<T>(this T obj, bool camel = true)
        {
            return Encoding.UTF8.GetBytes(camel switch
            {
                true => JsonSerializer.ToString<T>(obj, camelOptions),
                false => JsonSerializer.ToString<T>(obj, pascalOptions)
            });
        }
        public static string SerializeJsonAsString<T>(this T obj, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.ToString<T>(obj, camelOptions),
                false => JsonSerializer.ToString<T>(obj, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this ReadOnlySpan<byte> data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Parse<T>(data, camelOptions),
                false => JsonSerializer.Parse<T>(data, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this byte[] data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Parse<T>(data, camelOptions),
                false => JsonSerializer.Parse<T>(data, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this string str, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Parse<T>(str, camelOptions),
                false => JsonSerializer.Parse<T>(str, pascalOptions)
            };
        }

        public static object DeserializeJson(this ReadOnlySpan<byte> data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Parse(data, type, camelOptions),
                false => JsonSerializer.Parse(data, type, pascalOptions)
            };
        }
        public static object DeserializeJson(this byte[] data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Parse(data, type, camelOptions),
                false => JsonSerializer.Parse(data, type, pascalOptions)
            };
        }
        public static object DeserializeJson(this string str, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Parse(str, type, camelOptions),
                false => JsonSerializer.Parse(str, type, pascalOptions)
            };
        }
    }
}
