using SpanJson;
using SpanJson.AspNetCore.Formatter;
using System;
using System.Text;

namespace hjudgeWebHost.Utils
{
    public static class JsonHelper
    {
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
    }
}
