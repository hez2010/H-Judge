using SpanJson;
using SpanJson.AspNetCore.Formatter;
using System;
using System.Text;

namespace hjudgeWebHost.Utils
{
    public static class JsonHelper
    {
        public static byte[] SerializeJson<T>(this T obj)
        {
            return JsonSerializer.Generic.Utf8.Serialize<T, AspNetCoreDefaultResolver<byte>>(obj);
        }
        public static string SerializeJsonString<T>(this T obj)
        {
            return Encoding.UTF8.GetString(JsonSerializer.Generic.Utf8.Serialize<T, AspNetCoreDefaultResolver<byte>>(obj));
        }
        public static T DeserializeJson<T>(this ReadOnlySpan<byte> data)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T, AspNetCoreDefaultResolver<byte>>(data);
        }
        public static T DeserializeJson<T>(this byte[] data)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T, AspNetCoreDefaultResolver<byte>>(data);
        }
        public static T DeserializeJsonString<T>(this string str)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T, AspNetCoreDefaultResolver<byte>>(Encoding.UTF8.GetBytes(str));
        }
    }
}
